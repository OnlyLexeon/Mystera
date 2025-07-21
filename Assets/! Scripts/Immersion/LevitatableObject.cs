using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LevitatableObject : MonoBehaviour
{
    private Rigidbody rb;
    private bool isLevitating = false;
    private float upwardForce;
    private float levitateDuration;
    private float levitateTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyLevitate(float frequency, float duration)
    {
        upwardForce = frequency/4;
        levitateDuration = duration;
        levitateTimer = 0f;
        isLevitating = true;

        rb.useGravity = false;
    }

    private void Update()
    {
        if (isLevitating)
        {
            levitateTimer += Time.deltaTime;

            // Apply levitation force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, upwardForce, rb.linearVelocity.z);

            // Stop levitation after the duration
            if (levitateTimer >= levitateDuration)
            {
                isLevitating = false;
                rb.useGravity = true;
            }
        }
    }
}
