using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementDetection : MonoBehaviour
{
    public XROrigin xrOrigin;
    public float moveThreshold = 0.01f;
    public float teleportThreshold = 0.5f;
    public float turnThreshold = 1f;

    [Header("Tutorial Stuff")]
    public bool hasMoved;
    public bool hasTurned;

    private Vector3 lastPosition;
    private Vector3 lastTeleportCheckPosition;
    private float lastYRotation;

    public bool isTracking = false;

    public static MovementDetection instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (xrOrigin != null)
        {
            lastPosition = xrOrigin.transform.position;
            lastTeleportCheckPosition = lastPosition;
            lastYRotation = xrOrigin.transform.eulerAngles.y;
        }
    }

    void Update()
    {
        if (isTracking == false) return;

        if (xrOrigin == null)
            return;

        Vector3 currentPosition = xrOrigin.transform.position;
        float currentYRotation = xrOrigin.transform.eulerAngles.y;

        // Check for teleport (large instant jump in position)
        float teleportDistance = Vector3.Distance(currentPosition, lastTeleportCheckPosition);
        if (teleportDistance > teleportThreshold)
        {
            hasMoved = true;
            lastTeleportCheckPosition = currentPosition;
        }

        // Check for continuous movement
        float moveDistance = Vector3.Distance(currentPosition, lastPosition);
        if (moveDistance > moveThreshold)
        {
            hasMoved = true;
            lastPosition = currentPosition;
        }

        // Check for continuous turning
        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(lastYRotation, currentYRotation));
        if (deltaAngle > turnThreshold)
        {
            hasTurned = true;
            lastYRotation = currentYRotation;
        }
    }
}
