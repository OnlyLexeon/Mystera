using UnityEngine;

public class PotionMaterialsManager : MonoBehaviour
{
    public static PotionMaterialsManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Bottle")]
    public Material cork;
    public Material clear;

    [Header("Shaders")]
    public Material nausea;

    [Header("Particles")]
    public GameObject glassParticles;
    public GameObject combustParticles;
    public GameObject cloneParticles;
    public GameObject duplicatonParticles;
    public GameObject midasParticles;

    [Header("Sounds")]
    public AudioClip drinkSound;
    public AudioClip corkRemoveSound;
    public AudioClip midasSound;
}
