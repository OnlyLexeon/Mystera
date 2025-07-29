using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TagSpecificSocket : XRSocketInteractor
{
    [Header("Add Tag!!")]
    public string specificTag;

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && MatchesTag(interactable);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && MatchesTag(interactable);
    }

    private bool MatchesTag(IXRInteractable interactable)
    {
        if (interactable is MonoBehaviour mb)
        {
            if (specificTag == null) return false;
            return mb.CompareTag(specificTag);
        }

        return false;
    }

}
