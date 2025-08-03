using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class DungeonSettings
{
    public string dungeonID;
    public Sprite dungeonIcon;
    public bool unlocked = false;

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
    public string defaultDungeonID = "default";

    private HashSet<string> unlockedDungeons = new();
    private string savePath => Path.Combine(Application.persistentDataPath, "dungeons.json");

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadUnlockedDungeons();
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

    //UNLOCKED DUNGEONS MODULE

    [System.Serializable]
    public class DungeonSaveData
    {
        public List<string> unlockedDungeonIDs = new();
    }

    public bool IsUnlocked(string dungeonID)
    {
        return unlockedDungeons.Contains(dungeonID) || dungeonID == defaultDungeonID;
    }

    public void UnlockDungeon(string dungeonID)
    {
        if (!unlockedDungeons.Contains(dungeonID))
        {
            unlockedDungeons.Add(dungeonID);
            SaveUnlockedDungeons();
            Debug.Log($"Unlocked dungeon: {dungeonID}");
        }
    }

    private void SaveUnlockedDungeons()
    {
        DungeonSaveData data = new() { unlockedDungeonIDs = new List<string>(unlockedDungeons) };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadUnlockedDungeons()
    {
        unlockedDungeons.Clear();

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            DungeonSaveData data = JsonUtility.FromJson<DungeonSaveData>(json);
            unlockedDungeons = new HashSet<string>(data.unlockedDungeonIDs);
        }

        // Always allow the default dungeon
        if (!unlockedDungeons.Contains(defaultDungeonID))
            unlockedDungeons.Add(defaultDungeonID);
    }

    [ContextMenu("Clear Saved Dungeons")]
    public void ClearSavedDungeons()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        unlockedDungeons.Clear();
        unlockedDungeons.Add(defaultDungeonID);
    }
}
