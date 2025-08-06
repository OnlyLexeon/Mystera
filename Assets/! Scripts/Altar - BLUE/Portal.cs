using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool hasCollided = false;
    private Action onPortalEnter;

    public BoxCollider boxCollider;
    public Animator animator;
    public bool isOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        if (other.CompareTag("Player"))
        {
            hasCollided = true;

            onPortalEnter?.Invoke();
            Debug.Log("Invoked!");
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
