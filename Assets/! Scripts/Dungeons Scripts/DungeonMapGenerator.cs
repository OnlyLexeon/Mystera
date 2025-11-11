using UnityEngine;
using System.Collections.Generic;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("Room Settings")]
    public GameObject spawnRoom;
    public GameObject exitRoom;
    public List<GameObject> confirmedRooms;
    public List<GameObject> emptyRooms;

    [Header("Room Settings")]
    public int minRooms = 5;
    public int maxRooms = 10;
    [Tooltip("Default: -0.5")] public float borderOffset = -0.5f;
    [Tooltip("Default: 50")] public int spawnAttempts = 100;

    [Header("Fail Safe Settings")]
    [Tooltip("Default: 10")] public int maxRegenerationAttempts = 10; //regenerate whole map if failed to have exit/spawn/confirmed rooms
    private int regenerationTries = 0;

    private List<DungeonRoom> spawnedRooms = new();
    private List<DungeonConnector> openConnectors = new();
    private List<int> confirmedRoomSpawnPoints = new();

    private int roomsPlaced = 0;
    private int maxTargetRooms;
    private Queue<(int roomIndex, GameObject prefab)> scheduledConfirmedRooms = new();


    public void Generate()
    {
        regenerationTries = 0;
        InternalGenerate();
    }

    private void InternalGenerate()
    {
        Debug.Log("==== Dungeon Generation Started ====");

        roomsPlaced = 0;
        maxTargetRooms = Random.Range(minRooms, maxRooms + 1);
        Debug.Log($"Target room count: {maxTargetRooms}");

        PrepareConfirmedRoomSchedule();
        SpawnInitialRoom();

        if (!TryAddRooms())
        {
            FailAndRetry("Room generation failed.");
            return;
        }

        if (!SpawnExitRoom())
        {
            FailAndRetry("Exit room failed to spawn.");
            return;
        }

        SealUnusedConnectors();

        Debug.Log("==== Dungeon Generation Complete ====");
    }

    void PrepareConfirmedRoomSchedule()
    {
        scheduledConfirmedRooms.Clear();
        int count = confirmedRooms.Count;
        int division = count + 1;

        for (int i = 0; i < count; i++)
        {
            int index = Mathf.RoundToInt(maxTargetRooms * (i + 1f) / division);
            scheduledConfirmedRooms.Enqueue((index, confirmedRooms[i]));
            Debug.Log($"Confirmed Room Scheduled: Index={index}, Prefab={confirmedRooms[i].name}");
        }
    }

    void SpawnInitialRoom()
    {
        GameObject start = Instantiate(spawnRoom, Vector3.zero, Quaternion.identity);
        DungeonRoom room = start.GetComponent<DungeonRoom>();
        spawnedRooms.Add(room);
        AddRoomConnectors(room);
    }

    bool TryAddRooms()
    {
        int safety = 0;

        while (roomsPlaced < maxTargetRooms && safety < spawnAttempts)
        {
            safety++;

            if (openConnectors.Count == 0)
            {
                if (!BacktrackLastRoom())
                    return false;
                continue;
            }

            List<DungeonConnector> availableConnectors = new(openConnectors);
            bool roomPlaced = false;

            while (availableConnectors.Count > 0)
            {
                int index = Random.Range(0, availableConnectors.Count);
                DungeonConnector targetConnector = availableConnectors[index];
                availableConnectors.RemoveAt(index);

                GameObject prefab = SelectNextRoomPrefab();
                GameObject roomObj = Instantiate(prefab);
                DungeonRoom newRoom = roomObj.GetComponent<DungeonRoom>();

                if (TryPlaceRoom(newRoom, targetConnector))
                {
                    spawnedRooms.Add(newRoom);
                    AddRoomConnectors(newRoom);
                    roomsPlaced++;
                    roomPlaced = true;
                    break;
                }
                Destroy(roomObj);
            }

            if (!roomPlaced && !BacktrackLastRoom())
                return false;
        }

        return roomsPlaced >= minRooms;
    }



    bool BacktrackLastRoom()
    {
        if (spawnedRooms.Count <= 1)
            return false;

        DungeonRoom last = spawnedRooms[^1];
        Debug.LogWarning($"Backtracking room: {last.name}");

        spawnedRooms.RemoveAt(spawnedRooms.Count - 1);

        foreach (var conn in last.connectors)
        {
            openConnectors.Remove(conn);
        }

        Destroy(last.gameObject);
        roomsPlaced--;
        return true;
    }

    GameObject SelectNextRoomPrefab()
    {
        if (scheduledConfirmedRooms.Count > 0 && scheduledConfirmedRooms.Peek().roomIndex == roomsPlaced)
        {
            var scheduled = scheduledConfirmedRooms.Dequeue();
            //Debug.Log($"Spawning scheduled confirmed room: {scheduled.prefab.name}");
            return scheduled.prefab;
        }

        var randomEmpty = emptyRooms[Random.Range(0, emptyRooms.Count)];
       // Debug.Log($"Spawning empty room: {randomEmpty.name}");
        return randomEmpty;
    }

    bool TryPlaceRoom(DungeonRoom newRoom, DungeonConnector targetConn)
    {
        foreach (var conn in newRoom.connectors)
        {
            //Debug.Log($"Trying {newRoom.name} connector {conn.name} -> Target {targetConn.name}");

            AlignRoom(newRoom, conn, targetConn);

            //"Force Unity to update collider bounds after rotation/position change"
            //basically, uh unity does not imediately update collider bounds, so after alignment, the bounds may not have changed.
            //enable and disable will force it to rebound ig?
            newRoom.boundsCollider.enabled = false;
            newRoom.boundsCollider.enabled = true;

            if (IsRoomValid(newRoom, targetConn.parentRoom))
            {
                conn.MarkUsed();
                targetConn.MarkUsed();
                openConnectors.Remove(targetConn);
                return true;
            }
        }

        return false;
    }

    void AlignRoom(DungeonRoom room, DungeonConnector roomConn, DungeonConnector targetConn)
    {
        Quaternion rot = Quaternion.LookRotation(-targetConn.transform.forward, Vector3.up);
        Quaternion delta = rot * Quaternion.Inverse(Quaternion.LookRotation(roomConn.transform.forward, Vector3.up));
        room.transform.rotation = delta * room.transform.rotation;

        Vector3 offset = targetConn.transform.position - roomConn.transform.position;
        room.transform.position += offset;
    }

    bool IsRoomValid(DungeonRoom room, DungeonRoom ignoreRoom)
    {
        Bounds b = room.GetWorldBounds();
        b.Expand(borderOffset); //offset

        foreach (var placed in spawnedRooms)
        {
            if (placed == ignoreRoom) continue;

            Bounds p = placed.GetWorldBounds();
            p.Expand(borderOffset);
            if (b.Intersects(p))
            {
                //Debug.LogWarning($"Room {room.name} intersects with {placed.name}");

                return false;
            }
        }
        return true;
    }

    void AddRoomConnectors(DungeonRoom room)
    {
        foreach (var conn in room.connectors)
        {
            if (!conn.used)
            {
                openConnectors.Add(conn);
            }

            if (conn.parentRoom == null)
                conn.parentRoom = room;
        }
    }

    bool SpawnExitRoom()
    {
        int attempts = 0;

        while (attempts < spawnAttempts)
        {
            if (openConnectors.Count == 0)
            {
                if (!BacktrackLastRoom())
                    return false;

                attempts++;
                continue;
            }

            List<DungeonConnector> availableConnectors = new(openConnectors);
            while (availableConnectors.Count > 0)
            {
                int index = Random.Range(0, availableConnectors.Count);
                DungeonConnector targetConn = availableConnectors[index];
                availableConnectors.RemoveAt(index);

                GameObject exitRoomObj = Instantiate(exitRoom);
                DungeonRoom exitScript = exitRoomObj.GetComponent<DungeonRoom>();

                foreach (var exitConn in exitScript.connectors)
                {
                    AlignRoom(exitScript, exitConn, targetConn);
                    exitScript.boundsCollider.enabled = false;
                    exitScript.boundsCollider.enabled = true;

                    if (IsRoomValid(exitScript, targetConn.parentRoom))
                    {
                        exitConn.MarkUsed();
                        targetConn.MarkUsed();
                        openConnectors.Remove(targetConn);
                        spawnedRooms.Add(exitScript);
                        return true;
                    }
                }

                Destroy(exitRoomObj);
            }

            attempts++;
        }

        return false;
    }



    void SealUnusedConnectors()
    {
        foreach (var room in spawnedRooms)
        {
            foreach (var conn in room.connectors)
            {
                if (!conn.used)
                conn.Seal();
            }
        }
    }

    void FailAndRetry(string reason)
    {
        Debug.LogWarning($"[DungeonGen] {reason} Retrying...");

        regenerationTries++;

        if (regenerationTries >= maxRegenerationAttempts)
        {
            Debug.LogError($"[DungeonGen] Failed after {maxRegenerationAttempts} attempts. Aborting.");
            return;
        }

        ClearDungeon();
        InternalGenerate();
    }

    public void KillAllMobs()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
            if (enemy != null) Destroy(enemy);
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        foreach (DungeonRoom room in spawnedRooms)
            if (room != null) Destroy(room.gameObject);

        KillAllMobs();

        spawnedRooms.Clear();
        openConnectors.Clear();
        confirmedRoomSpawnPoints.Clear();
        scheduledConfirmedRooms.Clear();
    }

    [ContextMenu("Regenerate Dungeon")]
    public void RegenerateDungeon()
    {
        ClearDungeon();

        Debug.Log("Dungeon cleared. Regenerating...");
        Generate();
    }
#endif

}


