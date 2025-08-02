using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    public GameObject loadingUI;

    [Header("Scenes")]
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName, string dungeonIDToLoad = "")
    {
        if (sceneName == dungeonScene)
        {
            dungeonID = dungeonIDToLoad;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        FadeCanvas.instance.StartFadeIn();

        yield return new WaitForSeconds(FadeCanvas.instance.defaultDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        LoadingScreenManager.instance.Show();

        while (asyncLoad.progress < 0.9f)
        {
            LoadingScreenManager.instance.UpdateProgress(asyncLoad.progress);
            yield return null;
        }

        LoadingScreenManager.instance.UpdateProgress(1f);
        yield return new WaitForSeconds(0.25f);

        asyncLoad.allowSceneActivation = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FinalizeSceneLoad());
    }

    private IEnumerator FinalizeSceneLoad()
    {
        currentScene = SceneManager.GetActiveScene().name;

        // If it's a dungeon, generate
        if (currentScene == dungeonScene)
        {
            DungeonManager.instance.GenerateDungeon(dungeonID);
            Debug.Log($"Generated Dungeon with ID: {dungeonID}");
        }

        yield return null; // wait 1 frame

        //enable player


        LoadingScreenManager.instance.Hide();

        yield return new WaitForSeconds(LoadingScreenManager.instance.defaultDuration);
        
        FadeCanvas.instance.StartFadeOut();
        
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

    public bool isMainScene()
    {
        return SceneManager.GetActiveScene().name == mainScene;
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
