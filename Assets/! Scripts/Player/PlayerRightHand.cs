using UnityEngine;

public enum HandMaterial
{
    Normal,
    Midas,
}

public class PlayerRightHand : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public MeshRenderer handRenderer;
    public Material handMaterial;

    [Header("Midas Touch")]
    public bool isMidas = false;
    public Material midasGold;
    public LayerMask pickupsLayer;
    public Ingredient goldIngredient;

    public static PlayerRightHand instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isMidas) DoMidas(collision);
    }

    public void DoMidas(Collision collision)
    {
        GameObject target = collision.gameObject;

        //Already midased?
        if (target.GetComponent<MidasedObject>() != null) return; // Already transformed
        target.AddComponent<MidasedObject>();

        // 1. Replace all materials with midasGold
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material[] goldMats = new Material[meshRenderer.materials.Length];
            for (int i = 0; i < goldMats.Length; i++)
            {
                goldMats[i] = midasGold;
            }
            meshRenderer.materials = goldMats;
        }

        // 2. Check if target is in pickupsLayer
        if (((1 << target.layer) & pickupsLayer.value) != 0)
        {
            // Add IngredientObject if not already present
            IngredientObject ingredientObj = target.GetComponent<IngredientObject>();
            if (ingredientObj == null)
            {
                ingredientObj = target.AddComponent<IngredientObject>();
            }

            // Assign gold ingredient reference
            ingredientObj.ingredient = goldIngredient;
        }

        //3. Sound & Particle
        audioSource.PlayOneShot(PotionMaterialsManager.instance.midasSound);
        Instantiate(PotionMaterialsManager.instance.midasParticles, transform.position, Quaternion.identity);
    }

    public void SetHandMaterial(HandMaterial material)
    {
        Material[] mats = handRenderer.materials;

        switch (material)
        {
            case HandMaterial.Normal:
                mats[0] = handMaterial;
                break;
            case HandMaterial.Midas:
                mats[0] = midasGold;
                break;
            default:
                mats[0] = handMaterial;
                break;
        }

        handRenderer.materials = mats;
    }
}
