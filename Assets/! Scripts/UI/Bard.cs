using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Bard : MonoBehaviour
{
    public GameObject musicparticles;

    [Header("Music Settings")]
    public AudioSource audioSource;
    public AudioClip[] playlist;
    private int currentTrackIndex = 0;

    public enum PlayMode { Loop, Shuffle }
    private PlayMode currentMode = PlayMode.Shuffle;

    [Header("UI")]
    public Button modeToggleButton;
    public Image modeIcon;
    public Image stateIcon;
    public TextMeshProUGUI modeLabel;
    public TextMeshProUGUI songName;

    public Sprite loopIcon;
    public Sprite shuffleIcon;
    public Sprite playIcon;
    public Sprite pauseIcon;

    public Button pauseButton;
    public Button skipButton;


    [Header("Settings")]
    public float minPlayerDistance = 2f;

    [Header("Events")]
    public UnityEvent onPlayerEnterRange;
    public UnityEvent onPlayerExitRange;

    private Transform player;
    private bool isInRange = false;
    private bool isPausedManually = false;

    private void Start()
    {
        player = Camera.main.transform; // assumes player head is main camera

        //Music
        UpdateModeUI();
        PlayTrack(currentTrackIndex);
        UpdatePauseIcon(); // Set the correct icon on start

        modeToggleButton.onClick.AddListener(TogglePlayMode);
        pauseButton.onClick.AddListener(PauseOrResume);
        skipButton.onClick.AddListener(NextTrack);
    }   

    
    private void Update()
    {
        //Distance Checking
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

        //MUSIC
        if (!audioSource.isPlaying && audioSource.clip != null && audioSource.time == 0f && !isPausedManually)
        {
            NextTrack();
        }

    }

    //========================================
    //Music
    private void NextTrack()
    {
        if (currentMode == PlayMode.Loop)
        {
            PlayTrack(currentTrackIndex); // replay same song
        }
        else // Shuffle
        {
            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, playlist.Length);
            } while (nextIndex == currentTrackIndex && playlist.Length > 1);

            PlayTrack(nextIndex);
        }

        isPausedManually = false;
        UpdatePauseIcon();
    }


    private void PlayTrack(int index)
    {
        if (playlist.Length == 0) return;

        currentTrackIndex = index % playlist.Length;
        audioSource.clip = playlist[currentTrackIndex];
        songName.text = audioSource.clip.name;
        audioSource.Play();
    }

    private void TogglePlayMode()
    {
        currentMode = currentMode == PlayMode.Loop ? PlayMode.Shuffle : PlayMode.Loop;

        UpdateModeUI();
    }

    private void UpdateModeUI()
    {
        if (currentMode == PlayMode.Loop)
        {
            modeLabel.text = "Loop";
            modeIcon.sprite = loopIcon;
        }
        else
        {
            modeLabel.text = "Shuffle";
            modeIcon.sprite = shuffleIcon;
        }
    }

    private void PauseOrResume()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPausedManually = true;
        }
        else
        {
            audioSource.Play();
            isPausedManually = false;
        }

        UpdatePauseIcon();
    }

    private void UpdatePauseIcon()
    {
        if (isPausedManually)
        {
            stateIcon.sprite = playIcon;

            musicparticles.SetActive(false);
        }
        else
        {
            stateIcon.sprite = pauseIcon;

            musicparticles.SetActive(true);
        }
    }

}
