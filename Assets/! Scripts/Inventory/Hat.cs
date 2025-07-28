using Unity.XR.CoreUtils.Datums;
using UnityEngine;

public class Hat : MonoBehaviour
{
    public Transform spawnPos;

    public bool isSelected = false;

    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip spawnSound;
    public AudioClip storeSound;
    public AudioClip rejectSound;

    [Header("Particles")]
    public ToggleParticle storeParticle;
    public ToggleParticle spawnParticle;

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
            Reject(obj);
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

        storeParticle.Play();

        Destroy(obj); //stored!
    }
    private void Reject(GameObject obj)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 rejectForce = (obj.transform.position - transform.position).normalized * 2f + Vector3.up;
            rb.AddForce(rejectForce, ForceMode.Impulse);
        }
    }

    public void SpawnedDoEffects()
    {
        spawnParticle.Play();
    }
}
