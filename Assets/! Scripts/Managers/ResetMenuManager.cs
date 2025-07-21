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
        StartCoroutine(ResetCoroutine());
    }

    public IEnumerator ResetCoroutine()
    {
        fadeCanvas.StartFadeIn();

        yield return new WaitForSeconds(fadeDelay);

        // Reloads the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

}
