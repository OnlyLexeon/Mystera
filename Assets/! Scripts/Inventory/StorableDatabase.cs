using System.Collections.Generic;
using UnityEngine;

public class StorableDatabase : MonoBehaviour
{
    public static StorableDatabase Instance;

    public List<StorableData> entries;

    private Dictionary<string, StorableData> dataMap;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        dataMap = new Dictionary<string, StorableData>();
        foreach (var data in entries)
        {
            if (!dataMap.ContainsKey(data.itemID))
                dataMap.Add(data.itemID, data);
        }
    }

    public StorableData GetDataByID(string id)
    {
        return dataMap.TryGetValue(id, out var data) ? data : null;
    }
}
