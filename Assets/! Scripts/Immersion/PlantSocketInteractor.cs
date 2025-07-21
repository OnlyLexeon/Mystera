using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlantSocketInteractor : XRSocketInteractor
{
    [Header("PlantSocket")]
    public PottedPlant pottedPlant;
    public int spotIndex;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (pottedPlant != null)
        {
            pottedPlant.Pluck(spotIndex);
        }
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Transform interactableTransform = (interactable as Component)?.transform;
        if (interactableTransform == null) return false;

        return base.CanSelect(interactable) && interactableTransform.IsChildOf(pottedPlant.growSpots[spotIndex]);
    }

}