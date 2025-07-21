using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Pot : MonoBehaviour
{
    [Header("UI")]
    public PotUIManager potUI; 

    [Header("Stirring")]
    public int timesToStir;
    public int stirredTimes = 0;
    public float stirTime;
    public bool canStir = false;
    public float stirProgress = 0;

    [Header("Sounds")]
    public AudioClip ingredientAddSound;
    public AudioClip ingredientRejectSound;
    public AudioClip potBoiling;
    public AudioClip stirSound;
    public AudioClip bottledSound;
    public AudioClip potCleansed;

    [Header("Specific Ingredients")]
    public Ingredient durationMulter;
    public Ingredient effectMulter;
    public Ingredient nullRoot;

    [Header("Settings")]
    public float rejectForce = 5f;
    public float durationMultIncrement = 1.5f;
    public float effectMultIncrement = 0.25f;
    public float maxDurationMult = 6f;
    public float maxEffectMult = 3f;

    [Header("References")]
    public GameObject potionPrefab;
    public AudioSource audioSource;
    public AudioSource loopingAudioSource;
    public Transform spawnPosition;
    public MeshRenderer meshRenderer;
    public ParticleSystem bubblesParticle;
    public GameObject smokeParticle;
    public GameObject spawnParticle;
    public Material water;

    [Header("Debug")]
    public List<Ingredient> currentIngredients = new List<Ingredient>();
    public IngredientObject ingredientToAdd;
    public float durationMultiplier = 1f;
    public float effectMultiplier = 1f;

    public bool canDipEmptyBottle = false;
    public Recipe currentRecipe;
    public Recipe matchedRecipe;

    public static Pot instance;

    [Header("Tutorial Stuff")]
    public bool hasAddedIngredient = false;

    private void Awake()
    {
        instance = this;
    }

    private bool CheckIfBuffIngredient(Ingredient ingredient)
    {
        //see if its duration enhancer
        if (ingredient == durationMulter)
        {
            if (durationMultiplier >= maxDurationMult)
            {
                RejectIngredient();
            }
            else
            {
                Debug.Log("Duration Enhanced");
                durationMultiplier += durationMultIncrement;
                durationMultiplier = Mathf.Min(durationMultiplier, maxDurationMult);

                PlaySound(ingredientAddSound);
                PlayBubbleParticles();
                potUI.UpdateTime();
                potUI.timeBar.SetActive(true);

                Destroy(ingredientToAdd.gameObject);
            }

            return true;
        }
        else if (ingredient == effectMulter)
        {
            if (effectMultiplier >= maxEffectMult)
            {
                RejectIngredient();
            }
            else
            {
                Debug.Log("Effect Enhanced");
                effectMultiplier += effectMultIncrement;
                effectMultiplier = Mathf.Min(effectMultiplier, maxEffectMult);

                PlaySound(ingredientAddSound);
                PlayBubbleParticles();
                potUI.UpdateEffect();
                potUI.effectBar.SetActive(true);

                Destroy(ingredientToAdd.gameObject);
            }

            return true;
        }
        else if (ingredient == nullRoot)
        {
            PlaySound(potCleansed);
            PlayBubbleParticles();
            Destroy(ingredientToAdd.gameObject);

            ResetPot();

            return true;
        }

        return false;
    }

    public void PlayBubbleParticles()
    {
        bubblesParticle.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        ingredientToAdd = other.GetComponent<IngredientObject>();
        if (ingredientToAdd != null)
        {
            TryAddIngredient(ingredientToAdd.ingredient);
        }


        //EMPTY POTION BOTTLE
        Potion potionToSet = other.GetComponent<Potion>();
        if (canDipEmptyBottle && potionToSet != null && potionToSet.type == PotionType.Empty)
        {
            Debug.Log("Dipped!");

            Destroy(potionToSet.gameObject);
            SpawnPotion();

            ResetPot();
        }
        
    }

    public void ResetPot()
    {
        //Get Pot Mesh
        Material[] mats = meshRenderer.materials;
        mats[0] = water;
        meshRenderer.materials = mats;

        canDipEmptyBottle = false;
        currentRecipe = null;

        //Hide stir bar
        potUI.stirBar.SetActive(false);
        canStir = false;
        stirProgress = 0f;
        stirredTimes = 0;
        smokeParticle.gameObject.SetActive(false);

        durationMultiplier = 1;
        effectMultiplier = 1;
        currentIngredients.Clear();

        matchedRecipe = null;

        //UI
        potUI.UpdateTime();
        potUI.UpdateEffect();
        potUI.UpdateStir();
        potUI.timeBar.SetActive(false);
        potUI.effectBar.SetActive(false);

        //Sound
        loopingAudioSource.Stop();
    }

    private void OnTriggerStay(Collider other)
    {
        var stirrer = other.GetComponent<PotStirrer>();

        if (stirrer != null)
        {
            if (canStir && stirrer.isStirring)
            {
                if (stirProgress <= stirTime)
                {
                    stirProgress += Time.deltaTime;
                }

                if (stirProgress >= stirTime)
                {
                    stirredTimes++;
                    stirProgress = 0f;

                    PlaySound(stirSound);
                }

                if (stirredTimes >= timesToStir)
                {
                    canStir = false;
                    potUI.stirBar.SetActive(false);
                    smokeParticle.gameObject.SetActive(false);

                    potUI.effectBar.SetActive(false);
                    potUI.timeBar.SetActive(false);

                    MakePotion(matchedRecipe);
                    
                    potUI.ClearIcons();

                    stirredTimes = 0;
                }

                //Update UI
                potUI.UpdateStir();
            }
        }
    }

    private void TryAddIngredient(Ingredient ingredient)
    {
        if (CheckIfBuffIngredient(ingredient))
        {
            return;
        }

        //testlist = current ingredients in pot + to be added ingredient
        List<Ingredient> testList = new List<Ingredient>(currentIngredients) { ingredient };
        //match? if not, null
        Recipe match = RecipeManager.Instance.FindMatchingRecipe(testList);

        //not null?
        if (match != null) //added successfully 
        {
            //ADDED
            potUI.AddIcon(ingredient);

            //Particles
            PlayBubbleParticles();
            //Sound
            PlaySound(ingredientAddSound);

            currentIngredients.Add(ingredient);
            Debug.Log("Ingredient added: " + ingredient.ingredientID);
            //Destroy added ingredient
            Destroy(ingredientToAdd.gameObject);

            //tutorial
            hasAddedIngredient = true;

            //check if the recipe is now complete
            if (RecipeManager.Instance.IsRecipeComplete(match.ingredients, currentIngredients))
            {
                matchedRecipe = match;
                canStir = true;
                potUI.stirBar.SetActive(true);
                PlayLoopingSound(potBoiling);
                smokeParticle.gameObject.SetActive(true);
            }
        }
        else
        {
            RejectIngredient();
        }
    }

    private void RejectIngredient()
    {
        Debug.Log("Rejected ingredient!");

        // Sound
        PlaySound(ingredientRejectSound);

        // DO BOING AWAY LOGIC
        Rigidbody rb = ingredientToAdd.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Direction away from pot
            Vector3 direction = (ingredientToAdd.transform.position - transform.position).normalized;

            // Add upward component
            Vector3 upwardBoost = Vector3.up; // You can tweak this value

            // Final combined direction
            Vector3 forceDirection = (direction + upwardBoost).normalized;

            rb.AddForce(forceDirection * rejectForce, ForceMode.Impulse);
        }
    }


    public void MakePotion(Recipe recipe)
    {
        currentRecipe = recipe;

        if (recipe.isNonPotion)
        {
            SpawnObject();
            ResetPot();
        }
        else
        {
            canDipEmptyBottle = true;

            //Get Pot Mesh
            Material[] mats = meshRenderer.materials;
            mats[0] = currentRecipe.liquidMaterial;
            meshRenderer.materials = mats;
        }
    }

    public void SpawnPotion()
    {
        //cloud burse
        Instantiate(spawnParticle, spawnPosition.position, Quaternion.identity);

        PlaySound(bottledSound);

        Debug.Log("Filled");

        GameObject potionSpawned = Instantiate(potionPrefab, spawnPosition.position, Quaternion.identity);

        Potion potionScript = potionSpawned.GetComponent<Potion>();

        potionScript.isDrank = false;
        potionScript.isCorkRemoved = false;

        potionScript.recipe = currentRecipe;
        potionScript.type = currentRecipe.potionType;
        potionScript.SetLiquid(currentRecipe.liquidMaterial);
        potionScript.SetCork();
        //set defaults
        potionScript.duration = currentRecipe.defaultDuration;
        potionScript.intensity = currentRecipe.defaultIntensity;
        potionScript.frequency = currentRecipe.defaultFrequency;
        //duration
        potionScript.duration *= durationMultiplier;
        //effect
        potionScript.intensity *= effectMultiplier;
        potionScript.frequency *= effectMultiplier;
    }

    public void SpawnObject()
    {
        //cloud burse
        Instantiate(spawnParticle, spawnPosition.position, Quaternion.identity);

        GameObject objectSpawned = Instantiate(currentRecipe.objectToSpawn, spawnPosition.position, Quaternion.identity);

        //ensure it floats so it doesnt fall back into pot
        Rigidbody rb = objectSpawned.GetComponent<Rigidbody>();
        if (rb !=null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }

    public void PlayLoopingSound(AudioClip sound)
    {
        loopingAudioSource.clip = sound;
        loopingAudioSource.Play();
    }
}
