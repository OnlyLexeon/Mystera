using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DungeonSettings
{
    public string dungeonID;

    public int minRooms = 5;
    public int maxRooms = 10;
    public float borderOffset = -0.1f;
    public int spawnAttempts = 100;

    public GameObject spawnRoom;
    public GameObject exitRoom;
    public List<GameObject> confirmedRooms;
    public List<GameObject> emptyRooms;
}

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance { get; private set; }

    public DungeonSettings[] dungeonSettings;

    private string selectedDungeonID = "";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GenerateDungeon(string dungeonID)
    {
        var generator = FindFirstObjectByType<DungeonMapGenerator>();
        if (generator == null)
        {
            Debug.LogWarning("No DungeonMapGenerator found in scene.");
            return;
        }

        var settings = GetSettingsByID(dungeonID);
        if (settings == null)
        {
            Debug.LogError($"No DungeonSettings found for ID: {dungeonID}");
            return;
        }

        ApplySettings(generator, settings);
        generator.Generate();
    }

    private DungeonSettings GetSettingsByID(string id)
    {
        foreach (var settings in dungeonSettings)
        {
            if (settings.dungeonID == id)
                return settings;
        }
        return null;
    }

    private void ApplySettings(DungeonMapGenerator generator, DungeonSettings settings)
    {
        generator.minRooms = settings.minRooms;
        generator.maxRooms = settings.maxRooms;
        generator.borderOffset = settings.borderOffset;
        generator.spawnAttempts = settings.spawnAttempts;

        generator.spawnRoom = settings.spawnRoom;
        generator.exitRoom = settings.exitRoom;
        generator.confirmedRooms = new List<GameObject>(settings.confirmedRooms);
        generator.emptyRooms = new List<GameObject>(settings.emptyRooms);
    }
}
