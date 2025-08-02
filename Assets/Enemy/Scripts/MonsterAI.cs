using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Health))]
public class MonsterAI : MonoBehaviour
{
    public enum SlimeType 
    {
        Large,      // 大史莱姆
        Medium,     // 中史莱姆
        Small       // 小史莱姆
    }
    public SlimeType slimeType = SlimeType.Large;
    
    [Header("分裂设置")]
    public GameObject mediumSlimePrefab;
    public GameObject miniSlimePrefab;
    public float splitRadius = 2f;
    public float splitForce = 5f;

    [Header("巡逻设置")]
    public float patrolRange = 10f;
    public float minMoveTime = 2f;
    public float maxMoveTime = 4f;
    public float idleTime = 1.5f;
    [Range(0f, 1f)] public float idle02Chance = 0.5f;

    [Header("目标检测")]
    public float detectionAngle = 120f;
    public float detectionDistance = 15f;
    public float detectionHeightOffset = 1f;
    public float detectionInterval = 0.3f;
    public bool checkLineOfSight = true;
    public LayerMask obstacleLayers;
    public LayerMask targetLayers;              // 可攻击的目标层级（玩家和宠物）

    [Header("目标优先级")]
    public bool preferClosestTarget = true;      // 优先攻击最近的目标
    public float playerPriorityBonus = 0.8f;     // 玩家优先级加成（0.8表示相同距离下玩家优先）
    public float targetSwitchThreshold = 5f;     // 切换目标的距离阈值
    public float targetMemoryTime = 5f;          // 记住目标的时间

