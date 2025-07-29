using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HatInventoryManager : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Clear Saved Inventory")]
    private void Editor_ClearInventory()
    {
        ClearSavedInventory();
    }
#endif


    public static HatInventoryManager instance;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots = 15;
    public int maxStack = 9;

    private HatInventoryUI ui;

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

    private void Start()
    {
        if (ui == null) ui = FindFirstObjectByType<HatInventoryUI>();
    }

    public bool TryAddItem(GameObject item)
    {
        StorableObject storable = item.GetComponent<PotionStorable>() ?? item.GetComponent<StorableObject>();
        if (storable == null) return false;

        IStorableData customData = null;
        var potionStore = item.GetComponent<PotionStorable>();
        if (potionStore != null)
            customData = potionStore.GetPotionData();

        //adding
        if (slots.Count >= maxSlots) return false;

        //try to stack
        foreach (var slot in slots)
        {
            if (slot.itemID != storable.itemID)
                continue;

            //If potion, try to match with existing
            if (customData is StoredPotionData newPotion && slot.GetDeserializedData() is StoredPotionData existingPotion)
            {
                if (ArePotionsIdentical(existingPotion, newPotion))
                {
                    if (slot.stackCount >= maxStack) continue;
                    else
                    {
                        slot.stackCount++;
                        SaveInventory();
                        ui?.RefreshUI();
                        return true;
                    }
                }
            }
            //no potion
            else if (customData == null && string.IsNullOrEmpty(slot.jsonData))
            {
                if (slot.stackCount >= maxStack) continue;
                else
                {
                    slot.stackCount++;
                    SaveInventory();
                    ui?.RefreshUI();
                    return true;
                }
            }
        }

        slots.Add(new InventorySlot(storable.itemID, 1, customData));
        SaveInventory();
        return true;
    }

    private bool ArePotionsIdentical(StoredPotionData a, StoredPotionData b)
    {
        if (a == null || b == null) return false;

        return a.recipeID == b.recipeID &&
               Mathf.Approximately(a.duration, b.duration) &&
               Mathf.Approximately(a.intensity, b.intensity) &&
               Mathf.Approximately(a.frequency, b.frequency) &&
               a.isDrank == b.isDrank &&
               a.isCorkRemoved == b.isCorkRemoved;
    }


    public bool TryRemoveItem(string itemID, IStorableData data = null)
    {
        for (int i = slots.Count - 1; i >= 0 ; i--)
        {
            var slot = slots[i];

            if (slot.itemID != itemID)
                continue;

            //if custom data
            if (data is StoredPotionData toRemovePotion &&
                slot.GetDeserializedData() is StoredPotionData slotPotion)
            {
                if (!ArePotionsIdentical(slotPotion, toRemovePotion))
                    continue;
            }
            else if (data != null || !string.IsNullOrEmpty(slot.jsonData))
            {
                //no match
                continue;
            }

            //removing
            slot.stackCount--;
            if (slot.stackCount <= 0)
                slots.RemoveAt(i);

            SaveInventory();
            return true;
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

    public void ClearSavedInventory()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Inventory save file deleted.");
        }

        slots.Clear();

        ui?.RefreshUI();
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
