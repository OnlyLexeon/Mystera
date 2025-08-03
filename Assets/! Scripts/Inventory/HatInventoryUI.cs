using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.EventSystems;

public class HatInventoryUI : MonoBehaviour
{
    public Hat hat;

    public GameObject tooltip;
    public RectTransform tooltipBackground;
    public TextMeshProUGUI tooltipText;

    public GameObject uiCanvas;
    public GameObject slotPrefab;
    public Transform gridParent;

    public bool buttonsDisabled = false;

    private void Start()
    {
        RefreshUI();
        HideUI();

        if (hat == null) hat = GetComponent<Hat>();
    }

    //hide and show is called a lot by gaze interactor
    public void ShowUI()
    {
        if (hat.isSelected)
            uiCanvas.SetActive(true);
    }
    public void HideUI()
    {
        uiCanvas.SetActive(false);
    }

    public void RefreshUI()
    {
        //clear
        foreach (Transform child in gridParent)
        {
            if (child) Destroy(child.gameObject);
        }

        //set
        List<HatInventoryManager.InventorySlot> inventory = HatInventoryManager.instance.GetInventory();
        foreach (var slot in inventory)
        {
            GameObject slotObj = Instantiate(slotPrefab, gridParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null)
                slotUI.SetSlot(slot);
        }
    }


    public void DisableAllButtons(float duration)
    {
        buttonsDisabled = true;

        foreach (Transform child in gridParent)
        {
            var button = child.GetComponent<Button>();
            if (button != null)
                button.interactable = false;
        }

        Invoke(nameof(EnableAllButtons), duration);
    }

    private void EnableAllButtons()
    {
        foreach (Transform child in gridParent)
        {
            var button = child.GetComponent<Button>();
            if (button != null)
                button.interactable = true;
        }

        buttonsDisabled = false;
    }
}
