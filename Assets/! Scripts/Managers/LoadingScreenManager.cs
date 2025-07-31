using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager instance;

    public float lerpSpeed = 2.5f;

    [Header("References")]
    public GameObject loadingUI;
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;

    [Header("Fade")]
    public float defaultDuration = 1.5f;
    public Coroutine CurrentRoutine { private set; get; } = null;
    
    private float alpha = 0.0f;
    private float targetProgress = 0f;

    private void Awake()
    {
        instance = this;
    }

    public void Show()
    {
        loadingUI.SetActive(true);
        StartFadeIn();
        UpdateProgress(0f);
    }

    public void Hide()
    {
        StartFadeOut();
        loadingUI.SetActive(false);
    }

    public void UpdateProgress(float value)
    {
        text.text = "Progress: " + value + "%";
    }

    public void StartFadeIn()
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeIn(defaultDuration));
    }

    public void StartFadeOut()
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeOut(defaultDuration));
    }

    private IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0.0f;

        alpha = 0;

        while (alpha <= 1.0f)
        {
            SetAlpha(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0.0f;

        alpha = 1;

        while (alpha >= 0.0f)
        {
            SetAlpha(1 - (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void SetAlpha(float value)
    {
        alpha = value;
        canvasGroup.alpha = alpha;
    }
}
