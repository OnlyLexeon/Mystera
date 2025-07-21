using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Alchemy/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> ingredients;
    public Sprite icon;
    [TextArea] public string description;
    
    [Header("Settings")]
    public float defaultDuration = 5f;
    public float defaultFrequency = 5f;
    public float defaultIntensity = 5f;

    [Header("Potion")]
    public PotionType potionType;
    public Material liquidMaterial;
    public bool isShaderEffect; //shader effects dont stack

    [Header("Spawn Non Potion")]
    public bool isNonPotion;
    public GameObject objectToSpawn;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(recipeName))
        {
            string path = AssetDatabase.GetAssetPath(this);
            string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (currentName != recipeName)
            {
                AssetDatabase.RenameAsset(path, recipeName);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}
