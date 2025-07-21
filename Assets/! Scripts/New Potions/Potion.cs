using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class Potion : MonoBehaviour
{
    public Recipe recipe;
    public PotionType type;

    public bool isVelocity = false;
    public bool isSelected = false;

    [Header("Reference")]
    public AudioSource audioSource;
    public MeshRenderer meshRenderer;

    [Header("Dynamic Ground Mask")]
    public LayerMask groundMask;

    [Header("Debugging Only (Set at Recipe)")]
    public bool isDrank = false;
    public bool isCorkRemoved = false;
    public float duration;
    public float intensity;
    public float frequency;

    public void SetSelected(bool state)
    {
        isSelected = state;
    }

    public void SetVelocity(bool state)
    {
        isVelocity = state;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && isVelocity)
        {
            // Ignore collisions with objects tagged "Ladel"
            if (collision.gameObject.CompareTag("Ladel"))
                return;

            // Check if the collision object is on the dynamic groundMask
            if ((groundMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                // Collision happened with a ground object
                DestroyPotion();
            }
        }
    }


    public void Consume()
    {
        if (isDrank) return;

        SetDrank();
        ClearLiquid();
        PlayDrinkSound();

        OnConsume();

        //consume before marking as empty
        type = PotionType.Empty;
        recipe = null;
    }

    public void OnConsume()
    {
        EffectManager.instance.DoPotionEffect(recipe, duration, intensity, frequency);
    }

    public void RemoveCork()
    {
        if (isCorkRemoved) return;

        SetCorkClear();

        isCorkRemoved = true; // no cork

        PlayCorkPopSound();
    }
    //=====================================

    protected void ClearLiquid()
    {
        Material[] mats = meshRenderer.materials;
        if (mats.Length > 1)
        {
            mats[1] = PotionMaterialsManager.instance.clear;
            meshRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning("ClearLiquid: MeshRenderer does not have a second material slot.");
        }
    }

    public void SetLiquid(Material mat)
    {
        Material[] mats = meshRenderer.materials;
        if (mats.Length > 1)
        {
            mats[1] = mat;
            meshRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning("ClearLiquid: MeshRenderer does not have a second material slot.");
        }
    }

    public void SetCorkClear()
    {
        Material[] mats = meshRenderer.materials;
        if (mats.Length > 0)
        {
            mats[0] = PotionMaterialsManager.instance.clear;
            meshRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning("SetCorkClear: MeshRenderer has no material slots.");
        }
    }

    public void SetCork()
    {
        Material[] mats = meshRenderer.materials;
        if (mats.Length > 0)
        {
            mats[0] = PotionMaterialsManager.instance.cork;
            meshRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning("SetCorkClear: MeshRenderer has no material slots.");
        }
    }

    protected void SetDrank()
    {
        isDrank = true;
    }

    protected void PlayDrinkSound()
    {
        audioSource.PlayOneShot(PotionMaterialsManager.instance.drinkSound);
    }

    protected void PlayCorkPopSound()
    {
        audioSource.PlayOneShot(PotionMaterialsManager.instance.corkRemoveSound);
    }

    //=====================================
    protected void DestroyPotion()
    {
        //instantiate particles NOT AS CHILD
        //...
        //particles should play the sound since this potion is destroyed
        Instantiate(PotionMaterialsManager.instance.glassParticles, transform.position, Quaternion.identity);

        DoDestroy();
        //destroy
        Destroy(gameObject);
    }

    private void DoDestroy()
    {
        float radius = intensity;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        switch (type)
        {
            case PotionType.Fire:
                
                //Spwan combust
                GameObject combustParticles = Instantiate(PotionMaterialsManager.instance.combustParticles, transform.position, Quaternion.identity);
                combustParticles.transform.localScale = new Vector3(radius, radius, radius);

                //do fire
                foreach (Collider col in hitColliders)
                {
                    FlamableObject flammable = col.GetComponent<FlamableObject>();
                    if (flammable != null)
                    {
                        flammable.SetOnFire();
                    }
                }
                break;

            case PotionType.Glow:

                foreach (Collider col in hitColliders)
                {
                    XRGrabInteractable isGrabbale = col.GetComponent<XRGrabInteractable>();
                    if (isGrabbale != null)
                    {
                        GlowableObject glowable = col.GetComponent<GlowableObject>();
                        if (glowable == null)
                        {
                            glowable = col.AddComponent<GlowableObject>();
                        }  
                        
                        glowable.ApplyGlow(frequency, radius, duration);
                    }
                }
                break;

            case PotionType.Levitation:

                foreach (Collider col in hitColliders)
                {
                    XRGrabInteractable isGrabbale = col.GetComponent<XRGrabInteractable>();
                    if (isGrabbale != null)
                    {
                        LevitatableObject levitate = col.GetComponent<LevitatableObject>();
                        if (levitate == null)
                        {
                            levitate = col.AddComponent<LevitatableObject>();
                        }

                        levitate.ApplyLevitate(frequency, duration);
                    }
                }
                break;

            case PotionType.Duplication:
                //sound
                Instantiate(PotionMaterialsManager.instance.cloneParticles, transform.position, Quaternion.identity);

                List<GameObject> targetsToClone = new List<GameObject>();

                foreach (Collider col in hitColliders)
                {
                    XRGrabInteractable isGrabbable = col.GetComponent<XRGrabInteractable>();
                    if (isGrabbable != null && col.gameObject != gameObject && !targetsToClone.Contains(col.gameObject)) // prevent duplicating self
                    {
                        targetsToClone.Add(col.gameObject);
                    }
                }

                // Now instantiate clones
                foreach (GameObject target in targetsToClone)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.25f, 0.5f), Random.Range(-0.2f, 0.2f));
                    GameObject clones = Instantiate(target, target.transform.position + randomOffset, Quaternion.identity);

                    GameObject cloneParticles = Instantiate(PotionMaterialsManager.instance.duplicatonParticles, clones.transform.position, Quaternion.identity);
                }
                break;
        }
    }
    //=====================================
}
