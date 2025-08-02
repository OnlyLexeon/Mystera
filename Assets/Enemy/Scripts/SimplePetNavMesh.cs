using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class SimplePetNavMesh : MonoBehaviour
{
    [Header("基本设置")]
    public Transform player;
    public float followDistance = 3f;
    public float teleportDistance = 20f;
    
    [Header("战斗设置")]
    public float detectionRadius = 10f;          // 敌人检测半径
    public float attackRange = 2f;               // 攻击距离
    public float attackDamage = 10f;             // 攻击伤害
    public float attackCooldown = 1.5f;          // 攻击冷却
    public LayerMask enemyLayer;                 // 敌人层级
    public float assistRange = 15f;              // 协助玩家的最大距离
    
    [Header("目标优先级")]
    public bool prioritizePlayerTarget = true;   // 优先攻击玩家的目标
    public float targetSwitchDelay = 2f;         // 切换目标的延迟
    
    [Header("动画设置")]
    public string animationParam = "animation";
    public int idleValue = 1;
    public int moveValue = 15;
    public int attackValue = 20;                 // 攻击动画值
    
    [Header("生命值设置")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvulnerable = false;          // 是否无敌
    
    [Header("特效设置")]
    public GameObject attackEffectPrefab;        // 攻击特效
    public Transform attackPoint;                // 攻击点位置
    public AudioClip attackSound;                // 攻击音效
    public GameObject deathEffectPrefab;         // 死亡特效
    
    [Header("调试")]
    public bool showDebugInfo = false;
    
    // 私有变量
    private NavMeshAgent agent;
    private Animator animator;
    private Health healthComponent;
    
    // 状态管理
    private enum PetState { Idle, Following, Combat, Dead }
    private PetState currentState = PetState.Idle;
    
    // 战斗相关
    private Transform currentTarget;
    private float lastAttackTime;
    private float lastTargetCheckTime;
    private List<Transform> nearbyEnemies = new List<Transform>();
    private float targetLockTime;
    
    // 动画状态
    private int currentAnimState = 1;
    
    void Start()
    {
        InitializeComponents();
        currentHealth = maxHealth;
    }
    
    void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // 尝试获取Health组件，如果没有就添加一个
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = gameObject.AddComponent<Health>();
            healthComponent.maxHealth = (int)maxHealth;
            healthComponent.currentHealth = (int)currentHealth;
        }
        
        // 订阅死亡事件
        healthComponent.OnDeath += OnPetDeath;
        
        // 自动查找玩家
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // 设置初始动画
        SetAnimation(idleValue);
        
        // 设置为友方单位（通过标签）
        gameObject.tag = "Pet";
        
        // 确保在正确的层级
        int petLayer = LayerMask.NameToLayer("Pet");
        if (petLayer != -1)
        {
            gameObject.layer = petLayer;
        }
        
        Debug.Log($"[宠物初始化] 初始化完成 - 玩家: {player?.name}");
    }
    
    void Update()
    {
        if (currentState == PetState.Dead || player == null) return;
        
        // 更新健康值
        currentHealth = healthComponent.currentHealth;
        
        // 定期检测敌人
        if (Time.time - lastTargetCheckTime > 0.5f)
        {
            DetectEnemies();
            lastTargetCheckTime = Time.time;
        }
        
        // 状态机更新
        switch (currentState)
        {
            case PetState.Idle:
                IdleUpdate();
                break;
            case PetState.Following:
                FollowingUpdate();
                break;
            case PetState.Combat:
                CombatUpdate();
                break;
        }
        
        // 检查移动动画
        UpdateMovementAnimation();
        
        // 调试信息
        if (showDebugInfo)
        {
            Debug.Log($"[宠物状态] 当前状态: {currentState}, 目标: {currentTarget?.name}, 附近敌人: {nearbyEnemies.Count}");
        }
    }
    
    void IdleUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > followDistance)
        {
            currentState = PetState.Following;
        }
        else if (currentTarget != null)
        {
            currentState = PetState.Combat;
        }
    }
    
    void FollowingUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 传送检查
        if (distanceToPlayer > teleportDistance)
        {
            Teleport();
            return;
        }
        
        // 如果发现敌人且在协助范围内，进入战斗
        if (currentTarget != null && distanceToPlayer < assistRange)
        {
            currentState = PetState.Combat;
            return;
        }
        
        // 跟随玩家
        if (distanceToPlayer > followDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
            currentState = PetState.Idle;
        }
    }
    
    void CombatUpdate()
    {
        // 检查目标是否有效
        if (!IsValidTarget(currentTarget))
        {
            currentTarget = null;
            FindNewTarget();
            
            if (currentTarget == null)
            {
                currentState = PetState.Following;
                return;
            }
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 如果离玩家太远，放弃战斗
        if (distanceToPlayer > assistRange)
        {
            currentTarget = null;
            currentState = PetState.Following;
            return;
        }
        
        // 战斗逻辑
        if (distanceToTarget <= attackRange)
        {
            // 在攻击范围内，停止移动并攻击
            agent.ResetPath();
            
            // 面向目标
            Vector3 lookDirection = currentTarget.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // 执行攻击
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
            }
        }
        else
        {
            // 追击目标
            agent.SetDestination(currentTarget.position);
        }
    }
    
    void DetectEnemies()
    {
        nearbyEnemies.Clear();
        
        // 通过标签查找所有敌人
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (showDebugInfo)
        {
            Debug.Log($"[宠物检测] 场景中找到 {enemyObjects.Length} 个带Enemy标签的对象");
        }
        
        foreach (GameObject enemyObj in enemyObjects)
        {
            if (enemyObj == null || enemyObj == gameObject) continue;
            
            float distance = Vector3.Distance(transform.position, enemyObj.transform.position);
            
            // 检查是否在检测范围内
            if (distance <= detectionRadius)
            {
                Transform enemyTransform = enemyObj.transform;
                
                if (IsValidTarget(enemyTransform))
                {
                    nearbyEnemies.Add(enemyTransform);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[宠物检测] 发现有效敌人: {enemyTransform.name}, 距离: {distance:F1}");
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[宠物检测] 检测范围内有效敌人数量: {nearbyEnemies.Count}");
        }
        
        // 如果没有当前目标，选择一个新目标
        if (currentTarget == null && nearbyEnemies.Count > 0)
        {
            FindNewTarget();
        }
    }
    
    void FindNewTarget()
    {
        // 清理无效的敌人引用
        nearbyEnemies.RemoveAll(enemy => enemy == null);
        
        if (nearbyEnemies.Count == 0) return;
        
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        
        // 如果优先攻击玩家的目标
        if (prioritizePlayerTarget && player != null)
        {
            // 检查附近的敌人是否正在攻击玩家
            foreach (Transform enemyTransform in nearbyEnemies)
            {
                if (enemyTransform == null) continue;
                
                // 获取Enemy组件（支持所有继承自Enemy的类）
                Enemy enemy = enemyTransform.GetComponent<Enemy>();
                if (enemy != null && enemy.GetCurrentTarget() == player)
                {
                    bestTarget = enemyTransform;
                    if (showDebugInfo)
                    {
                        Debug.Log($"[宠物目标] 找到正在攻击玩家的敌人: {enemyTransform.name}");
                    }
                    break;
                }
            }
        }
        
        // 如果没有找到玩家的目标，选择最近的敌人
        if (bestTarget == null)
        {
            foreach (Transform enemy in nearbyEnemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.position);
                if (distance < bestScore)
                {
                    bestScore = distance;
                    bestTarget = enemy;
                }
            }
            
            if (showDebugInfo && bestTarget != null)
            {
                Debug.Log($"[宠物目标] 选择最近的敌人: {bestTarget.name}, 距离: {bestScore:F1}");
            }
        }
        
        currentTarget = bestTarget;
        targetLockTime = Time.time;
    }
    
    bool IsValidTarget(Transform target)
    {
        if (target == null) return false;
        
        // 检查是否有Health组件且存活
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.currentHealth <= 0) 
        {
            if (showDebugInfo)
            {
                Debug.Log($"[宠物检测] {target.name} 无效 - Health组件: {targetHealth != null}, 血量: {targetHealth?.currentHealth}");
            }
            return false;
        }
        
        // 只需要检查标签
        bool hasEnemyTag = target.CompareTag("Enemy");
        
        if (showDebugInfo)
        {
            Debug.Log($"[宠物检测] {target.name} - 标签: {target.tag}, 是敌人: {hasEnemyTag}");
        }
        
        return hasEnemyTag;
    }
    
    void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // 播放攻击动画
        SetAnimation(attackValue);
        
        // 生成攻击特效
        if (attackEffectPrefab != null && attackPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f);
        }
        
        // 播放攻击音效
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
        
        // 造成伤害
        if (currentTarget != null)
        {
            Health targetHealth = currentTarget.GetComponent<Health>();
            if (targetHealth != null)
            {
                // 修改：使用带攻击者参数的TakeDamage方法
                targetHealth.TakeDamage((int)attackDamage, gameObject);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[宠物攻击] {gameObject.name} 攻击了 {currentTarget.name}，造成 {attackDamage} 点伤害");
                }
                
                // 如果目标死亡，立即寻找新目标
                if (targetHealth.currentHealth <= 0)
                {
                    currentTarget = null;
                    FindNewTarget();
                }
            }
        }
        
        // 0.5秒后恢复到移动或待机动画
        Invoke(nameof(ResetAttackAnimation), 0.5f);
    }
    
    void ResetAttackAnimation()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            SetAnimation(moveValue);
        }
        else
        {
            SetAnimation(idleValue);
        }
    }
    
    void UpdateMovementAnimation()
    {
        // 只在非攻击状态下更新移动动画
        if (currentAnimState != attackValue)
        {
            if (agent.velocity.magnitude > 0.1f && currentAnimState != moveValue)
            {
                SetAnimation(moveValue);
            }
            else if (agent.velocity.magnitude < 0.1f && currentState == PetState.Idle && currentAnimState != idleValue)
            {
                SetAnimation(idleValue);
            }
        }
    }
    
    void SetAnimation(int value)
    {
        if (animator != null && currentAnimState != value)
        {
            animator.SetInteger(animationParam, value);
            currentAnimState = value;
        }
    }
    
    void Teleport()
    {
        Vector3 behindPlayer = player.position - player.forward * 2f;
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(behindPlayer, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            transform.LookAt(player);
            SetAnimation(idleValue);
            currentState = PetState.Idle;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isInvulnerable || currentState == PetState.Dead) return;
        
        healthComponent.TakeDamage((int)damage);
    }
    
    void OnPetDeath()
    {
        currentState = PetState.Dead;
        
        // 停止所有行为
        agent.isStopped = true;
        agent.enabled = false;
        
        // 播放死亡特效
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // 禁用碰撞
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // 可以选择延迟销毁或复活逻辑
        Invoke(nameof(RespawnPet), 5f);
    }
    
    void RespawnPet()
    {
        // 复活逻辑
        if (player != null)
        {
            // 传送到玩家身边
            transform.position = player.position - player.forward * 2f;
            
            // 重置状态
            currentHealth = maxHealth;
            healthComponent.currentHealth = (int)maxHealth;
            currentState = PetState.Idle;
            currentTarget = null;
            
            // 重新启用组件
            agent.enabled = true;
            agent.isStopped = false;
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            SetAnimation(idleValue);
        }
    }
    
    // 公共方法：获取当前目标（供其他系统使用）
    public Transform GetCurrentTarget() => currentTarget;
    
    // 公共方法：设置跟随目标
    public void SetFollowTarget(Transform newPlayer)
    {
        player = newPlayer;
    }
    
    // 公共方法：命令攻击特定目标
    public void CommandAttack(Transform target)
    {
        if (IsValidTarget(target))
        {
            currentTarget = target;
            currentState = PetState.Combat;
        }
    }
    
    // 添加调试方法
    [ContextMenu("Debug - Find All Enemies")]
    void DebugFindAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"[调试] 场景中找到 {enemies.Length} 个Enemy标签的对象:");
        
        foreach (var enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            
            Debug.Log($"- {enemy.name}:");
            Debug.Log($"  位置: {enemy.transform.position}");
            Debug.Log($"  Health组件: {health != null} (血量: {health?.currentHealth}/{health?.maxHealth})");
            Debug.Log($"  Enemy组件: {enemyComponent != null} (类型: {enemyComponent?.GetType().Name})");
            Debug.Log($"  距离: {Vector3.Distance(transform.position, enemy.transform.position):F1}米");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        // 检测范围（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // 攻击范围（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 协助范围（蓝色）
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, assistRange);
        }
        
        // 跟随距离（黄色）
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, followDistance);
            
            // 传送距离（红色）
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, teleportDistance);
        }
        
        // 当前目标连线（品红色）
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position + Vector3.up, currentTarget.position + Vector3.up);
        }
        
        // 显示附近的敌人
        Gizmos.color = Color.red * 0.5f;
        foreach (Transform enemy in nearbyEnemies)
        {
            if (enemy != null && enemy != currentTarget)
            {
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, enemy.position + Vector3.up * 0.5f);
            }
        }
    }
}