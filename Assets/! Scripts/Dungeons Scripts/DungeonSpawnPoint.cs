using UnityEngine;

public class DungeonSpawnPoint : MonoBehaviour
{
    public Transform spawnPos;

    public static DungeonSpawnPoint instance;

    private void Awake()
    {
        instance = this;
    }

    public Vector2 GetSpawnPosition()
    {
        return spawnPos.position;
    }
}
