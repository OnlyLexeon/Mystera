using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HatInventoryManager : MonoBehaviour
{
    public static HatInventoryManager Instance;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int MaxSlots = 15;
    
    private string savePath => Path.Combine(Application.persistentDataPath, "inventory.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        slots.Add(new InventorySlot(storable.itemID, 1));
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

        public InventorySlot(string id, int count)
        {
            itemID = id;
            stackCount = count;
        }
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<InventorySlot> slots;
    }
}
