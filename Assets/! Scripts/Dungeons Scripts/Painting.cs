using UnityEngine;
using UnityEngine.UI;

public class Painting : MonoBehaviour
{
    public MeshRenderer paintingRenderer;
    public bool isHorizontal = false;

    [Header("Variants")]
    public Texture[] paintingTextures;

    private void Start()
    {
        if (paintingTextures.Length == 0) return;

        var index = Random.Range(0, paintingTextures.Length);
        paintingRenderer.material.mainTexture = paintingTextures[index];

        if (isHorizontal)
        {
            // Rotate UVs 90 degrees counter-clockwise
            paintingRenderer.material.mainTextureScale = new Vector2(1, 1);
            paintingRenderer.material.mainTextureOffset = new Vector2(0, 0);
            paintingRenderer.material.SetTextureScale("_MainTex", new Vector2(1, 1));
            paintingRenderer.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
            paintingRenderer.material.SetFloat("_Rotation", 90f); // Only works if shader supports rotation
        }
    }
}
