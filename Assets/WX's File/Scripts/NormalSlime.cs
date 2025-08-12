using UnityEngine;

public class NormalSlime : Enemy
{
    [Header("Slime Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab;  // 死亡特效预制体
    [SerializeField] private float deathEffectDuration = 3f;  // 特效持续时间
    [SerializeField] private Vector3 deathEffectOffset = Vector3.zero;  // 特效位置偏移
    [SerializeField] private bool hideBodyOnDeath = true;  // 死亡时是否隐藏本体

    protected override void Start()
    {
        // 史莱姆立即开始检测目标
        SetImmediateDetection();

        base.Start();

        if (enableDebugLogs)
        {
            Debug.Log($"[SLIME] {gameObject.name} initialized with immediate detection");
        }
    }

    protected override System.Collections.IEnumerator FlashRed()
{
    Renderer[] renderers = GetComponentsInChildren<Renderer>();
    Color[] originalColors = new Color[renderers.Length];
    
    // 保存原始颜色
    for (int i = 0; i < renderers.Length; i++)
    {
        if (renderers[i]?.material != null)
        {
            originalColors[i] = renderers[i].material.color;
            // 使用白色更明显
            renderers[i].material.color = Color.white;
        }
    }
    
    yield return new WaitForSeconds(0.1f);
    
    // 恢复原始颜色
    for (int i = 0; i < renderers.Length; i++)
    {
        if (renderers[i]?.material != null)
        {
            renderers[i].material.color = originalColors[i];
        }
    }
}

    // 重写死亡方法
    protected override void OnDeath()
    {
        base.OnDeath();

        // 播放死亡特效
        PlayDeathEffect();

        // 如果需要隐藏本体
        if (hideBodyOnDeath)
        {
            HideSlimeBody();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[SLIME] {gameObject.name} death sequence completed");
        }
    }

    // 播放死亡特效
    private void PlayDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            // 计算特效生成位置
            Vector3 effectPosition = transform.position + deathEffectOffset;

            // 实例化死亡特效
            GameObject deathEffect = Instantiate(deathEffectPrefab, effectPosition, Quaternion.identity);

            // 如果特效有粒子系统，确保它会自动播放
            ParticleSystem ps = deathEffect.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
            }

            // 如果特效有动画组件，确保播放
            Animator effectAnimator = deathEffect.GetComponent<Animator>();
            if (effectAnimator != null)
            {
                effectAnimator.Play("Death", 0, 0);
            }

            // 销毁特效
            Destroy(deathEffect, deathEffectDuration);

            if (enableDebugLogs)
            {
                Debug.Log($"[SLIME] Death effect spawned at {effectPosition}");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[SLIME] No death effect prefab assigned for {gameObject.name}");
        }
    }

    // 隐藏史莱姆本体
    private void HideSlimeBody()
    {
        // 隐藏所有渲染器
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // 禁用阴影
        Light[] lights = GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.enabled = false;
        }
    }

    // 重写获取死亡延迟时间
    protected override float GetDeathDelay()
    {
        // 如果隐藏了本体，可以稍微延长销毁时间，确保特效播放完成
        if (hideBodyOnDeath)
        {
            return Mathf.Max(2f, deathEffectDuration + 0.5f);
        }

        // 否则返回默认的2秒
        return 2f;
    }

    // 可选：重写受伤方法添加史莱姆特有的效果
    public override void OnTakeDamage(int damage)
    {
        base.OnTakeDamage(damage);

        // 史莱姆受伤时的特殊效果
        SlimeBounceEffect();
    }

    // 史莱姆弹跳效果
    private void SlimeBounceEffect()
    {
        // 简单的缩放动画模拟弹跳
        if (transform != null && !isDead)
        {
            StartCoroutine(BounceAnimation());
        }
    }

    // 弹跳动画协程
    private System.Collections.IEnumerator BounceAnimation()
    {
        Vector3 originalScale = transform.localScale;
        float bounceTime = 0.2f;
        float elapsed = 0f;

        // 压扁
        while (elapsed < bounceTime / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (bounceTime / 2f);
            transform.localScale = Vector3.Lerp(originalScale,
                new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z * 1.2f),
                t);
            yield return null;
        }

        elapsed = 0f;

        // 恢复
        while (elapsed < bounceTime / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (bounceTime / 2f);
            transform.localScale = Vector3.Lerp(
                new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z * 1.2f),
                originalScale,
                t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    // 在编辑器中绘制额外的调试信息
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // 绘制死亡特效生成位置
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + deathEffectOffset, Vector3.one * 0.5f);
    }
}