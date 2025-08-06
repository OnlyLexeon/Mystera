using UnityEngine;

public class CameraDamageRaycast : MonoBehaviour
{
    [Header("伤害设置")]
    [Tooltip("每次点击造成的伤害值")]
    public float damageAmount = 10f;
    [Tooltip("射线检测距离")]
    public float raycastDistance = 100f;
    [Tooltip("伤害触发器的名称")]
    public string damageTriggerName = "takeDamage";

    [Header("视觉效果")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("脚本需要挂载在带有Camera组件的物体上!", this);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 检测鼠标左键点击
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                // 检查击中的物体是否有生命值组件
                Health health = hit.collider.GetComponent<Health>();
                if (health != null)
                {
                    // 造成伤害
                    health.TakeDamage((int)damageAmount);

                    // 播放命中效果
                    PlayHitEffects(hit.point);

                    // 尝试触发受击动画
                    Animator animator = hit.collider.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(damageTriggerName);
                    }
                    else
                    {
                        // 如果没有直接找到Animator，可能在父对象上
                        animator = hit.collider.GetComponentInParent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger(damageTriggerName);
                        }
                    }
                }
            }
        }
    }

    void PlayHitEffects(Vector3 position)
    {
        // 播放粒子效果
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }

        // 播放音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }
}