    [Header("战斗设置")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    public float attackRange = 3f;
    public float rangedAttackRange = 8f;
    [Range(0f, 1f)] public float chaseToRangedThreshold = 0.8f;
    [Range(1f, 2f)] public float rangedAbandonThreshold = 1.2f;
    public int maxRangedMisses = 3;
    public float attackCooldown = 2f;
    public float meleeAttackDelay = 0.5f;
    public float rangedAttackDelay = 0.3f;
    public float miniSlimeSpeedMultiplier = 2f;
    public float mediumSlimeSpeedMultiplier = 1.5f;
    public int meleeDamage = 10;                // 近战伤害
    public int rangedDamage = 15;               // 远程伤害

    [Header("投射物设置")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileArcHeight = 2f;
    public float projectileSpeed = 15f;

    [Header("特效设置")]
    public GameObject rangedAttackEffectPrefab;
    public Transform effectSpawnPoint;
    public float effectDuration = 1f;
    public AudioClip rangedAttackSound;
    public GameObject splitEffectPrefab;
    public AudioClip splitSound;

    [Header("爆炸设置")]
    public GameObject explosionPrefab;
    public float explosionDestroyDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionDamage = 20f;
    public float explosionForce = 10f;
    public LayerMask explosionDamageLayers;

    [Header("动画参数")]
    public string isMovingParam = "isMoving";
    public string idle02Trigger = "idle02";
    public string crawlTrigger = "crawl";
    public string attack02Trigger = "attack02";
    public string attack03Trigger = "attack03";
    public string deathTrigger = "death";
    public string hitTrigger = "hit";

    [Header("测试设置")]
    public bool enableTestMode = false;
    public KeyCode testAttackKey = KeyCode.F;
    public int testDamagePerHit = 50;
    public float testAttackRange = 10f;
    public GameObject testHitEffectPrefab;
    public AudioClip testHitSound;

    [Header("受击效果设置")]
    public float hitStunDuration = 1f;
    public int particleCount = 20;
    public float particleSize = 0.1f;
    public float particleSpeed = 3f;
    public float particleLifetime = 0.5f;
    public Material particleMaterial;

    // 私有变量
    private NavMeshAgent agent;
    private Animator animator;
    private Health health;
    
    // 目标管理
    private Transform currentTarget;
    private List<Transform> potentialTargets = new List<Transform>();
    private Dictionary<Transform, float> targetLastSeenTime = new Dictionary<Transform, float>();
    private Vector3 lastKnownTargetPosition;
    
    // 状态管理
    private enum State { Patrol, Chase, RangedAttack, MeleeAttack, Dead }
    private State currentState = State.Patrol;
    
    // 战斗变量
    private int rangedMissCount = 0;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    private float lastDetectionTime = 0f;
    private Coroutine currentAttackRoutine;
    private GameObject currentAttackEffect;
    private int testAttackCount = 0;
    private bool isStunned = false;
    private Coroutine stunCoroutine;

    void Start()
    {
        InitializeComponents();
        SetupSpeedMultiplier();
        StartCoroutine(PatrolRoutine());
        
        // 订阅受伤事件（注意是OnDamaged而不是OnDamage）
        health.OnDamaged += OnTakeDamage;
        
        if (enableTestMode)
        {
            Debug.Log($"[测试] {gameObject.name} 初始血量: {health.currentHealth}/{health.maxHealth}");
        }
    }

    void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        
        health.OnDeath += Die;
        
        // 设置为敌人标签
        gameObject.tag = "Enemy";
        
        // 初始化目标层级（如果未设置）
        if (targetLayers == 0)
        {
            targetLayers = LayerMask.GetMask("Player", "Pet");
        }
    }

    // 受伤回调
    void OnTakeDamage(int damage)
    {
        if (isDead || isStunned) return;
        
        Debug.Log($"[受伤] {gameObject.name} 受到 {damage} 点伤害");
        
        // 触发受击硬直
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(HitStunRoutine());
        
        // 生成白色方块粒子效果
        CreateHitParticles();
        
        // 可选：受击后短暂改变颜色
        StartCoroutine(FlashRed());
    }

    void SetupSpeedMultiplier()
    {
        float speedMultiplier = 1f;
        switch (slimeType)
        {
            case SlimeType.Large:
                speedMultiplier = 1f;
                break;
            case SlimeType.Medium:
                speedMultiplier = mediumSlimeSpeedMultiplier;
                break;
            case SlimeType.Small:
                speedMultiplier = miniSlimeSpeedMultiplier;
                break;
        }
        
        agent.speed = patrolSpeed * speedMultiplier;
        chaseSpeed *= speedMultiplier;
        patrolSpeed *= speedMultiplier;
    }

    void Update()
    {
        if (isDead) return;

        #if UNITY_EDITOR
        if (enableTestMode && Input.GetKeyDown(testAttackKey))
        {
            TestAttack();
        }
        #endif

        if (isStunned) return;

        // 定期检测目标
        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            DetectTargets();
            lastDetectionTime = Time.time;
        }

        // 清理过期的目标记忆
        CleanupTargetMemory();

        animator.SetBool(isMovingParam, agent.velocity.magnitude > 0.1f);

        switch (currentState)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chase: ChaseUpdate(); break;
            case State.RangedAttack: RangedAttackUpdate(); break;
            case State.MeleeAttack: MeleeAttackUpdate(); break;
        }
    }

    #region 目标检测和管理
    void DetectTargets()
    {
        potentialTargets.Clear();
        
        // 获取检测范围内的所有潜在目标
        Collider[] colliders = Physics.OverlapSphere(
            transform.position + Vector3.up * detectionHeightOffset,
            detectionDistance,
            targetLayers
        );

        foreach (Collider col in colliders)
        {
            if (IsValidTarget(col.transform))
            {
                Vector3 directionToTarget = col.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                
                // 角度检查
                if (angle <= detectionAngle / 2f)
                {
                    // 视线检查
                    if (!checkLineOfSight || !Physics.Raycast(
                        transform.position + Vector3.up * detectionHeightOffset,
                        directionToTarget.normalized,
                        directionToTarget.magnitude,
                        obstacleLayers))
                    {
                        potentialTargets.Add(col.transform);
                        targetLastSeenTime[col.transform] = Time.time;
                    }
                }
            }
        }

        // 选择最佳目标
        EvaluateAndSelectTarget();
    }

    bool IsValidTarget(Transform target)
    {
        if (target == null || target == transform) return false;
        
        // 检查Health组件
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.currentHealth <= 0) return false;
        
        // 检查标签
        if (target.CompareTag("Player") || target.CompareTag("Pet"))
        {
            return true;
        }
        
        return false;
    }

    void EvaluateAndSelectTarget()
    {
        if (potentialTargets.Count == 0)
        {
            // 如果没有新目标但记得旧目标位置，继续追击一段时间
            if (currentTarget != null && targetLastSeenTime.ContainsKey(currentTarget))
            {
                if (Time.time - targetLastSeenTime[currentTarget] > targetMemoryTime)
                {
                    currentTarget = null;
                    if (currentState != State.Patrol)
                    {
                        ReturnToPatrol();
                    }
                }
            }
            return;
        }

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (Transform target in potentialTargets)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            float score = distance;
            
            // 玩家优先级加成
            if (target.CompareTag("Player"))
            {
                score *= playerPriorityBonus;
            }
            
            // 如果已有目标，增加切换阈值以避免频繁切换
            if (target == currentTarget)
            {
                score *= 0.8f; // 当前目标有20%的"粘性"
            }
            
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        // 只有在新目标明显更好时才切换
        if (bestTarget != null)
        {
            if (currentTarget == null || bestTarget == currentTarget ||
                bestScore < Vector3.Distance(transform.position, currentTarget.position) - targetSwitchThreshold)
            {
                currentTarget = bestTarget;
                lastKnownTargetPosition = currentTarget.position;
                
                if (currentState == State.Patrol)
                {
                    StopAllCoroutines();
                    currentState = State.Chase;
                    agent.speed = chaseSpeed;
                }
            }
        }
    }

    void CleanupTargetMemory()
    {
        List<Transform> toRemove = new List<Transform>();
        
        foreach (var kvp in targetLastSeenTime)
        {
            if (kvp.Key == null || Time.time - kvp.Value > targetMemoryTime)
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (Transform t in toRemove)
        {
            targetLastSeenTime.Remove(t);
        }
    }
    #endregion

    #region 战斗逻辑修改
    void ChaseUpdate()
    {
        if (currentTarget == null) 
        {
            ReturnToPatrol();
            return;
        }

        // 检查目标是否仍然有效
        if (!IsValidTarget(currentTarget))
        {
            currentTarget = null;
            EvaluateAndSelectTarget();
            return;
        }

        lastKnownTargetPosition = currentTarget.position;
        float distance = Vector3.Distance(transform.position, lastKnownTargetPosition);

        if (currentAttackRoutine != null) return;

        if (distance <= attackRange)
        {
            currentState = State.MeleeAttack;
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
        }
        else if (slimeType == SlimeType.Large && distance <= rangedAttackRange)
        {
            currentState = State.RangedAttack;
            currentAttackRoutine = StartCoroutine(RangedAttackRoutine());
        }
        else
        {
            agent.SetDestination(lastKnownTargetPosition);
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        while (currentState == State.MeleeAttack && currentTarget != null && IsValidTarget(currentTarget))
        {
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance > attackRange * 1.2f)
            {
                if (slimeType == SlimeType.Large && currentDistance <= rangedAttackRange)
                {
                    currentState = State.RangedAttack;
                    currentAttackRoutine = StartCoroutine(RangedAttackRoutine());
                    yield break;
                }
                else
                {
                    break;
                }
            }

            if (Time.time - lastAttackTime < attackCooldown)
            {
                yield return null;
                continue;
            }

            agent.isStopped = true;
            
            Vector3 lookPos = new Vector3(
                currentTarget.position.x, 
                transform.position.y, 
                currentTarget.position.z
            );
            transform.LookAt(lookPos);

            if (Random.value < 0.5f) 
                animator.SetTrigger(attack02Trigger);
            else 
                animator.SetTrigger(attack03Trigger);

            yield return new WaitForSeconds(meleeAttackDelay);

            currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance <= attackRange * 1.2f && IsValidTarget(currentTarget))
            {
                Health targetHealth = currentTarget.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(meleeDamage);
                }
                
                // 如果是宠物，可能触发特殊效果
                SimplePetNavMesh pet = currentTarget.GetComponent<SimplePetNavMesh>();
                if (pet != null)
                {
                    Debug.Log($"{gameObject.name} 攻击了宠物 {currentTarget.name}，造成 {meleeDamage} 点伤害");
                }
            }

            lastAttackTime = Time.time;
            yield return new WaitForSeconds(0.5f);
        }

        agent.isStopped = false;
        currentState = State.Chase;
        currentAttackRoutine = null;
    }

    IEnumerator RangedAttackRoutine()
    {
        if (slimeType != SlimeType.Large) yield break;

        while (currentState == State.RangedAttack && currentTarget != null && IsValidTarget(currentTarget))
        {
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            
            if (currentDistance <= attackRange)
            {
                currentState = State.MeleeAttack;
                currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
                yield break;
            }
            
            if (currentDistance > rangedAttackRange * rangedAbandonThreshold)
            {
                rangedMissCount++;
                if (rangedMissCount >= maxRangedMisses)
                {
                    agent.isStopped = false;
                    currentState = State.Chase;
                    yield break;
                }
            }

            if (Time.time - lastAttackTime < attackCooldown)
            {
                yield return null;
                continue;
            }

            agent.isStopped = true;
            
            Vector3 lookPos = new Vector3(
                currentTarget.position.x, 
                transform.position.y, 
                currentTarget.position.z
            );
            transform.LookAt(lookPos);

            animator.SetTrigger(crawlTrigger);
            
            GameObject attackEffect = null;
            if (rangedAttackEffectPrefab != null && effectSpawnPoint != null)
            {
                attackEffect = Instantiate(
                    rangedAttackEffectPrefab,
                    effectSpawnPoint.position,
                    effectSpawnPoint.rotation,
                    effectSpawnPoint
                );
                Destroy(attackEffect, 2f);
            }

            if (rangedAttackSound != null)
            {
                AudioSource.PlayClipAtPoint(rangedAttackSound, transform.position);
            }

            yield return new WaitForSeconds(rangedAttackDelay);

            currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance > rangedAttackRange * rangedAbandonThreshold)
            {
                agent.isStopped = false;
                currentState = State.Chase;
                yield break;
            }

            // 发射投射物
            if (projectilePrefab && projectileSpawnPoint && IsValidTarget(currentTarget))
            {
                GameObject projectile = Instantiate(
                    projectilePrefab,
                    projectileSpawnPoint.position,
                    Quaternion.identity
                );
                
                Projectile proj = projectile.GetComponent<Projectile>();
                if (proj == null) proj = projectile.AddComponent<Projectile>();
                
                proj.speed = projectileSpeed;
                proj.arcHeight = projectileArcHeight;
                proj.damage = rangedDamage;
                proj.SetTarget(currentTarget);
            }

            lastAttackTime = Time.time;
            yield return new WaitForSeconds(0.7f);
        }

        agent.isStopped = false;
        currentState = State.Chase;
        rangedMissCount = 0;
        currentAttackRoutine = null;
    }
    #endregion

    #region 公共接口
    // 获取当前目标（供宠物AI查询）
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    // 强制设置目标（用于特殊情况）
    public void ForceTarget(Transform newTarget)
    {
        if (IsValidTarget(newTarget))
        {
            currentTarget = newTarget;
            lastKnownTargetPosition = newTarget.position;
            targetLastSeenTime[newTarget] = Time.time;
            
            if (currentState == State.Patrol)
            {
                StopAllCoroutines();
                currentState = State.Chase;
                agent.speed = chaseSpeed;
            }
        }
    }

    // 增加仇恨值（可选的扩展功能）
    public void AddThreat(Transform source, float amount)
    {
        // 这里可以实现更复杂的仇恨系统
        if (IsValidTarget(source) && currentTarget == null)
        {
            ForceTarget(source);
        }
    }
    #endregion

    #region 其他原有功能保持不变
    // PatrolRoutine, PatrolUpdate, ReturnToPatrol 等保持原样
    IEnumerator PatrolRoutine()
    {
        while (currentState == State.Patrol)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint(transform.position, patrolRange);
            if (randomPoint != Vector3.zero)
            {
                agent.SetDestination(randomPoint);
                
                float moveTime = Random.Range(minMoveTime, maxMoveTime);
                float elapsedTime = 0f;
                
                while (elapsedTime < moveTime && agent.remainingDistance > agent.stoppingDistance)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            agent.isStopped = true;
            
            if (Random.value < idle02Chance) animator.SetTrigger(idle02Trigger);

            yield return new WaitForSeconds(idleTime);
            agent.isStopped = false;
        }
    }

    void PatrolUpdate()
    {
        // 巡逻时额外逻辑
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero;
    }

    void ReturnToPatrol()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            CleanupAttackEffect();
            currentAttackRoutine = null;
        }
        
        currentState = State.Patrol;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        currentTarget = null;
        StartCoroutine(PatrolRoutine());
    }

    void RangedAttackUpdate()
    {
        // 远程攻击时的额外逻辑
    }

    void MeleeAttackUpdate()
    {
        // 近战攻击时的额外逻辑
    }

    void CleanupAttackEffect()
    {
        if (currentAttackEffect != null)
        {
            Destroy(currentAttackEffect);
            currentAttackEffect = null;
        }
    }

    #region 测试攻击系统
    void TestAttack()
    {
        if (isDead) 
        {
            Debug.Log($"[测试] {gameObject.name} 已死亡，无法攻击");
            return;
        }
        
        if (isStunned)
        {
            Debug.Log($"[测试] {gameObject.name} 正在硬直中，刷新硬直时间");
        }
        
        testAttackCount++;
        Debug.Log($"[测试] 第 {testAttackCount} 次攻击 {gameObject.name} - 当前状态: {currentState}");
        
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(HitStunRoutine());
        
        CreateHitParticles();
        
        if (testHitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(
                testHitEffectPrefab,
                transform.position + Vector3.up * 1f,
                Quaternion.identity
            );
            Destroy(hitEffect, 2f);
        }
        
        if (testHitSound != null)
        {
            AudioSource.PlayClipAtPoint(testHitSound, transform.position);
        }
        
        int healthBefore = health.currentHealth;
        health.TakeDamage(testDamagePerHit);
        int healthAfter = health.currentHealth;
        
        Debug.Log($"[测试] {gameObject.name} 受到 {testDamagePerHit} 点伤害: {healthBefore} → {healthAfter} HP");
        
        if (healthAfter <= 0)
        {
            Debug.Log($"[测试] {gameObject.name} 血量耗尽，即将分裂！");
        }
        
        StartCoroutine(FlashRed());
    }
    
    IEnumerator HitStunRoutine()
    {
        isStunned = true;
        
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            CleanupAttackEffect();
            currentAttackRoutine = null;
        }
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        if (!string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);
        }
        
        yield return new WaitForSeconds(hitStunDuration);
        
        isStunned = false;
        agent.isStopped = false;
        
        if (currentState == State.Chase && currentTarget != null)
        {
            agent.SetDestination(lastKnownTargetPosition);
        }
        
        stunCoroutine = null;
    }
    
    void CreateHitParticles()
    {
        Vector3 hitPoint = transform.position + Vector3.up * 1f;
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = hitPoint;
            particle.transform.localScale = Vector3.one * particleSize;
            
            Destroy(particle.GetComponent<Collider>());
            
            Renderer renderer = particle.GetComponent<Renderer>();
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Unlit/Color"));
                renderer.material.color = Color.white;
            }
            
            Rigidbody rb = particle.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 0.1f;
            
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1f, 1f)
            ).normalized;
            
            rb.linearVelocity = randomDirection * particleSpeed * Random.Range(0.8f, 1.2f);
            rb.angularVelocity = Random.insideUnitSphere * 10f;
            
            Destroy(particle, particleLifetime);
        }
    }
    
    IEnumerator FlashRed()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }
    #endregion

    #region 死亡和分裂
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[死亡] {gameObject.name} 开始死亡流程");

        StopAllCoroutines();
        CleanupAttackEffect();
        currentState = State.Dead;
        agent.isStopped = true;
        animator.SetTrigger(deathTrigger);

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        agent.enabled = false;

        bool shouldSplit = false;
        switch (slimeType)
        {
            case SlimeType.Large:
                shouldSplit = mediumSlimePrefab != null;
                Debug.Log($"[死亡] 大史莱姆，准备分裂成中史莱姆: {shouldSplit}");
                break;
            case SlimeType.Medium:
                shouldSplit = miniSlimePrefab != null;
                Debug.Log($"[死亡] 中史莱姆，准备分裂成小史莱姆: {shouldSplit}");
                break;
            case SlimeType.Small:
                shouldSplit = false;
                Debug.Log($"[死亡] 小史莱姆，不再分裂");
                break;
        }

        if (shouldSplit)
        {
            PerformSplit();
        }

        // 快速销毁，0.1秒
        enabled = false;
        Destroy(gameObject, 0.1f);
    }

    void PerformSplit()
    {
        Debug.Log($"[分裂] {gameObject.name} 开始分裂");
        
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab, 
                transform.position, 
                Quaternion.identity
            );
            
            Destroy(explosion, explosionDestroyDelay);
            
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(explosion, totalDuration);
            }
            
            ApplyExplosionEffects();
        }

        if (splitEffectPrefab != null)
        {
            GameObject effect = Instantiate(splitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (splitSound != null)
        {
            AudioSource.PlayClipAtPoint(splitSound, transform.position);
        }

        GameObject slimePrefabToSpawn = null;
        int spawnCount = 0;
        SlimeType childSlimeType = SlimeType.Small;

        switch (slimeType)
        {
            case SlimeType.Large:
                slimePrefabToSpawn = mediumSlimePrefab;
                spawnCount = 2;
                childSlimeType = SlimeType.Medium;
                break;
            case SlimeType.Medium:
                slimePrefabToSpawn = miniSlimePrefab;
                spawnCount = 4;
                childSlimeType = SlimeType.Small;
                break;
        }

        Debug.Log($"[分裂] 准备生成 {spawnCount} 个 {childSlimeType} 史莱姆");

        if (slimePrefabToSpawn != null)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                float angle = (360f / spawnCount) * i;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * splitRadius;
                Vector3 spawnPosition = transform.position + offset;

                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, splitRadius * 2, NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                }

                GameObject childSlime = Instantiate(slimePrefabToSpawn, spawnPosition, Quaternion.identity);
                Debug.Log($"[分裂] 生成了第 {i+1} 个子史莱姆: {childSlime.name}");
                
                Rigidbody rb = childSlime.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 pushDirection = (spawnPosition - transform.position).normalized;
                    pushDirection.y = 0.5f;
                    rb.AddForce(pushDirection * splitForce, ForceMode.Impulse);
                }

                MonsterAI childAI = childSlime.GetComponent<MonsterAI>();
                if (childAI != null)
                {
                    childAI.slimeType = childSlimeType;
                    
                    // 继承父史莱姆的目标（安全检查）
                    if (currentTarget != null && IsValidTarget(currentTarget))
                    {
                        // 使用延迟调用，确保子史莱姆初始化完成
                        StartCoroutine(DelayedTargetAssignment(childAI, currentTarget));
                    }
                }
            }
        }
    }

    void ApplyExplosionEffects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionDamageLayers);
        
        foreach (Collider hit in colliders)
        {
            Health targetHealth = hit.GetComponent<Health>();
            if (targetHealth != null && hit.gameObject != gameObject)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                float actualDamage = explosionDamage * damageMultiplier;
                
                targetHealth.TakeDamage((int)actualDamage);
            }
            
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                direction.y = 0.5f;
                
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float forceMultiplier = 1f - (distance / explosionRadius);
                float actualForce = explosionForce * forceMultiplier;
                
                rb.AddForce(direction * actualForce, ForceMode.Impulse);
            }
        }
    }
    #endregion

    // 延迟设置目标（确保子史莱姆初始化完成）
    IEnumerator DelayedTargetAssignment(MonsterAI childAI, Transform target)
    {
        yield return null;  // 等待一帧
        if (childAI != null && target != null)
        {
            childAI.ForceTarget(target);
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件，避免内存泄漏
        if (health != null)
        {
            health.OnDamaged -= OnTakeDamage;
            health.OnDeath -= Die;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测扇形
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Vector3 center = transform.position + Vector3.up * detectionHeightOffset;
        Vector3 forward = transform.forward * detectionDistance;
        
        int segments = 20;
        float deltaAngle = detectionAngle / segments;
        Vector3 prevPoint = center + Quaternion.Euler(0, -detectionAngle/2, 0) * forward;
        
        for (int i = 0; i <= segments; i++)
        {
            Vector3 point = center + Quaternion.Euler(0, -detectionAngle/2 + deltaAngle * i, 0) * forward;
            Gizmos.DrawLine(center, point);
            if (i > 0) Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // 绘制检测距离
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, detectionDistance);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRange);
        
        // 绘制远程攻击范围（只有大史莱姆显示）
        if (slimeType == SlimeType.Large)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center, rangedAttackRange);
        }
        
        // 绘制分裂范围（小史莱姆不显示）
        if (slimeType != SlimeType.Small)
        {
            GameObject prefabToCheck = (slimeType == SlimeType.Large) ? mediumSlimePrefab : miniSlimePrefab;
            if (prefabToCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, splitRadius);
            }
            
            // 绘制爆炸范围
            if (explosionPrefab != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }
        }
        
        // 绘制测试攻击范围（测试模式下）
        if (enableTestMode)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center, testAttackRange);
        }
        
        // 绘制当前目标连线
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(center, currentTarget.position + Vector3.up * detectionHeightOffset);
        }
    }
    #endregion
}