using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool hasCollided = false;
    private Action onPortalEnter;

    public BoxCollider boxCollider;
    public Animator animator;
    public bool isOpen = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        if (collision.collider.CompareTag("Player"))
        {
            hasCollided = true;

            onPortalEnter?.Invoke();
        }
    }

    public void SetEvent(Action action)
    {
        onPortalEnter = action;
    }

    private void OnDisable()
    {
        hasCollided = false;
    }

}
