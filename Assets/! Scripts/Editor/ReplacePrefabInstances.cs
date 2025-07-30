using UnityEditor;
using UnityEngine;

public class ReplacePrefabInstances : EditorWindow
{
    GameObject oldPrefab;
    GameObject newPrefab;

    [MenuItem("Tools/Replace Prefab Instances")]
    public static void ShowWindow() => GetWindow<ReplacePrefabInstances>("Replace Prefabs");

    void OnGUI()
    {
        oldPrefab = (GameObject)EditorGUILayout.ObjectField("Old Prefab", oldPrefab, typeof(GameObject), false);
        newPrefab = (GameObject)EditorGUILayout.ObjectField("New Prefab", newPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Replace All"))
        {
            ReplaceAllInstances();
        }
    }

    void ReplaceAllInstances()
    {
        var instances = FindObjectsOfType<GameObject>();

        foreach (var go in instances)
        {
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefabAsset == oldPrefab)
            {
                var newGO = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                newGO.transform.position = go.transform.position;
                newGO.transform.rotation = go.transform.rotation;
                newGO.transform.localScale = go.transform.localScale;

                Undo.RegisterCreatedObjectUndo(newGO, "Replaced Prefab");
                Undo.DestroyObjectImmediate(go);
            }
        }
    }
}
