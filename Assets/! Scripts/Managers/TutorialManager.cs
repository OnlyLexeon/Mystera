using System.Collections.Generic;
using UnityEngine;

public enum TutorialStep
{
    Welcome,
    MoveLeft,
    RotateRight,
    MoveToAltar,
    UseMenu,
    OpenRecipes,
    OpenIngredients,
    SummonIngredient,
    HighlightSpawnPosition,
    PickUpWand,
    UseTrigger,
    TraceParchment,
    CompleteParchment,
    DropInPot,
    Congratulate,
    GlassBottles,
    Repeatable,
    EndTutorial
}

public class TutorialManager : MonoBehaviour
{
    [Header("Events References")]
    public AltarTabManager altarTabManager;
    public AltarBook altarBook;
    public bool hasCompletedAParchment = false;
    public Pot potScript;

    [Header("Targets")]
    public Transform altar;
    public Wand wand;
    public Transform parchmentSpawnPos;
    public Transform pot;
    public Transform glassBottles;

    [Header("Tutorial References")]
    public TutorialStep tutorialStep = TutorialStep.Welcome;
    public Transform currentTarget;
    public GameObject fireFliesParticle;
    public GameObject spotLight;

    [Header("Settings")]
    public float minDistanceFromTarget = 2f;
    public float spotLightYOffset = 2f;
    public float lineRendererYOffset = -1f;

    [Header("Dialogue Boxes")]
    public GameObject dialogueBoxPrefab;
    public Vector3 dialogueSpawnOffset;
    public List<Dialogue> dialogues;

    public GameObject currentDialogueBox;

    private Transform player;
    private LineRenderer lineRenderer;

    public static TutorialManager instance;

    private bool doNotShowAgain = false;
    private bool onGoingTutorial = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        // Get main camera (usually tagged "MainCamera")
        Camera cam = Camera.main;
        if (cam != null)
            player = cam.transform;
        else
            Debug.LogWarning("MainCamera not found. Please tag your VR camera as MainCamera.");

