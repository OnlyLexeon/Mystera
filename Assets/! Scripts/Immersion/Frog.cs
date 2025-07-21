using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Frog : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float forwardForce = 2f;
    public float sideForce = 1f;
    public float minJumpTime = 1f;
    public float maxJumpTime = 3f;

    private Rigidbody rb;

    [Header("Ribbit Settings")]
    public AudioSource audioSource;
    public float minCroakTime = 1f;
    public float maxCroakTime = 3f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(JumpRoutine());

        StartCoroutine(CroakRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minJumpTime, maxJumpTime);
            yield return new WaitForSeconds(waitTime);
            Jump();
        }
    }
    private IEnumerator CroakRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minCroakTime, maxCroakTime);
            yield return new WaitForSeconds(waitTime);
            Croak();
        }
    }

    private void Croak()
    {
        audioSource.Play();
    }

    private void Jump()
    {
        if (rb == null) return;

        Croak();
        // Reset vertical velocity to make jump consistent
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Random left/right side force
        float sideOffset = Random.Range(-sideForce, sideForce) + 1f;
        Vector3 sideDirection = transform.right * sideOffset;

        // Apply upward and forward impulse
        Vector3 jumpDirection = transform.up * jumpForce + transform.forward * forwardForce;
        rb.AddForce(jumpDirection, ForceMode.Impulse);
    }
}
