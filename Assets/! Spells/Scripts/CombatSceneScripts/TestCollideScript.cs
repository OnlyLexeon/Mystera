using UnityEngine;

public class TestCollideScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(gameObject.name + " collided with " + collision.collider.name);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " collided trigger with " + other.name);
    }
}
