using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParchmentPaper : MonoBehaviour
{
    public GameObject cloudParticlesPrefab;
    public float spawnDelay = 2;
    public Rigidbody rb;
    public float upwardForce;

    private List<DrawableLine> drawLines = new List<DrawableLine>();
    private bool isComplete = false;
    private FlamableObject flamableObject;

    [Header("Ingredient to Spawn")]
    public GameObject ingredientPrefab;

    private void Start()
    {
        flamableObject = GetComponent<FlamableObject>();

        // Find and assign all child DrawLine components
        drawLines.AddRange(GetComponentsInChildren<DrawableLine>());

        foreach (var line in drawLines)
        {
            line.SetParchmentPaper(this);
        }
    }

    private void Update()
    {
        if (isComplete)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, upwardForce, rb.linearVelocity.z);
        }
    }

    public void CheckIfCompleted()
    {
        if (isComplete) return;

        foreach (var line in drawLines)
        {
            if (!line.drawn)
                return;
        }

        // All lines drawn
        CompleteParchment();
    }

    private void CompleteParchment()
    {
        if (isComplete) return;

        TutorialParchmentComplete tutorialComplete = GetComponent<TutorialParchmentComplete>();
        if (tutorialComplete != null)
        {
            tutorialComplete.TaskComplete();
            Debug.Log("Task completed!");
        }

            // Start the particle and spawn sequence
            StartCoroutine(SpawnIngredientAfterDelay());

        isComplete = true;
    }

    private IEnumerator SpawnIngredientAfterDelay()
    {
        // Spawn smoke particles
        if (cloudParticlesPrefab != null)
        {
            Instantiate(cloudParticlesPrefab, transform.position, Quaternion.identity);
        }

        // Wait before spawning ingredient
        yield return new WaitForSeconds(spawnDelay);

        if (ingredientPrefab != null)
        {
            Instantiate(ingredientPrefab, transform.position, Quaternion.identity);
        }

        //Set parchment on fire
        flamableObject.SetOnFire();
    }
}
