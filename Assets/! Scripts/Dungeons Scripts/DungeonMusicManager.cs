using UnityEngine;

public class DungeonMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float updateInterval = 0.1f;
    public float volumeChangeStep = 0.05f;
    public float maxVolume = 1f;
    public float minVolume = 0f;

    [Header("Enemy Detection Settings")]
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;
    public LayerMask obstructionLayers;
    public float loseSightGrace = 1.0f;

    [Header("Sounds")]
    public AudioClip[] dungeonAmbience;
    public AudioClip[] battleMusic;

    private Camera mainCamera;
    private float timer;
    private float currentVolume = 0f;
    private bool inBattle = false;
    private float lastSeenEnemyTime = -999f;

    private readonly Collider[] _hits = new Collider[32];

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            // start in ambience
            audioSource.clip = GetAmbience();
            audioSource.Play();
            currentVolume = maxVolume; // ambience full
            audioSource.volume = currentVolume;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || audioSource == null) return;

        bool enemyVisible = CheckForEnemiesInSight();
        if (enemyVisible) lastSeenEnemyTime = Time.time;

        bool shouldBeBattle = enemyVisible || (Time.time - lastSeenEnemyTime) < loseSightGrace;
        
        if (shouldBeBattle && !inBattle)
        {
            inBattle = true;
            currentVolume = minVolume;
            audioSource.clip = GetRandomBattle();
            audioSource.Play();
        }
        else if (!shouldBeBattle && inBattle)
        {
            inBattle = false;
            currentVolume = minVolume;
            audioSource.clip = GetRandomAmbience();
            audioSource.Play();
        }

        //ramp volume
        currentVolume = Mathf.MoveTowards(currentVolume, maxVolume, volumeChangeStep);
        audioSource.volume = Mathf.Clamp(currentVolume, minVolume, maxVolume);
    }

    private AudioClip GetRandomAmbience()
    {
        if (dungeonAmbience == null || dungeonAmbience.Length == 0) return null;
        return dungeonAmbience[Random.Range(0, dungeonAmbience.Length)];
    }

    private AudioClip GetRandomBattle()
    {
        if (battleMusic == null || battleMusic.Length == 0) return null;
        return battleMusic[Random.Range(0, battleMusic.Length)];
    }

    private bool CheckForEnemiesInSight()
    {
        if (mainCamera == null) return false;

        //enemies
        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, detectionRadius, enemyLayer);

        foreach (var hit in hits)
        {
            Vector3 dirToEnemy = (hit.transform.position - mainCamera.transform.position).normalized;
            float distToEnemy = Vector3.Distance(mainCamera.transform.position, hit.transform.position);

            //line of sight
            if (!Physics.Raycast(mainCamera.transform.position, dirToEnemy, distToEnemy, obstructionLayers))
            {
                return true; //found 1, stops
            }
        }

        return false;
    }
}
