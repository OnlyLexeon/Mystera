using System.Collections.Generic;
using Unity.XR.CoreUtils.Datums;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Hat : MonoBehaviour
{
    public Transform spawnPos;
    public XRSocketInteractor socket;
    public float comeBackDistance = 4f;
    public bool isSelected = false;
    public float rejectForce = 0.2f;

    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip spawnSound;
    public AudioClip storeSound;
    public AudioClip rejectSound;

    [Header("Particles")]
    public ToggleParticle storeParticle;
    public ToggleParticle spawnParticle;
    public ToggleParticle comeBackParticle;

    private HatInventoryUI ui;
    private Player player;

    private void Start()
    {
        ui = GetComponent<HatInventoryUI>();
        player = FindFirstObjectByType<Player>();

        if (socket == null) transform.parent.GetComponentInChildren<XRSocketInteractor>();
    }

    private void Update()
    {
        if (!isSelected && socket != null && player != null)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance > comeBackDistance && !socket.hasSelection)
            {
                TrySocketHat();
            }
        }
    }

    private void TrySocketHat()
    {
        var interactable = GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            //Force Socket
            socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)interactable);
            comeBackParticle.Play();
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor || args.interactorObject is XRRayInteractor)
            SetSelected(true);
        else ui?.HideUI(); //if selected but not by direct or ray, (prolly socket) hide
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor || args.interactorObject is XRRayInteractor)
            SetSelected(false);
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HatInventoryManager.instance == null || isSelected == false) return;

        GameObject obj = other.gameObject;

        if (!obj.TryGetComponent<StorableObject>(out var storable))
        {
            return;
        }

        if (storable.IsInLockout())
        {
            return;
        }

        if (!HatInventoryManager.instance.TryAddItem(obj))
        {
            Reject(obj);
            return;
        }

        //Effects
        storeParticle.Play();
        audioSource.PlayOneShot(storeSound);
        ui.RefreshUI();

        Destroy(obj); //stored!
    }

    private void Reject(GameObject obj)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 force = (obj.transform.position - transform.position).normalized * rejectForce + Vector3.up;
            rb.AddForce(force, ForceMode.Impulse);

            audioSource.PlayOneShot(rejectSound);
        }
    }

    public void SpawnedDoEffects()
    {
        spawnParticle.Play();
        audioSource.PlayOneShot(spawnSound);
    }
}
