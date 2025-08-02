using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

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
        FadeCanvasInstance.instance.StartFadeIn();

        yield return new WaitForSeconds(FadeCanvasInstance.instance.defaultDuration);

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

    public void OnSceneLoaded()
    {
        StartCoroutine(FinalizeSceneLoad());
    }

    private IEnumerator FinalizeSceneLoad()
    {
        currentScene = SceneManager.GetActiveScene().name;

        // If it's a dungeon, generate
        if (currentScene == dungeonScene)
        {
            DungeonManager dungeonManager= DungeonManager.instance;
            if (dungeonManager != null)
            {
                DungeonManager.instance.GenerateDungeon(dungeonID);
                Debug.Log($"Generated Dungeon with ID: {dungeonID}");
            }
            else
            {
                Debug.LogWarning("CANT FIND DUNGEON MANAGER");
            }

            //move player
            DungeonSpawnPoint spawnPoint = DungeonSpawnPoint.instance;
            if (spawnPoint != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();

                    if (cc != null)
                    {
                        cc.enabled = false; //disable before moving
                        player.transform.position = spawnPoint.spawnPos.position;
                        cc.enabled = true;  //re-enable after
                    }
                    else
                    {
                        player.transform.position = spawnPoint.spawnPos.position;
                    }
                }
                else Debug.LogWarning("No player found!");
            }
            else Debug.LogWarning("No spawn point found!");
        }

        yield return null; // wait 1 frame

        LoadingScreenManager.instance.Hide();

        yield return new WaitForSeconds(LoadingScreenManager.instance.defaultDuration);

        FadeCanvasInstance.instance.StartFadeOut();
        
    }

    public IEnumerator ResetCoroutine()
    {
        FadeCanvasInstance.instance.StartFadeIn();

        yield return new WaitForSeconds(FadeCanvasInstance.instance.defaultDuration);

        // Reloads the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public bool IsMainScene()
    {
        return (SceneManager.GetActiveScene().name == mainScene);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
