using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShaderManager : MonoBehaviour
{
    public ScriptableRendererData rendererData;

    public static ShaderManager instance;

    void Awake()
    {
        instance = this;
    }

    public void ToggleFeature(string featureName, bool enabled)
    {
        if (rendererData == null)
        {
            Debug.LogError("RendererData not assigned!");
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null && feature.name == featureName)
            {
                feature.SetActive(enabled);
                Debug.Log($"{featureName} set to {enabled}");
                return;
            }
        }

        Debug.LogWarning($"Renderer feature '{featureName}' not found.");
    }
}
