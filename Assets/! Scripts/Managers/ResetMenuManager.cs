using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetMenuManager : MonoBehaviour
{
    public bool isResetMenuOpen = false;
    public GameObject resetMenu;
    public FadeCanvas fadeCanvas;

    public float fadeDelay = 2f;

    public void ToggleResetMenu()
    {
        isResetMenuOpen = !isResetMenuOpen;

        if (isResetMenuOpen)
        {
            resetMenu.SetActive(true);
        }
        else
        {
            resetMenu.SetActive(false);
        }
    }

    public void Reset()
    {
        StartCoroutine(SceneController.instance.ResetCoroutine());
    }

    

}
