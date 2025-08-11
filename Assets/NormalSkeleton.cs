using UnityEngine;

public class NormalSkeleton : Enemy
{
    [Header("Skeleton Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab;  // 死亡特效预制体
    [SerializeField] private float deathEffectDuration = 3f;  // 特效持续时间
    [SerializeField] private Vector3 deathEffectOffset = Vector3.up * 0.5f;  // 特效位置偏移
    [SerializeField] private bool dropBones = true;  // 是否掉落骨头
    [SerializeField] private GameObject bonePilePrefab;  // 骨头堆预制体（可选）

    [Header("Skeleton Combat Settings")]
    [SerializeField] private float boneRattleChance = 0.3f;  // 骨头咔哒声概率
    [SerializeField] private AudioClip[] boneRattleSounds;  // 骨头咔哒声音效

    protected override void Start()
    {
        // 骷髅兵也可以选择是否立即检测
        // SetImmediateDetection();  // 如果需要立即检测，取消注释

        base.Start();

        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} initialized");
        }
    }

    // 重写死亡方法
    protected override void OnDeath()
    {
        base.OnDeath();

        // 播放死亡特效
        PlayDeathEffect();

        // 如果需要掉落骨头堆
        if (dropBones && bonePilePrefab != null)
        {
            DropBonePile();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} death sequence completed");
        }
    }

    // 播放死亡特效
    private void PlayDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            // 计算特效生成位置
            Vector3 effectPosition = transform.position + deathEffectOffset;

            // 实例化死亡特效（例如：骨头飞散效果）
            GameObject deathEffect = Instantiate(deathEffectPrefab, effectPosition, transform.rotation);

            // 如果特效有粒子系统，确保它会自动播放
            ParticleSystem ps = deathEffect.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
            }

            // 如果特效有动画组件
            Animator effectAnimator = deathEffect.GetComponent<Animator>();
            if (effectAnimator != null)
            {
                effectAnimator.Play("Death", 0, 0);
            }

            // 销毁特效
            Destroy(deathEffect, deathEffectDuration);

            if (enableDebugLogs)
            {
                Debug.Log($"[SKELETON] Death effect spawned at {effectPosition}");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[SKELETON] No death effect prefab assigned for {gameObject.name}");
        }
    }

    // 掉落骨头堆
    private void DropBonePile()
    {
        Vector3 dropPosition = transform.position;
        Quaternion dropRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject bonePile = Instantiate(bonePilePrefab, dropPosition, dropRotation);

        // 可以给骨头堆添加一些物理效果
        Rigidbody rb = bonePile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] Bone pile dropped at {dropPosition}");
        }
    }

    // 重写获取死亡延迟时间
    protected override float GetDeathDelay()
    {
        // 骷髅兵崩解动画可能需要更长时间
        return 2.5f;
    }

    // 重写近战攻击，添加骷髅兵特色
    protected override void PerformMeleeAttack()
    {
        base.PerformMeleeAttack();

        // 有概率播放骨头咔哒声
        if (Random.value < boneRattleChance)
        {
            PlayBoneRattleSound();
        }
    }

    // 播放骨头咔哒声
    private void PlayBoneRattleSound()
    {
        if (boneRattleSounds != null && boneRattleSounds.Length > 0 && CanPlaySound())
        {
            AudioClip clip = boneRattleSounds[Random.Range(0, boneRattleSounds.Length)];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume * 0.7f);
            }
        }
    }

    // 重写受伤方法，添加骷髅兵特有效果
    public override void OnTakeDamage(int damage)
    {
        base.OnTakeDamage(damage);

        // 骷髅兵受伤时的特殊效果
        SkeletonHitEffect();
    }

    // 骷髅兵受击效果
    private void SkeletonHitEffect()
    {
        // 播放骨头碰撞声
        PlayBoneRattleSound();

        // 可以添加骨头碎片飞出的效果
        if (!isDead)
        {
            // 轻微的骨架晃动
            StartCoroutine(BoneShakeAnimation());
        }
    }

    // 骨架晃动动画
    private System.Collections.IEnumerator BoneShakeAnimation()
    {
        float shakeDuration = 0.15f;
        float shakeIntensity = 0.1f;
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeIntensity, shakeIntensity);
            float z = Random.Range(-shakeIntensity, shakeIntensity);
            transform.localPosition = originalPosition + new Vector3(x, 0, z);
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    // 重写巡逻更新，让骷髅兵巡逻时偶尔发出声音
    protected override void PatrolUpdate()
    {
        base.PatrolUpdate();

        // 偶尔发出骨头声
        if (Random.value < 0.001f) // 很低的概率
        {
            PlayBoneRattleSound();
        }
    }

    // 在编辑器中绘制额外的调试信息
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // 绘制死亡特效生成位置
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + deathEffectOffset, Vector3.one * 0.5f);
    }
}