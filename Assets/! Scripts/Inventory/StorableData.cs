using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Storable Database Entry")]
public class StorableData : ScriptableObject
{
    public string itemID;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(itemID))
        {
            string path = AssetDatabase.GetAssetPath(this);
            string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (currentName != itemID)
            {
                AssetDatabase.RenameAsset(path, itemID);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}

