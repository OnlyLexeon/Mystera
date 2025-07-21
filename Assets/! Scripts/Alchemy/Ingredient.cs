using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Alchemy/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientID; // Unique name for the ingredient
    public Sprite sprite;
    [TextArea] public string description;

    [Header("Parchment")]
    public GameObject parchment;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(ingredientID))
        {
            string path = AssetDatabase.GetAssetPath(this);
            string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (currentName != ingredientID)
            {
                AssetDatabase.RenameAsset(path, ingredientID);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}
