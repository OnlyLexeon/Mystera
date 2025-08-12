using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;

    public bool isGameOver = false; //used by Menu to prevent opening pause menu when dead

    [Header("References")]
    public Button GoHomeButton;
    public Button RetryDungeonButton;
    public TextMeshProUGUI killedByText;
    public Camera UICamera;

    [Tooltip("The speed at which the canvas fades")]
    public float defaultDuration = 3f;

    [Header("UI Stuff")]
    public GameObject gameOverUI;
    private CanvasGroup canvasGroup = null;
    private float alpha = 0.0f;
    private float cameraFar = 25f;
    private Camera mainCamera;
    public LayerMask noUI;

    private GameObject lastAttacker;

    private SceneController sceneController;

    void Awake()
    {
        instance = this;

        gameOverUI.SetActive(false);
        canvasGroup = gameOverUI.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0; //invis

        mainCamera = Camera.main;
    }

    private void Start()
    {
        isGameOver = false;

        sceneController = SceneController.instance;
    }

    public void DoGameOver(GameObject attacker)
    {
        isGameOver = true;

        //disable moving
        MovementManager.instance.DisableMovement();

        lastAttacker = attacker;
        
        gameOverUI.SetActive(true);

        //camera settings
        mainCamera.cullingMask = noUI;
        UICamera.farClipPlane = 5;
        UICamera.enabled = true;

        StartCoroutine(DoCoroutines());
    }

    private IEnumerator DoCoroutines()
    {
        StartCoroutine(CanvasFadeIn(defaultDuration)); //fade canvas 3 seconds
        StartCoroutine(CameraFadeIn(defaultDuration));

        yield return new WaitForSeconds(defaultDuration);

        StartCoroutine(EnableButtons());
    }

    private IEnumerator EnableButtons()
    {
        if (lastAttacker != null) killedByText.text = "by " + lastAttacker.name;
        else killedByText.text = "by Unknown";
        yield return new WaitForSeconds(1);
        killedByText.gameObject.SetActive(true);

        GoHomeButton.onClick.AddListener(GoHomeScene);
        RetryDungeonButton.onClick.AddListener(RetryDungeon);
        GoHomeButton.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        RetryDungeonButton.gameObject.SetActive(true);
    }

    public void GoHomeScene()
    {
        sceneController.LoadScene(sceneController.mainScene);
    }

    public void RetryDungeon()
    {
        sceneController.LoadScene(sceneController.dungeonScene); //no need to input ID
    }

    private IEnumerator CameraFadeIn(float duration)
    {
        float elapsedTime = 0.0f;

        float startFar = 25f;
        float endFar = 2.5f;

        SetCameraFar(startFar);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            cameraFar = Mathf.Lerp(startFar, endFar, t);
            SetCameraFar(cameraFar);
            yield return null;
        }

        SetCameraFar(endFar);
    }


    private IEnumerator CanvasFadeIn(float duration)
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

    private void SetAlpha(float value)
    {
        alpha = value;
        canvasGroup.alpha = alpha;
    }

    private void SetCameraFar(float value)
    {
        cameraFar = value;
        mainCamera.farClipPlane = cameraFar;
    }
}