using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public float borderOffset = -0.1f;

    private List<DungeonRoom> spawnedRooms = new();
    private List<DungeonConnector> openConnectors = new();
    private List<int> confirmedRoomSpawnPoints = new();

    private int roomsPlaced = 0;
    private int maxTargetRooms;
    private Queue<(int roomIndex, GameObject prefab)> scheduledConfirmedRooms = new();

    public void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Debug.Log("==== Dungeon Generation Started ====");

        roomsPlaced = 0;
        maxTargetRooms = Random.Range(minRooms, maxRooms + 1);
        Debug.Log($"Target room count: {maxTargetRooms}");

        PrepareConfirmedRoomSchedule();
        SpawnInitialRoom();
        TryAddRooms();
        SpawnExitRoom();
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
        Debug.Log($"Spawned Spawn Room: {start.name}");
        spawnedRooms.Add(room);
        AddRoomConnectors(room);
    }

    void TryAddRooms()
    {
        int safety = 0;

        while (roomsPlaced < maxTargetRooms && safety < 200)
        {
            safety++;

            if (openConnectors.Count == 0)
            {
                Debug.LogWarning("No open connectors. Attempting backtrack...");
                if (!BacktrackLastRoom())
                {
                    Debug.LogError("Backtracking failed. Dungeon generation halted.");
                    return;
                }
                continue;
            }

            DungeonConnector targetConnector = openConnectors[Random.Range(0, openConnectors.Count)];
            GameObject prefab = SelectNextRoomPrefab();
            Debug.Log($"Trying to spawn room: {prefab.name} at RoomIndex={roomsPlaced}");

            GameObject roomObj = Instantiate(prefab);
            DungeonRoom newRoom = roomObj.GetComponent<DungeonRoom>();

            if (TryPlaceRoom(newRoom, targetConnector))
            {
                Debug.Log($"Placed room: {newRoom.name} at RoomIndex={roomsPlaced}");
                spawnedRooms.Add(newRoom);
                AddRoomConnectors(newRoom);
                roomsPlaced++;
            }
            else
            {
                Debug.LogWarning($"Failed to place room: {prefab.name}, destroying...");
                Destroy(roomObj);
            }
        }
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
            Debug.Log($"Spawning scheduled confirmed room: {scheduled.prefab.name}");
            return scheduled.prefab;
        }

        var randomEmpty = emptyRooms[Random.Range(0, emptyRooms.Count)];
        Debug.Log($"Spawning empty room: {randomEmpty.name}");
        return randomEmpty;
    }

    bool TryPlaceRoom(DungeonRoom newRoom, DungeonConnector targetConn)
    {
        foreach (var conn in newRoom.connectors)
        {
            Debug.Log($"Trying {newRoom.name} connector {conn.name} -> Target {targetConn.name}");

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

                Debug.Log($"Connected {newRoom.name} to {targetConn.parentRoom.name} via connector.");
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
                Debug.LogWarning($"Room {room.name} intersects with {placed.name}");

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
                Debug.Log($"Added connector from {room.name} to openConnectors.");
            }

            if (conn.parentRoom == null)
                conn.parentRoom = room;
        }
    }

    void SpawnExitRoom()
    {
        int attempts = 0;
        while (attempts < 10)
        {
            if (openConnectors.Count == 0)
            {
                Debug.LogWarning("No open connectors for exit room. Attempting backtrack...");
                if (!BacktrackLastRoom())
                {
                    Debug.LogError("Failed to backtrack for exit room.");
                    return;
                }
                attempts++;
                continue;
            }

            DungeonConnector conn = openConnectors[Random.Range(0, openConnectors.Count)];
            GameObject exitRoomObj = Instantiate(exitRoom);
            DungeonRoom exitScript = exitRoomObj.GetComponent<DungeonRoom>();

            foreach (var exitConn in exitScript.connectors)
            {
                AlignRoom(exitScript, exitConn, conn);

                if (IsRoomValid(exitScript, conn.parentRoom))
                {
                    exitConn.MarkUsed();
                    conn.MarkUsed();
                    openConnectors.Remove(conn);
                    spawnedRooms.Add(exitScript);
                    Debug.Log($"Exit room placed successfully at connector from {conn.parentRoom.name}");
                    return;
                }
            }

            Destroy(exitRoomObj);
            attempts++;
        }

        Debug.LogError("Unable to spawn exit room after retries.");
    }

    void SealUnusedConnectors()
    {
        foreach (var room in spawnedRooms)
        {
            foreach (var conn in room.connectors)
            {
                if (!conn.used)
                    Debug.Log($"Sealing unused connector on {room.name}");
                conn.Seal();
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Dungeon")]
    public void RegenerateDungeon()
    {
        // Destroy previously spawned rooms
        foreach (DungeonRoom room in spawnedRooms)
        {
            if (room != null)
                DestroyImmediate(room.gameObject);
        }

        // Clean up state
        spawnedRooms.Clear();
        openConnectors.Clear();
        confirmedRoomSpawnPoints.Clear();
        scheduledConfirmedRooms.Clear();

        Debug.Log("Dungeon cleared. Regenerating...");
        Generate();
    }
#endif

}


