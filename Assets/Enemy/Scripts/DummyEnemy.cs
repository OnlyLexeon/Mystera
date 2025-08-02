using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DummyEnemy : Enemy
{
    [Header("Hit Effect Settings")]
    [Tooltip("受击时生成的特效预制体")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("特效生成的高度偏移")]
    public float effectHeightOffset = 1.5f;
    
    [Tooltip("特效生成的水平偏移距离")]
    public float effectHorizontalOffset = 1f;
    
    [Tooltip("特效向前偏移（相对于靶子朝向）")]
    public float effectForwardOffset = 0f;
    
    [Tooltip("特效存在时间（0表示不自动销毁）")]
    public float effectLifetime = 2f;
    
    [Tooltip("是否让特效面向攻击者")]
    public bool effectFaceAttacker = true;
    
    [Tooltip("特效的缩放")]
    public Vector3 effectScale = Vector3.one;
    
    // 记录下次特效应该生成在哪一侧
    private bool spawnOnRightSide = true;
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // 禁用NavMeshAgent，靶子不移动
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // 靶子也使用Enemy标签，这样其他系统可以识别它是可攻击目标
        // gameObject.tag = "Enemy"; // 基类已经设置了
    }
    
    protected override void SetupInitialState()
    {
        // 靶子始终保持待机状态，不巡逻
        currentState = State.Patrol;
        // 不调用基类的SetupInitialState，避免启动巡逻
        // 不需要设置速度，因为靶子不移动
    }
    
    protected override void Start()
    {
        InitializeComponents();
        // 直接调用自己的SetupInitialState，不调用基类的Start
        SetupInitialState();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[DUMMY] {gameObject.name} initialized as training dummy");
        }
    }
    
    protected override void Update()
    {
        // 靶子不需要检测目标或更新状态
        // 只更新动画
        if (!isDead && animator != null)
        {
            animator.SetBool(isMovingParam, false); // 始终不移动
        }
    }
    
    protected override void DetectTargets()
    {
        // 靶子不检测目标
    }
    
    protected override void EvaluateAndSelectTarget()
    {
        // 靶子不选择目标
    }
    
    protected override void ChaseUpdate()
    {
        // 靶子不追击
    }
    
    protected override void PatrolUpdate()
    {
        // 靶子不巡逻，保持站立
    }
    
    protected override void AlertNearbyAllies(Transform attacker)
    {
        // 靶子不会警告其他单位
    }
    
    public override void OnAllyAlert(Transform threat, Vector3 alertSource)
    {
        // 靶子不响应警报
    }
    
    public override void SetHateTarget(Transform attacker)
    {
        // 靶子不会产生仇恨
    }
    
    protected override void OnTakeDamageWithAttacker(int damage, GameObject attacker)
    {
        if (isDead) return;
        
        // 记录伤害信息
        if (enableDebugLogs)
        {
            Debug.Log($"[DUMMY] {gameObject.name} 受到 {damage} 点伤害，来自 {(attacker != null ? attacker.name : "未知")}");
        }
        
        // 播放受击动画（不眩晕）
        if (!string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);
        }
        
        // 生成受击特效（左右交替）
        SpawnHitEffect(attacker);
        
        // 生成原有的粒子效果
        CreateHitParticles();
        StartCoroutine(FlashRed());
    }
    
    private void SpawnHitEffect(GameObject attacker)
    {
        if (hitEffectPrefab == null) return;
        
        // 计算特效生成位置
        Vector3 spawnPosition = CalculateEffectPosition();
        
        // 生成特效
        GameObject effect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
        
        // 设置特效缩放
        effect.transform.localScale = effectScale;
        
        // 如果需要，让特效面向攻击者
        if (effectFaceAttacker && attacker != null)
        {
            Vector3 lookDirection = attacker.transform.position - effect.transform.position;
            lookDirection.y = 0; // 保持水平朝向
            if (lookDirection != Vector3.zero)
            {
                effect.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        else
        {
            // 否则与靶子朝向一致
            effect.transform.rotation = transform.rotation;
        }
        
        // 如果设置了生命周期，自动销毁
        if (effectLifetime > 0)
        {
            Destroy(effect, effectLifetime);
        }
        
        // 切换下次生成的位置
        spawnOnRightSide = !spawnOnRightSide;
        
        if (enableDebugLogs)
        {
            string side = spawnOnRightSide ? "右侧" : "左侧";
            Debug.Log($"[DUMMY] 特效生成在{side}，位置: {spawnPosition}");
        }
    }
    
    private Vector3 CalculateEffectPosition()
    {
        // 基础位置（靶子位置 + 高度偏移）
        Vector3 basePosition = transform.position + Vector3.up * effectHeightOffset;
        
        // 计算水平偏移方向（左或右）
        Vector3 rightDirection = transform.right;
        float horizontalOffset = spawnOnRightSide ? effectHorizontalOffset : -effectHorizontalOffset;
        
        // 添加向前偏移
        Vector3 forwardDirection = transform.forward;
        
        // 最终位置
        Vector3 finalPosition = basePosition + 
                               rightDirection * horizontalOffset + 
                               forwardDirection * effectForwardOffset;
        
        return finalPosition;
    }
    
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log($"[DUMMY] {gameObject.name} 被击破");
        
        // 播放死亡动画
        if (!string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
        }
        
        // 禁用碰撞
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        // 可选：在死亡时生成一个特殊的特效
        if (hitEffectPrefab != null)
        {
            // 在中心位置生成一个特效
            Vector3 deathEffectPos = transform.position + Vector3.up * effectHeightOffset;
            GameObject deathEffect = Instantiate(hitEffectPrefab, deathEffectPos, transform.rotation);
            deathEffect.transform.localScale = effectScale * 1.5f; // 死亡特效稍大一些
            
            if (effectLifetime > 0)
            {
                Destroy(deathEffect, effectLifetime);
            }
        }
        
        // 延迟销毁，给死亡动画时间
        Destroy(gameObject, 2f);
    }
    
    protected override void OnDrawGizmosSelected()
    {
        // 绘制基本的碰撞范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up, Vector3.one * 2f);
        
        // 绘制特效生成位置预览
        if (Application.isPlaying)
        {
            // 显示下一个特效将生成的位置
            Gizmos.color = spawnOnRightSide ? Color.green : Color.blue;
            Vector3 nextEffectPos = CalculateEffectPosition();
            Gizmos.DrawWireSphere(nextEffectPos, 0.3f);
            
            // 绘制一条从靶子到特效位置的线
            Gizmos.DrawLine(transform.position + Vector3.up, nextEffectPos);
        }
        else
        {
            // 在编辑器中显示两个可能的位置
            Vector3 basePos = transform.position + Vector3.up * effectHeightOffset;
            
            // 右侧位置
            Gizmos.color = Color.green;
            Vector3 rightPos = basePos + transform.right * effectHorizontalOffset + transform.forward * effectForwardOffset;
            Gizmos.DrawWireSphere(rightPos, 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up, rightPos);
            
            // 左侧位置
            Gizmos.color = Color.blue;
            Vector3 leftPos = basePos - transform.right * effectHorizontalOffset + transform.forward * effectForwardOffset;
            Gizmos.DrawWireSphere(leftPos, 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up, leftPos);
        }
        
        // 标记文字
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, "Training Dummy");
        #endif
    }
    
    // 公开方法：手动重置特效生成侧
    [ContextMenu("Reset Effect Side")]
    public void ResetEffectSide()
    {
        spawnOnRightSide = true;
        Debug.Log("[DUMMY] 特效生成侧已重置为右侧");
    }
    
    // 公开方法：测试特效生成
    [ContextMenu("Test Spawn Effect")]
    public void TestSpawnEffect()
    {
        SpawnHitEffect(null);
    }
}