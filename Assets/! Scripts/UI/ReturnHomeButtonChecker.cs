using UnityEngine;
using UnityEngine.UI;

public class ReturnHomeButtonChecker : MonoBehaviour
{
    public GameObject returnButton;

    void Start()
    {
        SceneController sceneController = SceneController.instance;
        if (!sceneController.IsMainScene()) returnButton.SetActive(true);
        else returnButton.SetActive(false);

        returnButton.GetComponent<Button>().onClick.AddListener(() => sceneController.LoadScene(sceneController.mainScene));
    }
}
