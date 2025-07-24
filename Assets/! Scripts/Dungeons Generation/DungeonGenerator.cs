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

    private List<DungeonRoom> spawnedRooms = new();
    private List<DungeonConnector> openConnectors = new();
    private List<int> confirmedRoomSpawnPoints = new();

    private int roomsPlaced = 0;
    private int maxTargetRooms;

    public void Start()
    {
        Generate();
    }

    public void Generate()
    {
        roomsPlaced = 0;
        maxTargetRooms = Random.Range(minRooms, maxRooms + 1);
        PrepareConfirmedRoomSchedule();

        SpawnInitialRoom();
        TryAddRooms();
        SpawnExitRoom();
        SealUnusedConnectors();
    }

    void PrepareConfirmedRoomSchedule()
    {
        confirmedRoomSpawnPoints.Clear();

        int confirmedCount = confirmedRooms.Count;
        int division = confirmedCount + 1;

        for (int i = 1; i <= confirmedCount; i++)
        {
            int slot = Mathf.RoundToInt((float)maxTargetRooms * i / division);
            confirmedRoomSpawnPoints.Add(slot);
        }
    }

    void SpawnInitialRoom()
    {
        GameObject start = Instantiate(spawnRoom, Vector3.zero, Quaternion.identity);
        DungeonRoom room = start.GetComponent<DungeonRoom>();
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
                if (!BacktrackLastRoom())
                {
                    Debug.LogWarning("Backtracking failed. Dungeon generation halted.");
                    return;
                }

                continue;
            }

            DungeonConnector targetConnector = openConnectors[Random.Range(0, openConnectors.Count)];
            GameObject prefab = SelectNextRoomPrefab();
            GameObject roomObj = Instantiate(prefab);
            DungeonRoom newRoom = roomObj.GetComponent<DungeonRoom>();

            if (TryPlaceRoom(newRoom, targetConnector))
            {
                spawnedRooms.Add(newRoom);
                AddRoomConnectors(newRoom);
                roomsPlaced++;
            }
            else
            {
                Destroy(roomObj);
            }
        }
    }

    bool BacktrackLastRoom()
    {
        if (spawnedRooms.Count <= 1)
            return false; // Never delete spawn room

        DungeonRoom last = spawnedRooms[spawnedRooms.Count - 1];
        spawnedRooms.RemoveAt(spawnedRooms.Count - 1);

        // Remove its connectors from open list
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
        if (confirmedRoomSpawnPoints.Contains(roomsPlaced))
        {
            confirmedRoomSpawnPoints.Remove(roomsPlaced);
            return confirmedRooms[Random.Range(0, confirmedRooms.Count)];
        }

        return emptyRooms[Random.Range(0, emptyRooms.Count)];
    }

    bool TryPlaceRoom(DungeonRoom newRoom, DungeonConnector targetConn)
    {
        foreach (var conn in newRoom.connectors)
        {
            AlignRoom(newRoom, conn, targetConn);

            if (IsRoomValid(newRoom, targetConn.parentRoom))
            {
                conn.MarkUsed();
                targetConn.MarkUsed();
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
        foreach (var placed in spawnedRooms)
        {
            if (placed == ignoreRoom) continue;

            if (b.Intersects(placed.GetWorldBounds()))
                return false;
        }
        return true;
    }

    void AddRoomConnectors(DungeonRoom room)
    {
        foreach (var conn in room.connectors)
        {
            if (!conn.used)
                openConnectors.Add(conn);

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
                if (!BacktrackLastRoom())
                {
                    Debug.LogWarning("Failed to backtrack for exit room.");
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
                    spawnedRooms.Add(exitScript);
                    return;
                }
            }

            Destroy(exitRoomObj);
            attempts++;
        }

        Debug.LogWarning("Unable to spawn exit room after retries.");
    }

    void SealUnusedConnectors()
    {
        foreach (var room in spawnedRooms)
        {
            foreach (var conn in room.connectors)
            {
                conn.Seal();
            }
        }
    }
}
