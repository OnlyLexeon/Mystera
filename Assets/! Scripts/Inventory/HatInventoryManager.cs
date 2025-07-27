using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HatInventoryManager : MonoBehaviour
{
    public static HatInventoryManager instance;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int MaxSlots = 15;
    
    private string savePath => Path.Combine(Application.persistentDataPath, "inventory.json");

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventory();
    }

    public bool TryAddItem(GameObject item)
    {
        StorableObject storable = item.GetComponent<StorableObject>();
        if (storable == null) return false;

        //stacking
        foreach (var slot in slots)
        {
            if (slot.itemID == storable.itemID)
            {
                slot.stackCount++;
                SaveInventory();
                return true;
            }
        }

        //checking maximum
        if (slots.Count >= MaxSlots) return false;

        //data type shit
        IStorableData customData = null;
        var potionStore = item.GetComponent<PotionStorable>();
        if (potionStore != null)
            customData = potionStore.GetPotionData();

        slots.Add(new InventorySlot(storable.itemID, 1, customData));
        SaveInventory();
        return true;
    }

    //remove 1 item from stack
    public bool TryRemoveItem(string itemID)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemID == itemID)
            {
                slots[i].stackCount--;
                if (slots[i].stackCount <= 0)
                    slots.RemoveAt(i);

                SaveInventory();
                return true;
            }
        }
        return false;
    }

    public List<InventorySlot> GetInventory() => new List<InventorySlot>(slots);

    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData { slots = slots };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
        slots = data.slots ?? new List<InventorySlot>();
    }

    [System.Serializable]
    public class InventorySlot
    {
        public string itemID;
        public int stackCount;

        public string jsonData;
        public string dataType;

        public InventorySlot(string id, int count, IStorableData data = null)
        {
            itemID = id;
            stackCount = count;

            if (data != null)
            {
                jsonData = JsonUtility.ToJson(data);
                dataType = data.GetType().Name;
            }
        }

        public IStorableData GetDeserializedData()
        {
            if (string.IsNullOrEmpty(jsonData) || string.IsNullOrEmpty(dataType))
                return null;

            switch (dataType)
            {
                case nameof(StoredPotionData):
                    return JsonUtility.FromJson<StoredPotionData>(jsonData);

                default:
                    Debug.LogWarning($"Unsupported data type: {dataType}");
                    return null;
            }
        }
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<InventorySlot> slots;
    }
}
