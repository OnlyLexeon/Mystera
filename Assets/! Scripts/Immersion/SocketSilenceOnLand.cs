using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class SocketSilenceOnLand : MonoBehaviour
{
    private XRSocketInteractor socketInteractor;

    private void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
        socketInteractor.selectEntered.AddListener(OnSelectEntered);
        socketInteractor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        socketInteractor.selectEntered.RemoveListener(OnSelectEntered);
        socketInteractor.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        GameObject insertedObject = args.interactableObject.transform.gameObject;

        OnLandPlaySound soundScript = insertedObject.GetComponent<OnLandPlaySound>();

        if (soundScript != null)
        {
            soundScript.enabled = false;
            Debug.Log($"Disabled OnLandPlaySound on {insertedObject.name}");
        }
        else
        {
            Debug.Log($"No OnLandPlaySound found on {insertedObject.name}");
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        GameObject removedObject = args.interactableObject.transform.gameObject;

        OnLandPlaySound soundScript = removedObject.GetComponent<OnLandPlaySound>();

        if (soundScript != null)
        {
            soundScript.enabled = true;
            Debug.Log($"Re-enabled OnLandPlaySound on {removedObject.name}");
        }
    }
}
