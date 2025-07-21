using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class RubbishBin : MonoBehaviour
{
    public AudioClip trashedSound;
    public Transform insideBinPos;
    public GameObject lastTrashed;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        XRGrabInteractable interactable = other.GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            // Play trash sound
            if (trashedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(trashedSound);
            }

            // Destroy the previous item if there was one
            if (lastTrashed != null && lastTrashed != other.gameObject)
            {
                Destroy(lastTrashed);
            }


            // Store and move the new object
            lastTrashed = other.gameObject;

            // Disable all colliders on the trashed object
            Collider[] colliders = lastTrashed.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Rigidbody rb = lastTrashed.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            //Teleport into Bin
            lastTrashed.transform.position = insideBinPos.position;
        }
    }
}
