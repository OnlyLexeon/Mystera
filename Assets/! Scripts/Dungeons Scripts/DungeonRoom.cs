using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum RoomType
{
    Spawn,
    Exit,
    Confirmed,
    Empty
}

public class EnemySetting
{
    public float weightage;
    public GameObject enemyPrefab;
}

public class DungeonRoom : MonoBehaviour
{
    public RoomType roomType;
    public List<DungeonConnector> connectors;
    public BoxCollider boundsCollider;

    [Header("Enemy Settings")]
    public bool hasEnemy = false;
    public Transform[] enemySpawnPoints;
    public EnemySetting[] enemyToSpawn;

    private void Start()
    {
        foreach (Transform spawnPoint in enemySpawnPoints)
        {

        }
    }

    public Bounds GetWorldBounds()
    {
        return boundsCollider.bounds;
    }
}
