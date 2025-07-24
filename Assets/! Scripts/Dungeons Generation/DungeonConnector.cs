using UnityEngine;

public class DungeonConnector : MonoBehaviour
{
    public DungeonRoom parentRoom;
    public GameObject sealPrefab;

    [HideInInspector] public bool used = false;

    public void MarkUsed()
    {
        used = true;
    }

    public void Seal()
    {
        if (used || sealPrefab == null) return;

        Quaternion sealRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
        Instantiate(sealPrefab, transform.position, sealRotation, transform);

    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = used ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.75f);
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
#endif
}
