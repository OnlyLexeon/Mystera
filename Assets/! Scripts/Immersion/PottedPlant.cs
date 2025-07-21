using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PottedPlant : MonoBehaviour
{
    [Header("Growth Settings")]
    public Transform[] growSpots;
    public GameObject plantToGrow;
    public float growthTime = 5f;

    [Header("References")]
    private AudioSource audioSource;
    public AudioClip plantPluck;
    public AudioClip plantGrown;

    [Header("Debug")]
    public float[] growthTimers;
    public GameObject[] currentPlants;

    private void Awake()
    {
        growthTimers = new float[growSpots.Length];
        currentPlants = new GameObject[growSpots.Length];

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        for (int i = 0; i < growSpots.Length; i++)
        {
            GrowPlant(i, false);
        }
    }

    private void Update()
    {
        for (int i = 0; i < growSpots.Length; i++)
        {
            XRSocketInteractor socket = growSpots[i].GetComponent<XRSocketInteractor>();

            bool hasSelection = socket != null && socket.hasSelection;

            if (!hasSelection)
            {
                growthTimers[i] += Time.deltaTime;

                if (growthTimers[i] >= growthTime)
                {
                    GrowPlant(i, true);
                    growthTimers[i] = 0f;
                }

                // Clear the old plant reference
                currentPlants[i] = null;
            }
            else
            {
                growthTimers[i] = 0f;
            }
        }
    }



    private void GrowPlant(int index, bool playSound)
    {
        if (playSound) PlaySound(plantGrown);

        GameObject newPlant = Instantiate(plantToGrow, growSpots[index].position, growSpots[index].rotation);

        // Apply random Y-axis rotation
        float randomY = Random.Range(0f, 360f);
        newPlant.transform.rotation = Quaternion.Euler(0f, randomY, 0f);

        newPlant.transform.SetParent(growSpots[index]);
        currentPlants[index] = newPlant;

        // Try to get XRSocketInteractor from the grow spot
        XRSocketInteractor socket = growSpots[index].GetComponent<XRSocketInteractor>();
        XRGrabInteractable interactable = newPlant.GetComponent<XRGrabInteractable>();

        if (socket != null && interactable != null)
        {
            // Manually select the new plant so the socket "holds" it
            socket.StartManualInteraction((IXRSelectInteractable)interactable);
        }

        Rigidbody rb = newPlant.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public void Fertilize()
    {
        for (int i = 0; i < growSpots.Length; i++)
        {
            if (currentPlants[i] == null)
            {
                GrowPlant(i, true);
                growthTimers[i] = 0f; // Reset just in case
            }
        }
    }

    public void Pluck(int index)
    {
        if (index < 0 || index >= growSpots.Length) return;

        if (currentPlants[index] != null)
        {
            //Debug.Log("I been plucked!");

            //PlaySound(plantPluck);

            GameObject plucked = currentPlants[index];
            plucked.transform.SetParent(null); // Detach from the pot
            currentPlants[index] = null;
            growthTimers[index] = 0f; // Start regrowth timer
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}