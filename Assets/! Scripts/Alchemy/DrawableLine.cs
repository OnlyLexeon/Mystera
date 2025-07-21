using UnityEngine;

public class DrawableLine : MonoBehaviour
{
    public bool drawn = false;
    public Material drawnMaterial;

    private MeshRenderer meshRenderer;
    private ParchmentPaper parchmentPaper;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetParchmentPaper(ParchmentPaper paper)
    {
        parchmentPaper = paper;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (drawn) return;

        if (other.CompareTag("Draw"))
        {
            drawn = true;
            Material[] mats = meshRenderer.materials;
            mats[0] = drawnMaterial;
            meshRenderer.materials = mats;


            // Notify the parchment to check if it's completed
            if (parchmentPaper != null)
            {
                parchmentPaper.CheckIfCompleted();
            }
        }
    }
}
