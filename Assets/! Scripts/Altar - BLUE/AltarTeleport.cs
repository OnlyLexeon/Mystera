using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class AltarTeleport : MonoBehaviour
{
    [Header("Settings")]
    public float minPlayerDistance = 2f;

    [Header("Events")]
    public UnityEvent onPlayerEnterRange;
    public UnityEvent onPlayerExitRange;

    private Transform player;
    private bool isInRange = false;

    [Header("References")]
    public Button dungeonsButton;
    public Button trainingButton;
    public Button homeButton;

    [Header("Dungeon UI")]
    public GameObject dungeonsMenu;
    public GameObject mainMenu;
    public Transform dungeonButtonHolder;
    public GameObject dungeonButtonPrefab;


    private SceneController sceneController;
    private DungeonManager dungeonManager;

    private void Start()
    {
        sceneController = SceneController.instance;
        dungeonManager = DungeonManager.instance;

        player = Camera.main.transform;

        if (homeButton != null)
            homeButton.onClick.AddListener(GoHomeScene);

        if (dungeonsButton != null)
            dungeonsButton.onClick.AddListener(OpenDungeonsMenu);

        if (trainingButton != null)
            trainingButton.onClick.AddListener(GoTrainingScene);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool currentlyInRange = distance <= minPlayerDistance;

        if (currentlyInRange && !isInRange)
        {
            isInRange = true;
            onPlayerEnterRange.Invoke();
        }
        else if (!currentlyInRange && isInRange)
        {
            isInRange = false;
            onPlayerExitRange.Invoke();
        }
    }

    public void GoHomeScene()
    {
        sceneController.LoadScene(sceneController.mainScene);
    }

    public void OpenDungeonsMenu()
    {
        dungeonsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void CloseDungeonsMenu()
    {
        dungeonsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void GoDungeonScene(string ID)
    {
        sceneController.LoadScene(sceneController.dungeonScene, ID);
    }

    public void GoTrainingScene()
    {
        sceneController.LoadScene(sceneController.trainingScene);
    }

    //===========================================

    public void LoadDungeonsButtons()
    {
        //clear existing buttons
        foreach (Transform child in dungeonButtonHolder)
            Destroy(child.gameObject);

        foreach (var settings in dungeonManager.dungeonSettings)
        {
            GameObject buttonObj = Instantiate(dungeonButtonPrefab, dungeonButtonHolder);
            Button btn = buttonObj.GetComponent<Button>();
            Text btnText = buttonObj.GetComponentInChildren<Text>();

            //text
            btnText.text = settings.dungeonID;

            //setting on button blick
            string dungeonID = settings.dungeonID;
            btn.onClick.AddListener(() => GoDungeonScene(dungeonID));

            //gray out locked dungeons
            if (!dungeonManager.IsUnlocked(dungeonID))
            {
                btn.interactable = false;
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.gray;
                btn.colors = colors;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dungeonButtonHolder);
    }

}
