using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PokeHaptic : MonoBehaviour
{
    private XRPokeInteractor pokeInteractor;
    private XRBaseInputInteractor controllerInteractor;

    private void Awake()
    {
        pokeInteractor = GetComponent<XRPokeInteractor>();
        controllerInteractor = GetComponent<XRBaseInputInteractor>();
    }

    private void OnEnable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.AddListener(OnPokeStarted);
        }
    }

    private void OnDisable()
    {
        if (pokeInteractor != null)
        {
            pokeInteractor.selectEntered.RemoveListener(OnPokeStarted);
        }
    }


    private void OnPokeStarted(SelectEnterEventArgs args)
    {
        if (controllerInteractor != null)
        {
            controllerInteractor.SendHapticImpulse(0.5f, 0.1f);
        }
    }
}
