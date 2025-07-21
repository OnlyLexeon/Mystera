using UnityEngine;

public class Belt : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Offset Settings")]
    public float xOffset;
    public float yOffset;
    public float zOffset;

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Offset in local space, then apply to camera position
        Vector3 offset = cameraTransform.right * xOffset +
                         Vector3.up * yOffset +
                         cameraTransform.forward * zOffset;

        transform.position = cameraTransform.position + offset;

        // Only follow Y rotation (yaw)
        Vector3 cameraEuler = cameraTransform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, cameraEuler.y, 0);
    }
}
