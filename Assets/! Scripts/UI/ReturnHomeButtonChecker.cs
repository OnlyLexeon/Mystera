using UnityEngine;
using UnityEngine.UI;

public class ReturnHomeButtonChecker : MonoBehaviour
{
    public GameObject returnButton;

    void Start()
    {
        SceneController sceneController = SceneController.instance;
        if (!sceneController.IsMainScene())
        {
            returnButton.SetActive(true);
            returnButton.GetComponent<Button>().onClick.AddListener(() => sceneController.LoadScene(sceneController.mainScene));
        }
        else returnButton.SetActive(false);
    }
}
