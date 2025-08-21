using UnityEngine;

public class TutorialButtonChecker : MonoBehaviour
{
    public GameObject tutorialButton;

    private TutorialManager tutorialManager;
    private bool canDoTutorial = false;

    void Start()
    {
        SceneController sceneController = SceneController.instance;
        if (sceneController.IsMainScene()) CanDoTutorial();
        else tutorialButton.SetActive(false);

        Debug.Log($"Tutorial Button: {tutorialButton.activeSelf}");
    }

    public void CanDoTutorial()
    {
        canDoTutorial = true;

        tutorialButton.SetActive(true);

        tutorialManager = TutorialManager.instance;
    }

    public void DoTutorial()
    {
        if (canDoTutorial && tutorialManager) tutorialManager.StartTutorial();
    }

    public void ToggleDoNotShowAgain()
    {
        if (canDoTutorial && tutorialManager) tutorialManager.ToggleDoNotShowAgain();
    }
}
