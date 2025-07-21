using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class Bed : MonoBehaviour
{
    public Button sleepButton;
    public FadeCanvas fadeCanvas;

    [Header("Settings")]
    public float minPlayerDistance = 2f;

    [Header("Events")]
    public UnityEvent onPlayerEnterRange;
    public UnityEvent onPlayerExitRange;

    private Transform player;
    private bool isInRange = false;

    public static Bed instance;

    private void Awake()
    {
        player = Camera.main.transform; // assumes player head is main camera

        instance = this;
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
    }

    public void EnableSleepButton()
    {
        sleepButton.gameObject.SetActive(true);
    }

    public void DisableSleepButton()
    {
        sleepButton.gameObject.SetActive(false);
    }

    public void SetTimeMorning()
    {
        sleepButton.interactable = false;
        StartCoroutine(MorningCoroutine());
    }

    public IEnumerator MorningCoroutine()
    {
        fadeCanvas.StartFadeIn();

        yield return new WaitForSeconds(fadeCanvas.defaultDuration);

        DayNightManager.instance.SetTimeMorning();

        fadeCanvas.StartFadeOut();
        sleepButton.interactable = true;
    }
}
