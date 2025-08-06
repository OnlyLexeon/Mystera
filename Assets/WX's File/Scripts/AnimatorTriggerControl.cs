using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorTriggerControl : MonoBehaviour
{
    private Animator animator;


    void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("未找到Animator组件!", this);
        }
    }

    void Update()
    {
      
    }

    // 触发动画的公共方法
    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
            Debug.Log("触发动画: " + triggerName);
        }
    }

    // 重置所有触发器（可选）
    public void ResetAllTriggers()
    {
        animator.ResetTrigger("attack02");
        animator.ResetTrigger("attack03");
        animator.ResetTrigger("crawl");
        animator.ResetTrigger("idle02");
        animator.ResetTrigger("takeDamage");
    }
}