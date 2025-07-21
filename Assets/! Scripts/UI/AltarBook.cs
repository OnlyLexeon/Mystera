using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AltarBook : MonoBehaviour
{
    [Header("Parchment Spawning")]
    public GameObject cloudParticle;
    public Transform spawnPos;
    public AudioSource spawnAudioSource;

    [Header("Settings")]
    public float minPlayerDistance = 2f;

    [Header("Events")]
    public UnityEvent onPlayerEnterRange;
    public UnityEvent onPlayerExitRange;

    private Transform player;
    private bool isInRange = false;

    public static AltarBook instance;

    [Header("Tutorial Stuff")]
    public bool hasSpawned = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        player = Camera.main.transform; // assumes player head is main camera
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool currentlyInRange = distance <= minPlayerDistance;

        if (currentlyInRange && !isInRange)
        {
            isInRange = true;
            onPlayerEnterRange.Invoke();
        }
        else if (!currentlyInRange && isInRange)
        {
            isInRange = false;
            onPlayerExitRange.Invoke();
        }
    }

    public void DoSpawn(GameObject parchment)
    {
        Instantiate(cloudParticle, spawnPos.position, Quaternion.identity);

        spawnAudioSource.Play();

        GameObject spawnedParchment = Instantiate(parchment, spawnPos.position, Quaternion.identity);
        if (TutorialManager.instance.hasCompletedAParchment == false || !hasSpawned)
        {
            hasSpawned = true;
            TutorialParchmentComplete parchMentToComplete = spawnedParchment.AddComponent<TutorialParchmentComplete>();
            TutorialManager.instance.SubscribeParchmentCompleteEvent(parchMentToComplete);
        }
    }
}
