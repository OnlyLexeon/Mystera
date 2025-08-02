using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Health))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRange = 10f;
    public float minMoveTime = 2f;
    public float maxMoveTime = 4f;
    public float idleTime = 1.5f;
    [Range(0f, 1f)] public float idle02Chance = 0.5f;

    [Header("Target Detection")]
    public float detectionAngle = 120f;
    public float detectionDistance = 15f;
    public float detectionHeightOffset = 1f;
    public float detectionInterval = 0.1f;  // 改为0.1秒，提高响应速度
    public bool checkLineOfSight = true;
    public LayerMask obstacleLayers;
    public LayerMask targetLayers;

    [Header("Target Priority")]
    public bool preferClosestTarget = true;
    public float playerPriorityBonus = 0.8f;
    public float targetSwitchThreshold = 5f;
    public float targetMemoryTime = 5f;
    
    [Header("Alert System")]
    public float alertRadius = 10f;
    public float alertDuration = 10f;
    public bool showAlertEffect = true;
    public GameObject alertEffectPrefab;
    public AudioClip alertSound;
    public float hatePriority = 0.5f;

    [Header("Combat Settings")]
    public float chaseSpeed = 5f;  // 提高追击速度
    public float patrolSpeed = 2f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float meleeAttackDelay = 0.3f;
    public int meleeDamage = 10;
    public float stoppingDistance = 1.2f;  // 统一停止距离

    [Header("Animation Parameters")]
    public string isMovingParam = "isMoving";
    public string idle02Trigger = "idle02";
    public string attack02Trigger = "attack02";
    public string attack03Trigger = "attack03";
    public string deathTrigger = "death";
    public string hitTrigger = "hit";

    [Header("Hit Effect Settings")]
    public float hitStunDuration = 1f;
    public int particleCount = 20;
    public float particleSize = 0.1f;
    public float particleSpeed = 3f;
    public float particleLifetime = 0.5f;
    public Material particleMaterial;
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = false;

    // Protected variables accessible by child classes
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Health health;
    
    // Target management
    protected Transform currentTarget;
    protected List<Transform> potentialTargets = new List<Transform>();
    protected Dictionary<Transform, float> targetLastSeenTime = new Dictionary<Transform, float>();
    protected Vector3 lastKnownTargetPosition;
    
    // Hate system
    protected Transform hateTarget;
    protected float hateEndTime;
    protected bool isAlerted = false;
    
    // State management
    protected enum State { Patrol, Chase, RangedAttack, MeleeAttack, Dead }
    protected State currentState = State.Patrol;
    
    // Combat variables
    protected bool isDead = false;
    protected float lastAttackTime = 0f;
    protected float lastDetectionTime = 0f;
    protected Coroutine currentAttackRoutine;
    protected bool isStunned = false;
    protected Coroutine stunCoroutine;

    protected virtual void Start()
    {
        InitializeComponents();
        SetupInitialState();
        
        // 立即进行一次检测，避免初始延迟
        lastDetectionTime = -detectionInterval;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[INIT] {gameObject.name} initialized - Health: {health.currentHealth}/{health.maxHealth}");
        }
    }

    protected virtual void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        
        if (health != null)
        {
            health.OnDeath += Die;
            health.OnDamagedByAttacker += OnTakeDamageWithAttacker;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[INIT] {gameObject.name} subscribed to Health events");
            }
        }
        
        gameObject.tag = "Enemy";
        
        if (targetLayers == 0)
        {
            targetLayers = LayerMask.GetMask("Player", "Pet");
        }
    }

    protected virtual void SetupInitialState()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = stoppingDistance;  // 设置统一的停止距离
        StartCoroutine(PatrolRoutine());
        
        // 立即进行一次目标检测
        DetectTargets();
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if (isStunned) return;

        // Periodic target detection
        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            DetectTargets();
            lastDetectionTime = Time.time;
        }

        // Cleanup expired target memory
        CleanupTargetMemory();

        // Update animation
        animator.SetBool(isMovingParam, agent.velocity.magnitude > 0.1f);

        // State machine update
        UpdateStateMachine();
    }

    protected virtual void UpdateStateMachine()
    {
        switch (currentState)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chase: ChaseUpdate(); break;
            case State.RangedAttack: RangedAttackUpdate(); break;
            case State.MeleeAttack: MeleeAttackUpdate(); break;
        }
    }

    #region Target Detection and Management
    protected virtual void DetectTargets()
    {
        potentialTargets.Clear();
        
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
                
                if (angle <= detectionAngle / 2f)
                {
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

        EvaluateAndSelectTarget();
    }

    protected virtual bool IsValidTarget(Transform target)
    {
        if (target == null || target == transform) return false;
        
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.currentHealth <= 0) return false;
        
        if (target.CompareTag("Player") || target.CompareTag("Pet"))
        {
            return true;
        }
        
        return false;
    }

    protected virtual void EvaluateAndSelectTarget()
    {
        // Check hate target first
        if (hateTarget != null && Time.time < hateEndTime)
        {
            if (IsValidTarget(hateTarget))
            {
                currentTarget = hateTarget;
                lastKnownTargetPosition = hateTarget.position;
                return;
            }
            else
            {
                hateTarget = null;
                isAlerted = false;
            }
        }
        
        if (potentialTargets.Count == 0)
        {
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
            
            if (target == hateTarget && Time.time < hateEndTime)
            {
                score *= hatePriority;
            }
            
            if (target.CompareTag("Player"))
            {
                score *= playerPriorityBonus;
            }
            
            if (target == currentTarget)
            {
                score *= 0.8f;
            }
            
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

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
                    agent.isStopped = false;
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[DETECT] {gameObject.name} detected {currentTarget.name}, switching to Chase");
                    }
                }
            }
        }
    }

    protected void CleanupTargetMemory()
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
    
    // 新增：获取实际的边缘到边缘距离
    protected virtual float GetActualDistance(Transform target)
    {
        if (target == null) return float.MaxValue;
        
        // 获取碰撞体
        Collider myCollider = GetComponent<Collider>();
        Collider targetCollider = target.GetComponent<Collider>();
        
        if (myCollider != null && targetCollider != null)
        {
            // 计算碰撞体边缘之间的最近距离
            Vector3 closestPointOnMe = myCollider.ClosestPoint(target.position);
            Vector3 closestPointOnTarget = targetCollider.ClosestPoint(transform.position);
            return Vector3.Distance(closestPointOnMe, closestPointOnTarget);
        }
        
        // 如果没有碰撞体，使用中心点距离
        return Vector3.Distance(transform.position, target.position);
    }
    #endregion

    #region Damage and Alert System
    protected virtual void OnTakeDamageWithAttacker(int damage, GameObject attacker)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[DAMAGE] {gameObject.name} took {damage} damage from {(attacker != null ? attacker.name : "unknown")}");
        }
        
        if (isDead || isStunned) return;
        
        if (attacker != null)
        {
            Transform attackerTransform = attacker.transform;
            
            if (IsValidTarget(attackerTransform))
            {
                SetHateTarget(attackerTransform);
                AlertNearbyAllies(attackerTransform);
            }
        }
        
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(HitStunRoutine());
        
        CreateHitParticles();
        StartCoroutine(FlashRed());
    }

    public virtual void SetHateTarget(Transform attacker)
    {
        if (attacker == null || isDead) return;
        
        hateTarget = attacker;
        hateEndTime = Time.time + alertDuration;
        isAlerted = true;
        
        currentTarget = hateTarget;
        lastKnownTargetPosition = hateTarget.position;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[HATE] {gameObject.name} locked onto {attacker.name}");
        }
        
        if (currentState == State.Patrol || currentState == State.Chase)
        {
            if (!isStunned)
            {
                StopAllCoroutines();
                currentState = State.Chase;
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(lastKnownTargetPosition);
                }
            }
        }
        
        ShowAlertEffect();
    }

    protected virtual void AlertNearbyAllies(Transform attacker)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ALERT] {gameObject.name} alerting nearby allies about {attacker.name}");
        }
        
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, alertRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue;
            
            Enemy nearbyEnemy = col.GetComponent<Enemy>();
            if (nearbyEnemy != null && !nearbyEnemy.isDead)
            {
                nearbyEnemy.OnAllyAlert(attacker, transform.position);
            }
        }
    }

    public virtual void OnAllyAlert(Transform threat, Vector3 alertSource)
    {
        if (isDead || currentState == State.Dead) return;
        
        if (currentState == State.MeleeAttack || currentState == State.RangedAttack)
        {
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                RespondToAlert(threat);
            }
        }
        else
        {
            RespondToAlert(threat);
        }
    }

    protected virtual void RespondToAlert(Transform threat)
    {
        if (!IsValidTarget(threat)) return;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[ALERT] {gameObject.name} responding to alert about {threat.name}");
        }
        
        hateTarget = threat;
        hateEndTime = Time.time + alertDuration * 0.7f;
        isAlerted = true;
        
        currentTarget = threat;
        lastKnownTargetPosition = threat.position;
        
        if (currentState == State.Patrol)
        {
            StopAllCoroutines();
            currentState = State.Chase;
            agent.speed = chaseSpeed;
            agent.isStopped = false;
        }
        
        ShowAlertEffect(0.7f);
    }

    protected void ShowAlertEffect(float scale = 1f)
    {
        if (showAlertEffect && alertEffectPrefab != null)
        {
            GameObject alertEffect = Instantiate(alertEffectPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            alertEffect.transform.localScale = Vector3.one * scale;
            alertEffect.transform.SetParent(transform);
            Destroy(alertEffect, 2f);
        }
        
        if (alertSound != null)
        {
            AudioSource.PlayClipAtPoint(alertSound, transform.position);
        }
    }
    #endregion

    #region State Updates
    protected virtual void PatrolUpdate()
    {
        // Additional patrol logic can be added in child classes
    }

    protected virtual void ChaseUpdate()
    {
        if (currentTarget == null) 
        {
            ReturnToPatrol();
            return;
        }

        if (!IsValidTarget(currentTarget))
        {
            currentTarget = null;
            EvaluateAndSelectTarget();
            return;
        }

        lastKnownTargetPosition = currentTarget.position;
        
        // 使用实际距离而不是中心点距离
        float distance = GetActualDistance(currentTarget);

        // 如果正在攻击，不要处理移动
        if (currentAttackRoutine != null) return;

        // 根据距离决定行为
        if (distance <= attackRange)
        {
            // 在攻击范围内，立即停止并攻击
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            currentState = State.MeleeAttack;
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
            
            if (enableDebugLogs)
            {
                Debug.Log($"[CHASE] {gameObject.name} in attack range ({distance}), starting melee attack");
            }
        }
        else
        {
            // 继续追击
            if (agent.enabled && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.SetDestination(lastKnownTargetPosition);
            }
            
            // 检查远程攻击（由子类实现）
            CheckRangedAttack(distance);
        }
    }

    protected virtual void CheckRangedAttack(float distance)
    {
        // Override in child classes that have ranged attacks
    }

    protected virtual void RangedAttackUpdate()
    {
        // Override in child classes that have ranged attacks
    }

    protected virtual void MeleeAttackUpdate()
    {
        // Additional melee logic can be added in child classes
    }
    #endregion

    #region Combat
    protected virtual IEnumerator MeleeAttackRoutine()
    {
        // 立即停止移动并锁定位置
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        
        Vector3 attackPosition = transform.position;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[MELEE] {gameObject.name} starting melee attack routine");
        }
        
        while (currentState == State.MeleeAttack && currentTarget != null && IsValidTarget(currentTarget))
        {
            // 检查是否被击晕
            if (isStunned)
            {
                currentAttackRoutine = null;
                yield break;
            }
            
            // 使用实际距离
            float currentDistance = GetActualDistance(currentTarget);
            
            // 如果目标跑远了，退出攻击
            if (currentDistance > attackRange * 1.5f)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[MELEE] Target too far ({currentDistance}), breaking attack");
                }
                break;
            }

            // 检查冷却
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 面向目标
                Vector3 lookPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);
                transform.LookAt(lookPos);

                // 立即播放攻击动画
                PerformMeleeAttack();

                // 等待动画到达伤害帧
                yield return new WaitForSeconds(meleeAttackDelay);

                // 再次检查距离和有效性
                currentDistance = GetActualDistance(currentTarget);
                if (currentDistance <= attackRange * 1.2f && IsValidTarget(currentTarget))
                {
                    DealMeleeDamage();
                }

                lastAttackTime = Time.time;
                
                // 保持位置
                transform.position = attackPosition;
            }
            
            yield return null;
        }

        // 恢复控制
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        currentState = State.Chase;
        currentAttackRoutine = null;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[MELEE] {gameObject.name} exiting melee attack, returning to chase");
        }
    }

    protected virtual void PerformMeleeAttack()
    {
        if (Random.value < 0.5f) 
            animator.SetTrigger(attack02Trigger);
        else 
            animator.SetTrigger(attack03Trigger);
    }

    protected virtual void DealMeleeDamage()
    {
        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(meleeDamage, gameObject);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[COMBAT] {gameObject.name} dealt {meleeDamage} melee damage to {currentTarget.name}");
            }
        }
    }
    #endregion

    #region Patrol System
    protected IEnumerator PatrolRoutine()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PATROL] {gameObject.name} starting patrol routine");
        }
        
        while (currentState == State.Patrol)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint(transform.position, patrolRange);
            if (randomPoint != Vector3.zero)
            {
                agent.SetDestination(randomPoint);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[PATROL] {gameObject.name} moving to {randomPoint}");
                }
                
                float moveTime = Random.Range(minMoveTime, maxMoveTime);
                float elapsedTime = 0f;
                
                while (elapsedTime < moveTime && agent.remainingDistance > agent.stoppingDistance)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            agent.isStopped = true;
            
            if (Random.value < idle02Chance) 
                animator.SetTrigger(idle02Trigger);

            yield return new WaitForSeconds(idleTime);
            agent.isStopped = false;
        }
    }

    protected Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[PATROL] {gameObject.name} failed to find valid NavMesh point");
        }
        
        return Vector3.zero;
    }

    protected virtual void ReturnToPatrol()
    {
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }
        
        currentState = State.Patrol;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        currentTarget = null;
        StartCoroutine(PatrolRoutine());
        
        if (enableDebugLogs)
        {
            Debug.Log($"[STATE] {gameObject.name} returning to patrol");
        }
    }
    #endregion

    #region Effects
    protected IEnumerator HitStunRoutine()
    {
        isStunned = true;
        
        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
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
        
        if (hateTarget != null && Time.time < hateEndTime)
        {
            currentTarget = hateTarget;
            lastKnownTargetPosition = hateTarget.position;
            currentState = State.Chase;
            agent.speed = chaseSpeed;
        }
        
        if (currentState == State.Chase && currentTarget != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(lastKnownTargetPosition);
        }
        
        stunCoroutine = null;
    }

    protected void CreateHitParticles()
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

    protected IEnumerator FlashRed()
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

    #region Death
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[DEATH] {gameObject.name} died");

        StopAllCoroutines();
        currentState = State.Dead;
        agent.isStopped = true;
        animator.SetTrigger(deathTrigger);

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        agent.enabled = false;

        // Call child class death logic
        OnDeath();

        enabled = false;
        Destroy(gameObject, GetDeathDelay());
    }

    protected virtual void OnDeath()
    {
        // Override in child classes for specific death behavior
    }

    protected virtual float GetDeathDelay()
    {
        return 2f; // Default death delay
    }
    #endregion

    #region Public Interface
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

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
    #endregion

    protected virtual void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamagedByAttacker -= OnTakeDamageWithAttacker;
            health.OnDeath -= Die;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Basic gizmos
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * detectionHeightOffset, detectionDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position + Vector3.up, currentTarget.position + Vector3.up);
        }
        
        // 显示实际的攻击范围（考虑碰撞体）
        if (Application.isPlaying)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                // 显示有效攻击范围
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Gizmos.DrawWireSphere(transform.position, attackRange + col.bounds.extents.magnitude);
                
                // 显示退出攻击范围
                Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
                Gizmos.DrawWireSphere(transform.position, attackRange * 1.5f + col.bounds.extents.magnitude);
            }
        }
    }
}