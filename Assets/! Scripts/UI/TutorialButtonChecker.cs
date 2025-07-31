using UnityEngine;

public class TutorialButtonChecker : MonoBehaviour
{
    public GameObject tutorialButton;

    void Start()
    {
        SceneController sceneController = SceneController.instance;
        if (sceneController.GetSceneName() == sceneController.mainScene) tutorialButton.SetActive(true);
        else tutorialButton.SetActive(false);

        Debug.Log($"Tutorial Button: {tutorialButton.activeSelf}");
    }
}
