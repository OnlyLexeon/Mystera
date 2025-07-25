using UnityEngine;

public class NPCAnimatorScript : MonoBehaviour
{
    public Animator animator;
    public void TriggerAnimation(string shapeName)
    {
        switch (shapeName)
        {
            case "Avada Kedavra":
                animator.SetTrigger("Dead");
                break;
            case "Crucio":
                animator.SetTrigger("Agony");
                break;
            case "Stupify":
                animator.SetTrigger("StopMoving");
                break;
        }
    }
}
