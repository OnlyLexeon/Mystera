using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    public string mainScene = "Alchemist";
    public string trainingScene = "TrainingArea";
    public string dungeonScene = "Dungeons";

    [Header("Current")]
    public string currentScene = "";
    public string dungeonID = "";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void DoOnSceneLoad()
    {
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == dungeonScene)
        {
            DungeonManager.instance.GenerateDungeon(dungeonID);

            Debug.Log($"Generated Dungeon with ID: {dungeonID}");
        }
    }

    public void LoadScene(string sceneName, string dungeonIDToLoad = "")
    {
        if (sceneName == dungeonScene)
        {
            dungeonID = dungeonIDToLoad;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    public IEnumerator LoadSceneCoroutine(string sceneName)
    {
        FadeCanvas.instance.StartFadeOut();

        yield return new WaitForSeconds(FadeCanvas.instance.defaultDuration);

        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator ResetCoroutine()
    {
        FadeCanvas.instance.StartFadeIn();

        yield return new WaitForSeconds(FadeCanvas.instance.defaultDuration);

        // Reloads the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }


    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
