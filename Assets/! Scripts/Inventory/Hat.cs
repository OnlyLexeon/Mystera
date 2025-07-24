using UnityEngine;

public class Hat : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (HatInventoryManager.Instance == null) return;

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

        if (!HatInventoryManager.Instance.TryAddItem(obj))
        {
            Reject(obj);
            return;
        }

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
}
