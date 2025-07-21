using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class MovementManager : MonoBehaviour
{
    public enum MovementMode
    {
        Continuous,
        Teleport
    }

    [Header("Movement Mode")]
    public MovementMode currentMode = MovementMode.Continuous;

    [Header("References")]
    public ContinuousMoveProvider continuousMoveProvider;
    public TeleportationProvider teleportationProvider;
    public GameObject leftTeleportRay;
    public XRRayInteractor leftRayInteractor;
    public Transform xrCamera; // The Main Camera under Camera Offset

    [Header("Input")]
    public InputActionProperty teleportActivate;  // New input action (e.g., left thumbstick up)
    public InputActionProperty teleportCancel;    // New input action (e.g., left grip)

    [Header("Events")]
    public UnityEvent OnTeleport;

    private bool teleportRayActive = false;

    private void OnEnable()
    {
        teleportActivate.action.performed += OnTeleportActivate;
        teleportActivate.action.canceled += OnTeleportRelease;

        teleportCancel.action.performed += OnTeleportCancel;

        teleportActivate.action.Enable();
        teleportCancel.action.Enable();
    }

    private void OnDisable()
    {
        teleportActivate.action.performed -= OnTeleportActivate;
        teleportActivate.action.canceled -= OnTeleportCancel;

        teleportCancel.action.performed -= OnTeleportRelease;

        teleportActivate.action.Disable();
        teleportCancel.action.Disable();
    }

    private void Update()
    {
        switch (currentMode)
        {
            case MovementMode.Continuous:
                continuousMoveProvider.enabled = true;
                teleportationProvider.enabled = false;
                leftTeleportRay.SetActive(false);
                break;

            case MovementMode.Teleport:
                continuousMoveProvider.enabled = false;
                teleportationProvider.enabled = true;
                break;
        }
    }

    private void OnTeleportActivate(InputAction.CallbackContext context)
    {
        if (currentMode != MovementMode.Teleport) return;

        Debug.Log("Teleport Ray Active!");
        teleportRayActive = true;
        leftTeleportRay.SetActive(true);
    }

    private void OnTeleportRelease(InputAction.CallbackContext context)
    {
        if (!teleportRayActive) return;

        if (leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Check if it's a valid teleport target
            var teleportTarget = hit.transform.GetComponent<TeleportationArea>();

            if (teleportTarget != null && IsTeleportLocationClear(hit.point))
            {
                Debug.Log("Teleporting to valid area...");
                var request = new TeleportRequest
                {
                    destinationPosition = hit.point,
                    destinationRotation = Quaternion.Euler(0, xrCamera.eulerAngles.y, 0),
                    matchOrientation = MatchOrientation.None
                };
                teleportationProvider.QueueTeleportRequest(request);

                Player.instance.ForceTeleport(hit.point);

                OnTeleport?.Invoke();
            }
            else
            {
                Debug.Log("Invalid teleport target.");
            }
        }

        teleportRayActive = false;
        leftTeleportRay.SetActive(false);
    }

    private void OnTeleportCancel(InputAction.CallbackContext context)
    {
        if (!teleportRayActive) return;

        Debug.Log("Teleport Canceled.");
        teleportRayActive = false;
        leftTeleportRay.SetActive(false);
    }

    public void SetMovementModeDropdown(int index)
    {
        currentMode = (MovementMode)index;
        Debug.Log($"Movement mode set to {currentMode}");
    }

    private bool IsTeleportLocationClear(Vector3 destination)
    {
        CharacterController cc = Player.instance.GetComponent<CharacterController>();
        float radius = cc.radius;
        float height = cc.height;

        float groundOffset = 0.05f; // Small offset to prevent clipping into the ground

        // Apply offset to lift the capsule slightly above the ground
        destination += Vector3.up * groundOffset;

        // Calculate capsule points based on character height and radius
        Vector3 bottom = destination + Vector3.up * radius ;
        Vector3 top = bottom + Vector3.up * (height - 2 * radius);

        return !Physics.CheckCapsule(bottom, top, radius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
    }

}
