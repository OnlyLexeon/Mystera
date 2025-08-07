using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ScenePersistentItemGrabber : MonoBehaviour
{
    private ItemHoldingManager itemHoldingManager;

    public bool isLeftHand = false;
    public XRBaseInteractor interactor;


    private void Start()
    {
        itemHoldingManager = ItemHoldingManager.instance;
    }

    public void DoHolding(bool isHolding)
    {
        //this is responsible for making sure objects are persistent throiughout scenes!!!

        if (itemHoldingManager == null || interactor == null)
            return;

        GameObject heldObject = GetHeldObject(interactor);

        if (isHolding && heldObject != null)
        {
            itemHoldingManager.SetHeldItem(heldObject, isLeftHand);
        }
        else
        {
            itemHoldingManager.ClearHeldItem(isLeftHand);
        }
    }

    public GameObject GetHeldObject(XRBaseInteractor interactor)
    {
        if (interactor.hasSelection)
        {
            var interactable = interactor.firstInteractableSelected?.transform;
            return interactable?.gameObject;
        }

        return null;
    }
}
