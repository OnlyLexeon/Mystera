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
        if (!hasEnemy || enemySpawnPoints.Length == 0 || enemyToSpawn.Length == 0)
            return;

        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            GameObject selectedEnemy = GetRandomEnemyPrefab();
            if (selectedEnemy != null)
            {
                Instantiate(selectedEnemy, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    public Bounds GetWorldBounds()
    {
        return boundsCollider.bounds;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;
        foreach (var setting in enemyToSpawn)
            totalWeight += setting.weightage;

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (var setting in enemyToSpawn)
        {
            cumulative += setting.weightage;
            if (randomValue <= cumulative)
                return setting.enemyPrefab;
        }

        // Fallback
        return enemyToSpawn[enemyToSpawn.Length - 1].enemyPrefab;
    }
}
