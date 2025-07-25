using UnityEngine;

public class NPCAnimScript : MonoBehaviour
{
    private Animator animator;
    public bool dead = false;
    public CapsuleCollider npc_collider;

    private void Start()
    {
        npc_collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
    }
    public void GetHit()
    {
        animator.SetTrigger("Hit");
    }
    public void GetKilled()
    {
        npc_collider.enabled = false;
        animator.SetTrigger("Dead");
        dead = true;
    }
    public void Retreat()
    {
        if (!dead)
        {
            animator.SetTrigger("Retreat");
            Destroy(gameObject, 2);
        }
    }
}
