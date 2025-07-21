using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Fire")]
    public GameObject[] onFireObjects;

    [Header("Glow")]
    public GameObject onGlow;

    [Header("Jumping")]
    public float defaultJumpHeight = 2f;
    public LayerMask groundMask;

    [Header("Gravity")]
    public float defaultGravity = Physics.gravity.y;

    [Header("Levitation")]
    public bool isLevitating = false;
    private float levitateForce = 0f;

    [Header("Debug")]
    public float currentJumpHeight = 0f;
    public float currentGravity = 0f;

    private CharacterController controller;
    private Vector3 movement;
    private bool _isGrounded;

    public static Player instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentJumpHeight = defaultJumpHeight;
        currentGravity = defaultGravity;
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _isGrounded = IsGrounded();

        // Apply levitation or gravity
        if (isLevitating)
        {
            movement.y = levitateForce;
        }
        else //gravity
        {
            movement.y += currentGravity * Time.deltaTime;
        }

        controller.Move(movement * Time.deltaTime);
    }

    public bool IsGrounded()
    {
        return controller.isGrounded;
    }

    public void ForceTeleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        movement = Vector3.zero;
        controller.enabled = true;
    }

    public void SetJumpHeight(float value)
    {
        currentJumpHeight = value;
    }

    public void ResetJumpHeight()
    {
        currentJumpHeight = defaultJumpHeight;
    }

    public void SetGravity(float value)
    {
        currentGravity = value;
    }

    public void ResetGravity()
    {
        currentGravity = defaultGravity;
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            movement.y = Mathf.Sqrt(currentJumpHeight * -3.0f * currentGravity);
        }
    }

    public void EnableFire()
    {
        foreach (GameObject child in onFireObjects)
        {
            child.SetActive(true);
        }
    }

    public void DisableFire()
    {
        foreach (GameObject child in onFireObjects)
        {
            child.SetActive(false);
        }
    }

    public void EnableGlow(float range, float intensity)
    {
        onGlow.SetActive(true);
        Light light = onGlow.GetComponent<Light>();

        light.range = range + 1;
        light.intensity = intensity + 1;
    }

    public void DisableGlow()
    {
        onGlow.SetActive(false);
    }

    public void ApplyLevitate(float frequency, float duration)
    {
        levitateForce = frequency / 4;
        isLevitating = true;

        SetGravity(0f); // Disable gravity during levitation
    }

    public void RemoveLevitate()
    {
        levitateForce = 0;
        isLevitating = false;

        ResetGravity();
    }
}
