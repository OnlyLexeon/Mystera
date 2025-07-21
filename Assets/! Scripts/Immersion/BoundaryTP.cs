using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoundaryTP : MonoBehaviour
{
    [Tooltip("How long to wait after an object exits before teleporting it back.")]
    public float outOfBoundsTime = 2f;

    [Tooltip("The point to teleport the object back to.")]
    public Transform teleportPoint;

    // Track currently waiting objects so we don't start multiple coroutines
    private Dictionary<GameObject, Coroutine> outOfBoundsObjects = new Dictionary<GameObject, Coroutine>();

    private void OnTriggerExit(Collider other)
    {
        // Avoid duplicating coroutines for the same object
        if (!outOfBoundsObjects.ContainsKey(other.gameObject))
        {
            Coroutine teleportRoutine = StartCoroutine(TeleportAfterDelay(other.gameObject));
            outOfBoundsObjects.Add(other.gameObject, teleportRoutine);
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
