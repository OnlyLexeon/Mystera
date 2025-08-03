using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class MovementManager : MonoBehaviour
{
    public enum MovementMode
    {
        Continuous,
        Teleport
    }

    [Header("Movement Mode")]
    public MovementMode currentMode = MovementMode.Continuous;

    [Header("Cooldown")]
    public float teleportCooldown = 2f;
    private float lastTeleportTime = -Mathf.Infinity;

    [Header("References")]
    public TMP_Dropdown moveModeDropdown;
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

    private const string MoveModeKey = "MoveMode";

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

    void Start()
    {
        LoadMoveMode();
        moveModeDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    public void OnDropdownChanged(int index)
    {
        SetMoveMode((MovementMode)index);
        SaveMoveMode(index);
    }

    private void SetMoveMode(MovementMode mode)
    {
        switch (mode)
        {
            case MovementMode.Continuous:
                if (teleportationProvider != null) teleportationProvider.enabled = false;
                if (continuousMoveProvider != null) continuousMoveProvider.enabled = true;
                leftTeleportRay.SetActive(false);
                break;
            case MovementMode.Teleport:
                if (teleportationProvider != null) teleportationProvider.enabled = true;
                if (continuousMoveProvider != null) continuousMoveProvider.enabled = false;
                break;
        }
    }

    private void SaveMoveMode(int index)
    {
        PlayerPrefs.SetInt(MoveModeKey, index);
        PlayerPrefs.Save();
    }

    private void LoadMoveMode()
    {
        int savedIndex = PlayerPrefs.GetInt(MoveModeKey, (int)MovementMode.Continuous);
        moveModeDropdown.value = savedIndex;
        SetMoveMode((MovementMode)savedIndex);
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

        //cooldown
        if (Time.time < lastTeleportTime + teleportCooldown)
        {
            Debug.Log("[Teleport] Cooldown active.");
            teleportRayActive = false;
            leftTeleportRay.SetActive(false);
            return;
        }

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
        float maxSlopeAngle = 60f;

        //raycast from above to get normal
        if (Physics.Raycast(destination + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle > maxSlopeAngle)
            {
                Debug.Log($"[Teleport] Slope too steep: {angle}°");
                return false;
            }

            //adjust bottom/top based on hit point
            Vector3 bottom = hit.point + Vector3.up * radius;
            Vector3 top = bottom + Vector3.up * (height - 2 * radius);

            bool blocked = Physics.CheckCapsule(bottom, top, radius, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            if (blocked)
            {
                Debug.Log("[Teleport] Capsule check blocked.");
                return false;
            }

            return true;
        }

        Debug.Log("[Teleport] No surface detected at target.");
        return false;
    }


}
