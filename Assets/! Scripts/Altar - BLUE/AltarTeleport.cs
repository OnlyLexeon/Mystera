using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class AltarTeleport : MonoBehaviour
{
    public Portal portal;

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
    public Button nextDungeonButton;

    [Header("Dungeon UI")]
    public CanvasGroup canvas;
    public GameObject dungeonsMenu;
    public GameObject mainMenu;
    public Transform dungeonButtonHolder;
    public GameObject dungeonButtonPrefab;


    private SceneController sceneController;
    private DungeonManager dungeonManager;

    private void Start()
    {
        canvas.alpha = 0;

        sceneController = SceneController.instance;
        dungeonManager = DungeonManager.instance;

        player = Camera.main.transform;

        if (homeButton != null)
            homeButton.onClick.AddListener(() => SetPortal(GoHomeScene));

        if (dungeonsButton != null)
            dungeonsButton.onClick.AddListener(OpenDungeonsMenu);

        if (trainingButton != null)
            trainingButton.onClick.AddListener(() => SetPortal(GoTrainingScene));

        //only enable next dungeon if theres actually a dungeon
        if(sceneController.isDungeonScene())
        {
            string nextDungeon = dungeonManager.GetSettingsByID(sceneController.dungeonID).nextDungeonID;
            if (dungeonManager.GetSettingsByID(nextDungeon) == null)
            {
                nextDungeonButton.gameObject.SetActive(false);
            }
        }

        if (nextDungeonButton != null)
            nextDungeonButton.onClick.AddListener(() => SetPortal(GoNextDungeon));

        LoadDungeonsButtons();
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

    // Set Portal Event
    public void SetPortal(Action function)
    {        
        portal.SetEvent(function);
        portal.isOpen = true;

        StartCoroutine(AnimationPortal());
    }

    private IEnumerator AnimationPortal()
    {
        if (portal.isOpen) //close first
        {
            portal.boxCollider.enabled = false;
            portal.animator.SetTrigger("Close");
            yield return new WaitForSeconds(0.6f);
        }

        //open
        portal.animator.SetTrigger("Open");
        portal.boxCollider.enabled = true;
    }

    // UI
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

    // GO Functions
    public void GoHomeScene()
    {
        if (sceneController.isDungeonScene())  //unlock new dungeon
        {
            dungeonManager.UnlockDungeon(dungeonManager.GetSettingsByID(sceneController.dungeonID).nextDungeonID);
        }
        sceneController.LoadScene(sceneController.mainScene);
    }

    public void GoDungeonScene(string ID)
    {
        sceneController.LoadScene(sceneController.dungeonScene, ID);
    }

    public void GoTrainingScene()
    {
        sceneController.LoadScene(sceneController.trainingScene);
    }

    public void GoNextDungeon()
    {
        if (sceneController.isDungeonScene()) //unlock new dungeon
        {
            dungeonManager.UnlockDungeon(dungeonManager.GetSettingsByID(sceneController.dungeonID).nextDungeonID);
        }
        sceneController.LoadScene(sceneController.dungeonScene, dungeonManager.GetSettingsByID(sceneController.dungeonID).nextDungeonID);
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
            TextMeshProUGUI btnText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            //text
            btnText.text = settings.dungeonID;

            //setting on button click
            string dungeonID = settings.dungeonID;
            btn.onClick.AddListener(() => SetPortal(() => GoDungeonScene(dungeonID)));

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
