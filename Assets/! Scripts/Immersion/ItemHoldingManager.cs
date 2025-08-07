using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ItemHoldingManager : MonoBehaviour
{
    public static ItemHoldingManager instance;

    public bool isPaused = false;

    [Header("Saves the item you are holding through scenes")]
    public GameObject heldLeftItem;
    public GameObject heldRightItem;

    private HatInventoryManager inventoryManager;

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

    //called by XR direct/ray interactable
    public void SetHeldItem(GameObject item, bool isLeftHand)
    {
        if (isPaused) return;

        if (isLeftHand)
            heldLeftItem = item;
        else
            heldRightItem = item;
    }

    //called by XR direct/ray interactable
    public void ClearHeldItem(bool isLeftHand)
    {
        if (isPaused) return;

        if (isLeftHand)
            heldLeftItem = null;
        else
            heldRightItem = null;
    }

    public void SetPauseHoldingItem(bool state)
    {
        isPaused = state;
    }

    public void TryPutHoldingItemsInHat()
    {
        if (heldLeftItem != null) inventoryManager.TryAddItem(heldLeftItem);
        if (heldRightItem != null) inventoryManager.TryAddItem(heldRightItem);
    }
}
