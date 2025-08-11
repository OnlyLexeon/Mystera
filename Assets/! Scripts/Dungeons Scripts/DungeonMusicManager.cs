using UnityEngine;

public class DungeonMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float maxVolume = 1f;
    public float minVolume = 0f;

    [Tooltip("How fast battle/ambience ramps up (volume per second).")]
    public float battleFadeInPerSec = 1.2f;
    public float ambienceFadeInPerSec = 0.25f;

    [Tooltip("How fast we fade down to silence before swapping back to ambience.")]
    public float fadeOutPerSec = 1.0f;

    [Header("Detection Tick")]
    public float updateInterval = 0.1f;

    [Header("Enemy Detection Settings")]
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;
    public LayerMask obstructionLayers;
    public float loseSightGrace = 1.0f;

    [Header("Sounds")]
    public AudioClip[] dungeonAmbience;
    public AudioClip[] battleMusic;

    private Camera mainCamera;
    private float detectTimer;
    private bool contactLOS; // in sight + in distance
    private float lastSeenEnemyTime = -999f;

    private enum MusicState { Ambience, Battle, FadingOutToAmbience }
    private MusicState state = MusicState.Ambience;
    private bool isDungeon;

    public void OnSceneLoadCheck()
    {
        if (SceneController.instance.isDungeonScene()) isDungeon = true;
        else isDungeon = false;

        if (isDungeon)
        {
            audioSource.loop = true;
            audioSource.clip = GetRandomAmbience();
            audioSource.volume = 0f; // ambience will fade in
            state = MusicState.Ambience;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) return;
    }

    private void Update()
    {
        if (!isDungeon) return;

        if (mainCamera == null) mainCamera = Camera.main;
        if (audioSource == null || mainCamera == null) return;

        detectTimer += Time.deltaTime;
        if (detectTimer >= updateInterval)
        {
            detectTimer = 0f;
            bool visible = CheckForEnemiesInSight();
            if (visible) lastSeenEnemyTime = Time.time;
            contactLOS = visible || (Time.time - lastSeenEnemyTime) < loseSightGrace;
        }

        switch (state)
        {
            case MusicState.Battle:
                if (contactLOS)
                {
                    audioSource.volume = Move(audioSource.volume, maxVolume, battleFadeInPerSec);
                }
                else
                {
                    state = MusicState.FadingOutToAmbience;
                }
                break;

            case MusicState.FadingOutToAmbience:
                if (contactLOS)
                {
                    state = MusicState.Battle;
                    break;
                }

                audioSource.volume = Move(audioSource.volume, minVolume, fadeOutPerSec);

                if (audioSource.volume <= minVolume + 0.0001f)
                {
                    audioSource.clip = GetRandomAmbience();
                    audioSource.volume = minVolume; // start quiet
                    audioSource.Play();
                    state = MusicState.Ambience;
                }
                break;

            case MusicState.Ambience:
                audioSource.volume = Move(audioSource.volume, maxVolume, ambienceFadeInPerSec);

                if (contactLOS)
                {
                    if (!IsClipFromArray(audioSource.clip, battleMusic))
                    {
                        audioSource.clip = GetRandomBattle();
                        audioSource.Play();
                    }
                    state = MusicState.Battle;
                }
                break;
        }
    }

    // Smooth volume helper
    private float Move(float current, float target, float perSec)
    {
        if (perSec <= 0f) return target;
        return Mathf.MoveTowards(current, target, perSec * Time.deltaTime);
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

    private bool IsClipFromArray(AudioClip clip, AudioClip[] array)
    {
        if (!clip || array == null) return false;
        for (int i = 0; i < array.Length; i++)
            if (array[i] == clip) return true;
        return false;
    }

    //los
    private bool CheckForEnemiesInSight()
    {
        if (mainCamera == null) return false;

        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, detectionRadius, enemyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            var t = hits[i].transform;
            Vector3 dir = (t.position - mainCamera.transform.position);
            float dist = dir.magnitude;
            if (dist <= 0.001f) continue;

            dir /= dist; // normalize

            // Clear line of sight?
            if (!Physics.Raycast(mainCamera.transform.position, dir, dist, obstructionLayers))
                return true;
        }
        return false;
    }
}
