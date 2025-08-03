using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor.Experimental.GraphView;
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
            homeButton.onClick.AddListener(() => SetPortal(GoHomeScene));

        if (dungeonsButton != null)
            dungeonsButton.onClick.AddListener(OpenDungeonsMenu);

        if (trainingButton != null)
            trainingButton.onClick.AddListener(() => SetPortal(GoTrainingScene));

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
