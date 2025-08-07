using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoundaryTP : MonoBehaviour
{
    public float outOfBoundsTime = 2f;

    public Transform teleportPoint;
    public Transform playerPoint;

    private Dictionary<GameObject, Coroutine> outOfBoundsObjects = new Dictionary<GameObject, Coroutine>();

    private void OnTriggerExit(Collider other)
    {
        if (!outOfBoundsObjects.ContainsKey(other.gameObject) && !other.CompareTag("Player")) // not player
        {
            Coroutine teleportRoutine = StartCoroutine(TeleportAfterDelay(other.gameObject));
            outOfBoundsObjects.Add(other.gameObject, teleportRoutine);
        }

        if (other.CompareTag("Player"))
        {
            other.transform.position = teleportPoint.position;
            other.transform.rotation = teleportPoint.rotation;

            //Reset rigidbody velocity
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private IEnumerator TeleportAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(outOfBoundsTime);

        if (obj != null && teleportPoint != null)
        {
            obj.transform.position = teleportPoint.position;
            obj.transform.rotation = teleportPoint.rotation;

            // Optional: Reset rigidbody velocity
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        outOfBoundsObjects.Remove(obj);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cancel pending teleport if object re-enters the trigger in time
        if (outOfBoundsObjects.TryGetValue(other.gameObject, out Coroutine routine))
        {
            StopCoroutine(routine);
            outOfBoundsObjects.Remove(other.gameObject);
        }
    }
}
