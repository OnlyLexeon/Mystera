using UnityEngine;
using System.Collections;

public class SkeletonEnemy : Enemy 
{
    [Header("Skeleton Revive Settings")]
    public bool canRevive = true;
    public float reviveDelay = 10f;
    public float reviveEffectDuration = 3f;
    public GameObject reviveEffectPrefab;
    public AudioClip reviveSound;
    public string rebirthTrigger = "rebirth";
    public float reviveHealthPercent = 0.5f;
   
    
    // 复活状态
    private bool hasRevived = false;
    private bool isReviving = false;
    private GameObject currentReviveEffect;
    private Coroutine invincibilityCoroutine;
    
    // 缓存组件
    private Collider myCollider;
    private Renderer myRenderer;
    
    protected override void Start()
    {
        base.Start();
        
        // 缓存组件
        myCollider = GetComponent<Collider>();
        myRenderer = GetComponentInChildren<Renderer>();
        
        // 确保标签正确
        gameObject.tag = "Enemy";
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} initialized - " +
                $"Can Revive: {canRevive}, " +
                $"Tag: {gameObject.tag}, " +
                $"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        }
    }
    
    protected override void Update()
    {
        // 如果正在复活或已死亡（但还没复活），跳过基类更新
        if (isReviving || (isDead && !hasRevived && canRevive))
        {
            if (enableDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[SKELETON DEBUG] {gameObject.name} - Reviving: {isReviving}, Dead: {isDead}");
            }
            return;
        }
        
        base.Update();
    }
    
    protected override void UpdateStateMachine()
    {
        // 复活中不更新状态机
        if (isReviving || (isDead && !hasRevived && canRevive))
        {
            return;
        }
        
        base.UpdateStateMachine();
    }
    
    protected override void Die()
    {
        // 只处理第一次死亡的判断，具体死亡逻辑交给FirstDeath或FinalDeath处理
        if (!hasRevived && canRevive)
        {
            FirstDeath();
        }
        else
        {
            FinalDeath();
        }
    }
    
    private void FirstDeath()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} first death at position {transform.position}");
        }
        
        // 停止所有战斗协程
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }
        
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }
        
        // 停止巡逻协程
        StopCoroutine("PatrolRoutine");
        
        // 设置状态
        currentState = State.Dead;
        isDead = true;
        
        // 禁用NavMeshAgent对Transform的控制
        agent.updatePosition = false;
        agent.updateRotation = false;
        
        // 播放死亡动画
        animator.SetTrigger(deathTrigger);
        
        // 禁用碰撞器
        if (myCollider != null) 
        {
            myCollider.enabled = false;
        }
        
        // 清空目标
        currentTarget = null;
        hateTarget = null;
        
        // 启动复活协程
        StartCoroutine(ReviveRoutine());
    }
    
    private IEnumerator ReviveRoutine()
    {
        yield return new WaitForSeconds(reviveDelay);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} starting revive at position {transform.position}");
        }
        
        isReviving = true;
        
        // 创建复活特效
        if (reviveEffectPrefab != null)
        {
            currentReviveEffect = Instantiate(reviveEffectPrefab, transform.position, Quaternion.identity);
            currentReviveEffect.transform.SetParent(transform);
        }
        
        // 播放复活音效
        if (reviveSound != null)
        {
            AudioSource.PlayClipAtPoint(reviveSound, transform.position);
        }
        
        // 触发复活动画
        if (!string.IsNullOrEmpty(rebirthTrigger))
        {
            animator.SetTrigger(rebirthTrigger);
        }
        
        // 等待复活特效
        yield return new WaitForSeconds(reviveEffectDuration);
        
        // 清理特效
        if (currentReviveEffect != null)
        {
            Destroy(currentReviveEffect);
            currentReviveEffect = null;
        }
        
        Revive();
    }
    
    private void Revive()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} reviving at position {transform.position}");
        }
        
        // 更新状态标记
        hasRevived = true;
        isReviving = false;
        isDead = false;
        isInvincible = false;
        
        // 重置Health组件状态并恢复生命值
        if (health != null)
        {
            // 重置死亡状态
            health.ResetDeathState();
            
            // 设置复活后的生命值
            int reviveHealth = Mathf.RoundToInt(health.maxHealth * reviveHealthPercent);
            health.SetHealth(reviveHealth);
            
            // 重新订阅Health事件
            health.OnDeath -= Die;
            health.OnDamagedByAttacker -= OnTakeDamageWithAttacker;
            health.OnDeath += Die;
            health.OnDamagedByAttacker += OnTakeDamageWithAttacker;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[SKELETON] Health reset complete. Current health: {health.currentHealth}/{health.maxHealth}");
            }
        }
        
        // 重新启用碰撞器
        if (myCollider != null) 
        {
            myCollider.enabled = true;
        }
        
        // 重置目标和状态
        currentTarget = null;
        hateTarget = null;
        isAlerted = false;
        isStunned = false;
        
        // 清空目标记录
        potentialTargets.Clear();
        targetLastSeenTime.Clear();
        
        // 重新启用NavMeshAgent对Transform的控制
        agent.updatePosition = true;
        agent.updateRotation = true;
        
        // 同步NavMeshAgent位置
        if (agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }
        
        // 设置为巡逻状态
        currentState = State.Patrol;
        
        // 重置动画
        animator.SetBool(isMovingParam, false);
        animator.ResetTrigger(deathTrigger);
        animator.ResetTrigger(rebirthTrigger);
        
        // 确保脚本启用
        enabled = true;
        
        // 重置检测时间，立即开始检测
        lastDetectionTime = -detectionInterval;
        
        // 立即进行一次目标检测
        DetectTargets();
        
        // 开始巡逻
        StartCoroutine(PatrolRoutine());
        
        // 复活无敌
        if (invincibilityDuration > 0)
        {
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
            }
            invincibilityCoroutine = StartCoroutine(ReviveInvincibility());
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} revival complete - " +
                $"State: {currentState}, " +
                $"Enabled: {enabled}, " +
                $"isDead: {isDead}, " +
                $"isInvincible: {isInvincible}, " +
                $"Health.IsAlive(): {health?.IsAlive() ?? false}, " +
                $"Targets detected: {potentialTargets.Count}");
        }
    }
    
    private void FinalDeath()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} final death");
        }
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 确保无敌状态被清除
        isInvincible = false;
        invincibilityCoroutine = null;
        
        // 标记为已死亡
        isDead = true;
        currentState = State.Dead;
        
        // 停止移动
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        
        // 禁用NavMeshAgent对Transform的控制
        agent.updatePosition = false;
        agent.updateRotation = false;
        
        // 播放死亡动画
        animator.SetTrigger(deathTrigger);
        
        // 禁用碰撞器
        if (myCollider != null) 
        {
            myCollider.enabled = false;
        }
        
        // 触发死亡事件
        OnDeath();
        
        // 延迟销毁（给死亡动画时间）
        Destroy(gameObject, GetDeathDelay());
    }
    
    protected override void OnDeath()
    {
        if (hasRevived || !canRevive)
        {
            // 最终死亡时的逻辑（掉落物品等）
            if (enableDebugLogs)
            {
                Debug.Log($"[SKELETON] {gameObject.name} dropped loot");
            }
        }
    }
    
    protected override float GetDeathDelay()
    {
        // 第一次死亡不销毁对象
        if (!hasRevived && canRevive)
        {
            return float.MaxValue;
        }
        
        // 第二次死亡，给足够的时间播放死亡动画
        return 3f;
    }
    
    public override void OnTakeDamageWithAttacker(int damage, GameObject attacker)
    {
        // 死亡状态不受伤害
        if (isDead)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SKELETON] {gameObject.name} is dead, damage blocked");
            }
            return;
        }
        
        // 无敌或复活中不受伤害
        if (isInvincible || isReviving)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SKELETON] {gameObject.name} damage blocked - Invincible: {isInvincible}, Reviving: {isReviving}");
            }
            return;
        }
        
        // 调用基类的受伤逻辑
        base.OnTakeDamageWithAttacker(damage, attacker);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} took {damage} damage from {(attacker != null ? attacker.name : "unknown")}. Health: {health.currentHealth}/{health.maxHealth}");
        }
    }
    
    private IEnumerator ReviveInvincibility()
    {
        isInvincible = true;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} invincibility started for {invincibilityDuration} seconds");
        }
        
        // 闪烁效果
        if (myRenderer != null)
        {
            float elapsed = 0f;
            
            while (elapsed < invincibilityDuration)
            {
                myRenderer.enabled = !myRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            myRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }
        
        isInvincible = false;
        invincibilityCoroutine = null;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SKELETON] {gameObject.name} invincibility ended");
        }
    }
    
    public void PreventRevive()
    {
        canRevive = false;
        hasRevived = true;
        
        if (isReviving)
        {
            StopAllCoroutines();
            if (currentReviveEffect != null)
            {
                Destroy(currentReviveEffect);
            }
            
            FinalDeath();
        }
    }
    
    // 测试受伤方法
    [ContextMenu("Test Take Damage")]
    public void TestTakeDamage()
    {
        Debug.Log($"[TEST] Testing damage on {gameObject.name}");
        Debug.Log($"[TEST] Current states - isDead: {isDead}, isInvincible: {isInvincible}, isReviving: {isReviving}");
        Debug.Log($"[TEST] Health.IsAlive(): {health?.IsAlive() ?? false}");
        
        if (health != null)
        {
            Debug.Log($"[TEST] Before damage: {health.currentHealth}/{health.maxHealth}");
            
            // 直接调用Health的TakeDamage来测试
            health.TakeDamage(10, gameObject);
            
            Debug.Log($"[TEST] After damage: {health.currentHealth}/{health.maxHealth}");
        }
        else
        {
            Debug.LogError("[TEST] Health component is null!");
        }
    }
    
    // 调试方法
    public void DebugCheckStatus()
    {
        Debug.Log($"[SKELETON DEBUG] {gameObject.name} Status Check:");
        Debug.Log($"  - Enabled: {enabled}");
        Debug.Log($"  - isDead: {isDead}");
        Debug.Log($"  - isReviving: {isReviving}");
        Debug.Log($"  - hasRevived: {hasRevived}");
        Debug.Log($"  - isInvincible: {isInvincible}");
        Debug.Log($"  - currentState: {currentState}");
        Debug.Log($"  - currentTarget: {(currentTarget != null ? currentTarget.name : "null")}");
        Debug.Log($"  - potentialTargets count: {potentialTargets.Count}");
        Debug.Log($"  - Agent enabled: {agent.enabled}");
        Debug.Log($"  - Agent on NavMesh: {agent.isOnNavMesh}");
        Debug.Log($"  - Collider enabled: {(myCollider != null ? myCollider.enabled : false)}");
        Debug.Log($"  - Health: {(health != null ? $"{health.currentHealth}/{health.maxHealth}" : "null")}");
        Debug.Log($"  - Health.IsAlive(): {health?.IsAlive() ?? false}");
        Debug.Log($"  - Tag: {gameObject.tag}");
        Debug.Log($"  - Layer: {LayerMask.LayerToName(gameObject.layer)}");
        
        // 手动检测一次
        Debug.Log($"[SKELETON DEBUG] Manual detection test:");
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position + Vector3.up * detectionHeightOffset,
            detectionDistance,
            targetLayers
        );
        Debug.Log($"  - Found {nearbyColliders.Length} potential targets in range");
        foreach (var col in nearbyColliders)
        {
            Debug.Log($"    - {col.name} (Tag: {col.tag}, Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
        }
    }
    
    // 公开属性
    public bool IsInFakeDeathState => isDead && !hasRevived && canRevive;
    public bool IsReviving => isReviving;
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (isReviving)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 3f, 0.5f);
        }
        
        // 显示死亡位置
        if (isDead && !hasRevived && canRevive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + Vector3.up * 0.1f, Vector3.one * 0.3f);
        }
    }
    
// 修改 OnDestroy 方法，去掉 override 关键字
protected void OnDestroy()
{
    // 清理资源
    if (currentReviveEffect != null)
    {
        Destroy(currentReviveEffect);
    }
    
    // 如果基类将来添加了 OnDestroy，可以调用
    // base.OnDestroy();
}
}