        //Showing tutorial menu
        int showTutorial = PlayerPrefs.GetInt("TutorialDoNotShowAgain", 0);
        if (showTutorial == 0)
        {
            MenuUIManager.instance.OpenTutorialMenu();
        }
    }

    public void ToggleDoNotShowAgain()
    {
        doNotShowAgain = !doNotShowAgain;

        if (doNotShowAgain) PlayerPrefs.SetInt("TutorialDoNotShowAgain", 1); //1 - dont show again, 0 - show ig idk
        else PlayerPrefs.SetInt("TutorialDoNotShowAgain", 0);
    }

    public void StartTutorial()
    {
        onGoingTutorial = true;
        tutorialStep = 0;
        hasCompletedAParchment = false;
        MovementDetection.instance.hasMoved = false;
        MovementDetection.instance.hasTurned = false;
        altarTabManager.hasOpenedRecipes = false;
        altarTabManager.hasOpenedIngredients = false;
        altarBook.hasSpawned = false; ;
        wand.hasBeenSelected = false;
        wand.hasBeenActivated = false;
        potScript.hasAddedIngredient = false;

        MovementDetection.instance.isTracking = true;

        DoTutorialNextStep();
    }

    //Set targets, set current dialogues
    public void DoTutorialNextStep()
    {
        DisableFireFlies();
        DisableLine();
        DisableSpotLight();

        switch (tutorialStep)
        {
            case TutorialStep.Welcome:
                ShowDialogue(0);
                break;
            case TutorialStep.MoveLeft:
                ShowDialogue(1);
                break;
            case TutorialStep.RotateRight:
                ShowDialogue(2);
                break;
            case TutorialStep.MoveToAltar:
                currentTarget = altar;
                ShineSpotlight();
                MoveFireFlies();
                DrawLineToCurrentTarget();
                ShowDialogue(3);
                break;
            case TutorialStep.UseMenu:
                ShowDialogue(4);
                break;
            case TutorialStep.OpenRecipes:
                ShowDialogue(5);
                break;
            case TutorialStep.OpenIngredients:
                ShowDialogue(6);
                break;
            case TutorialStep.SummonIngredient:
                ShowDialogue(7);
                break;
            case TutorialStep.HighlightSpawnPosition:
                currentTarget = parchmentSpawnPos;
                ShineSpotlight();
                MoveFireFlies();
                DrawLineToCurrentTarget();
                ShowDialogue(8);
                break;
            case TutorialStep.PickUpWand:
                currentTarget = wand.transform;
                ShineSpotlight();
                MoveFireFlies();
                DrawLineToCurrentTarget();
                ShowDialogue(9);
                break;
            case TutorialStep.UseTrigger:
                ShowDialogue(10);
                break;
            case TutorialStep.TraceParchment:
                ShowDialogue(11);
                break;
            case TutorialStep.CompleteParchment:
                ShowDialogue(12);
                break;
            case TutorialStep.DropInPot:
                currentTarget = pot;
                ShineSpotlight();
                MoveFireFlies();
                DrawLineToCurrentTarget();
                ShowDialogue(13);
                break;
            case TutorialStep.Congratulate:
                ShowDialogue(14);
                break;
            case TutorialStep.GlassBottles:
                currentTarget = glassBottles;
                ShineSpotlight();
                MoveFireFlies();
                DrawLineToCurrentTarget();
                ShowDialogue(15);
                break;
            case TutorialStep.Repeatable:
                ShowDialogue(16);
                break;
            case TutorialStep.EndTutorial:
                EndTutorial();
                break;
        }
    }

    //Use this to check if step is completed
    private void Update()
    {
        if (lineRenderer.enabled == true)
        {
            UpdateLineRenderer();
        }

        if (onGoingTutorial)
        {
            switch (tutorialStep)
            {
                case TutorialStep.Welcome:
                    if (IsDialogueDone()) 
                        CompleteStep(TutorialStep.Welcome);
                    break;
                case TutorialStep.MoveLeft:
                    if (MovementDetection.instance.hasMoved)
                        CompleteStep(TutorialStep.MoveLeft);
                    break;
                case TutorialStep.RotateRight:
                    if (MovementDetection.instance.hasTurned)
                        CompleteStep(TutorialStep.RotateRight);
                    break;
                case TutorialStep.MoveToAltar:
                    if (IsPlayerCloseToTarget())
                        CompleteStep(TutorialStep.MoveToAltar);
                    break;
                case TutorialStep.UseMenu:
                    if (IsDialogueDone())
                        CompleteStep(TutorialStep.UseMenu);
                    break;
                case TutorialStep.OpenRecipes:
                    if (altarTabManager.hasOpenedRecipes)
                        CompleteStep(TutorialStep.OpenRecipes);
                    break;
                case TutorialStep.OpenIngredients:
                    if (altarTabManager.hasOpenedIngredients)
                        CompleteStep(TutorialStep.OpenIngredients);
                    break;
                case TutorialStep.SummonIngredient:
                    if (altarBook.hasSpawned)
                        CompleteStep(TutorialStep.SummonIngredient);
                    break;
                case TutorialStep.HighlightSpawnPosition:
                    if (IsPlayerCloseToTarget())
                        CompleteStep(TutorialStep.HighlightSpawnPosition);
                    break;
                case TutorialStep.PickUpWand:
                    if (wand.hasBeenSelected)
                        CompleteStep(TutorialStep.PickUpWand);
                    break;
                case TutorialStep.UseTrigger:
                    if (wand.hasBeenActivated)
                        CompleteStep(TutorialStep.UseTrigger);
                    break;
                case TutorialStep.TraceParchment:
                    if (hasCompletedAParchment)
                        CompleteStep(TutorialStep.TraceParchment);
                    break;
                case TutorialStep.CompleteParchment:
                    if (IsDialogueDone())
                        CompleteStep(TutorialStep.CompleteParchment);
                    break;
                case TutorialStep.DropInPot:
                    if (potScript.hasAddedIngredient) 
                        CompleteStep(TutorialStep.DropInPot);
                    break;
                case TutorialStep.GlassBottles:
                    if (IsDialogueDone())
                        CompleteStep(TutorialStep.GlassBottles);
                    break;
                case TutorialStep.Repeatable:
                    if (IsDialogueDone())
                        CompleteStep(TutorialStep.Repeatable);
                    break;
                case TutorialStep.Congratulate:
                    if (IsDialogueDone())
                        CompleteStep(TutorialStep.Congratulate);
                    break;
            }
        }
    }

    public void CompleteStep(TutorialStep stepToComplete)
    {
        if (tutorialStep == stepToComplete)
        {
            tutorialStep = (TutorialStep)((int)stepToComplete + 1);

            DoTutorialNextStep();
        }
    }


    public void ShineSpotlight()
    {
        if (spotLight == null) return;

        Vector3 spotlightPos = currentTarget.position + Vector3.up * spotLightYOffset;
        spotLight.transform.position = spotlightPos;
        spotLight.SetActive(true);
    }

    public void DisableSpotLight()
    {
        spotLight.SetActive(false);
    }

    public void MoveFireFlies()
    {
        if (fireFliesParticle == null) return;

        fireFliesParticle.transform.position = currentTarget.position;
        fireFliesParticle.SetActive(true);
    }

    public void DisableFireFlies()
    {
        fireFliesParticle.SetActive(false);
    }

    private void ShowDialogue(int index)
    {
        if (currentDialogueBox != null)
            Destroy(currentDialogueBox);

        currentDialogueBox = Instantiate(dialogueBoxPrefab, currentTarget.position + dialogueSpawnOffset, Quaternion.identity);

        TutorialDialogue dialogueScript = currentDialogueBox.GetComponent<TutorialDialogue>();
        dialogueScript.PlayDialogue(dialogues[index]);

    }

    private void EndTutorial()
    {
        MovementDetection.instance.isTracking = false;

        if (currentDialogueBox != null)
            Destroy(currentDialogueBox);
    }

    public void DrawLineToCurrentTarget()
    {
        if (player == null || currentTarget == null || lineRenderer == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3 offset = new Vector3 (0, lineRendererYOffset, 0);
        lineRenderer.SetPosition(0, player.position + offset);
        lineRenderer.SetPosition(1, currentTarget.position + offset);
        lineRenderer.enabled = true;
    }

    public void UpdateLineRenderer()
    {
        Vector3 offset = new Vector3(0, lineRendererYOffset, 0);
        lineRenderer.SetPosition(0, player.position + offset);
        lineRenderer.SetPosition(1, currentTarget.position + offset);
    }

    public void DisableLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    public bool IsDialogueDone()
    {
        if (currentDialogueBox == null) return true;

        TutorialDialogue dialogueScript = currentDialogueBox.GetComponent<TutorialDialogue>();
        if (dialogueScript == null) return true;

        return dialogueScript.isDone;
    }

    public bool IsPlayerCloseToTarget()
    {
        if (player == null || currentTarget == null) return false;

        float distance = Vector3.Distance(player.position, currentTarget.position);
        return distance <= minDistanceFromTarget;
    }

    public void SubscribeParchmentCompleteEvent(TutorialParchmentComplete parchment)
    {
        Debug.Log($"Subscribed to {parchment}!");
        parchment.OnTaskCompleted += ParchmentCompleted;
    }

    public void ParchmentCompleted()
    {
        Debug.Log("parchment completed");
        hasCompletedAParchment = true;
    }
}
