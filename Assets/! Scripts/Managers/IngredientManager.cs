using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IngredientsManager : MonoBehaviour
{
    public static IngredientsManager instance;

    [Header("Master Ingredient List")]
    public List<Ingredient> allIngredients;

    [Header("Default Unlocked Ingredients")]
    public List<Ingredient> defaultUnlocked;

    private HashSet<string> unlockedIDs = new HashSet<string>();
    private string savePath => Path.Combine(Application.persistentDataPath, "unlockedIngredients.json");

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadUnlockedIngredients();
    }

    public bool IsUnlocked(string id)
    {
        return unlockedIDs.Contains(id);
    }

    public void UnlockIngredient(string id)
    {
        if (!unlockedIDs.Contains(id))
        {
            unlockedIDs.Add(id);
            SaveUnlockedIngredients();
        }
    }

    public void SaveUnlockedIngredients()
    {
        var data = new UnlockedData { unlockedIDs = new List<string>(unlockedIDs) };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadUnlockedIngredients()
    {
        unlockedIDs.Clear();
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<UnlockedData>(json);
            foreach (var id in data.unlockedIDs)
                unlockedIDs.Add(id);
        }

        foreach (var ingre in defaultUnlocked)
        {
            if (ingre != null)
                unlockedIDs.Add(ingre.ingredientID);
        }
    }

    [System.Serializable]
    public class UnlockedData
    {
        public List<string> unlockedIDs;
    }
}
