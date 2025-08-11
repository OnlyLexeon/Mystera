using UnityEngine;

public class DayTimeUIChecker : MonoBehaviour
{
    public GameObject dayTimeUI;

    void Start()
    {
        SceneController sceneController = SceneController.instance;
        if (sceneController.IsMainScene()) dayTimeUI.SetActive(true);
        else dayTimeUI.SetActive(false);
    }
}
