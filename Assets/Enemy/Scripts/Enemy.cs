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
    public bool checkLineOfSight = true;
    public LayerMask obstacleLayers;

    [Header("Alert System")]
    public float alertRadius = 10f;
    public float alertDuration = 10f;
    public bool showAlertEffect = true;
    public GameObject alertEffectPrefab;
    public AudioClip alertSound;
    public float alertEffectYOffset = 2f;

    [Header("Combat Settings")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float meleeAttackDelay = 0.5f;
    public int meleeDamage = 10;
    public float stoppingDistance = 1.2f;
    public bool useEdgeToEdgeDistance = true;
    public float desiredAttackDistance = 1.5f;

    [Header("Combat Sound Effects")]
    public AudioClip[] attackSounds;      // 攻击音效数组（随机播放）
    public AudioClip[] hurtSounds;        // 受伤音效数组（随机播放）
    public AudioClip deathSound;          // 死亡音效
    public float combatSoundVolume = 1f;  // 战斗音效音量
    [Range(0f, 1f)] public float attackSoundChance = 0.8f;  // 攻击音效播放概率
    public float minSoundInterval = 0.3f; // 音效最小间隔时间（避免音效重叠）

    [Header("Animation Parameters")]
    public string isMovingParam = "isMoving";
    public string idle02Trigger = "idle02";
    public string attack02Trigger = "attack02";
    public string attack03Trigger = "attack03";
    public string deathTrigger = "death";
    public string hitTrigger = "hit";

    [Header("Hit Effect Settings")]
    public float hitStunDuration = 1f;
    public float invincibilityDuration = 0.4f;  // 无敌时间
    public int particleCount = 20;
    public float particleSize = 0.1f;
    public float particleSpeed = 3f;
    public float particleLifetime = 0.5f;
    public Material particleMaterial;

    [Header("Debug Settings")]
    public bool enableDebugLogs = false;

    // Protected variables
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Health health;

    // Target management - 简化但保留必要的
    protected Transform currentTarget;
    protected List<Transform> potentialTargets = new List<Transform>();
    protected Dictionary<Transform, float> targetLastSeenTime = new Dictionary<Transform, float>();
    protected Vector3 lastKnownTargetPosition;

    // Hate system - 保留基础的仇恨系统
    protected Transform hateTarget;
    protected float hateEndTime;
    protected bool isAlerted = false;

    // State management
    public enum State { Patrol, Chase, RangedAttack, MeleeAttack, Dead }
    protected State currentState = State.Patrol;

    // Combat variables
    protected bool isDead = false;
    protected float lastAttackTime = 0f;
    protected float lastDetectionTime = 0f;
    protected float detectionInterval = 0.3f;
    protected LayerMask targetLayers;
    protected Coroutine currentAttackRoutine;
    protected bool isStunned = false;
    protected Coroutine stunCoroutine;

    // Invincibility system
    protected float lastDamageTime = -999f;
    protected bool isInvincible = false;

    // Flag for immediate detection
    protected bool immediateDetection = false;

    // Sound management
    public float lastSoundPlayTime = 0f;

    protected virtual void Start()
    {
        InitializeComponents();
        SetupInitialState();

        // 订阅伤害事件
        if (health != null)
        {
            health.OnDamaged += OnTakeDamage;
        }

        // 启动巡逻或立即检测
        if (immediateDetection)
        {
            ImmediateTargetDetection();
        }
        else
        {
            StartCoroutine(PatrolRoutine());
        }

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
        }

        // 设置标签
        gameObject.tag = "Enemy";

        // 初始化目标层级 - 默认检测Player和Pet层
        targetLayers = LayerMask.GetMask("Player", "Pet");
    }

    protected virtual void SetupInitialState()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    // 立即目标检测
    protected virtual void ImmediateTargetDetection()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[DETECT] {gameObject.name} performing immediate target detection");
        }

        // 立即执行检测
        DetectTargets();

        // 如果找到目标，立即追击
        if (currentTarget != null)
        {
            currentState = State.Chase;
            agent.speed = chaseSpeed;
            if (enableDebugLogs)
            {
                Debug.Log($"[DETECT] {gameObject.name} detected target {currentTarget.name}, chasing immediately!");
            }
        }
        else
        {
            // 没有找到目标，开始巡逻
            StartCoroutine(PatrolRoutine());
            if (enableDebugLogs)
            {
                Debug.Log($"[DETECT] {gameObject.name} no target detected, starting patrol");
            }
        }
    }

    // 设置立即检测标志
    public virtual void SetImmediateDetection()
    {
        immediateDetection = true;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if (isStunned) return;

        // 更新无敌状态
        UpdateInvincibility();

        // 每帧进行简化的目标检测
        DetectTargets();

        // 清理过期的目标记忆
        CleanupTargetMemory();

        // 更新动画
        animator.SetBool(isMovingParam, agent.velocity.magnitude > 0.1f);

        // 状态机更新
        UpdateStateMachine();
    }

    // 更新无敌状态
    protected virtual void UpdateInvincibility()
    {
        if (isInvincible && Time.time - lastDamageTime >= invincibilityDuration)
        {
            isInvincible = false;
            if (enableDebugLogs)
            {
                Debug.Log($"[INVINCIBILITY] {gameObject.name} invincibility ended");
            }
        }
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

    #region Simplified Target Detection
    protected virtual void DetectTargets()
    {
        potentialTargets.Clear();
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        // 查找所有玩家
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Transform target = player.transform;
            if (IsTargetInRange(target, out float distance))
            {
                potentialTargets.Add(target);
                targetLastSeenTime[target] = Time.time;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
        }

        // 查找所有宠物
        GameObject[] pets = GameObject.FindGameObjectsWithTag("Pet");
        foreach (GameObject pet in pets)
        {
            Transform target = pet.transform;
            if (IsTargetInRange(target, out float distance))
            {
                potentialTargets.Add(target);
                targetLastSeenTime[target] = Time.time;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
        }

        // 评估并选择目标
        EvaluateAndSelectTarget(closestTarget);
    }

    protected virtual bool IsTargetInRange(Transform target, out float distance)
    {
        distance = float.MaxValue;

        if (target == null) return false;

        // 检查Health组件
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.currentHealth <= 0) return false;

        // 计算距离
        distance = Vector3.Distance(transform.position, target.position);
        if (distance > detectionDistance) return false;

        // 计算角度
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        if (angle > detectionAngle / 2f) return false;

        // 视线检查（可选）
        if (checkLineOfSight)
        {
            Vector3 rayStart = transform.position + Vector3.up * detectionHeightOffset;
            Vector3 rayDirection = target.position - rayStart;

            if (Physics.Raycast(rayStart, rayDirection.normalized, rayDirection.magnitude, obstacleLayers))
            {
                return false;
            }
        }

        return true;
    }

    protected virtual bool IsValidTarget(Transform target)
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

    protected virtual void EvaluateAndSelectTarget(Transform closestTarget)
    {
        // 优先检查仇恨目标
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

        // 记录是否首次发现目标
        bool isFirstDetection = false;

        // 如果没有找到目标
        if (closestTarget == null)
        {
            // 如果之前有目标，检查是否还记得
            if (currentTarget != null && targetLastSeenTime.ContainsKey(currentTarget))
            {
                if (Time.time - targetLastSeenTime[currentTarget] > 5f) // 5秒记忆时间
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

        // 检查是否是首次发现目标
        if (currentTarget == null)
        {
            isFirstDetection = true;

            if (enableDebugLogs)
            {
                Debug.Log($"[FIRST_DETECT] {gameObject.name} first detection of {closestTarget.name}");
            }
        }

        // 更新当前目标
        currentTarget = closestTarget;
        lastKnownTargetPosition = closestTarget.position;
        targetLastSeenTime[closestTarget] = Time.time;

        // 如果是首次发现目标，触发警报效果
        if (isFirstDetection)
        {
            ShowAlertEffect();

            // 设置仇恨目标
            hateTarget = closestTarget;
            hateEndTime = Time.time + alertDuration;
            isAlerted = true;

            if (enableDebugLogs)
            {
                Debug.Log($"[ALERT] {gameObject.name} alerted about {closestTarget.name}");
            }
        }

        if (currentState == State.Patrol)
        {
            StopAllCoroutines();
            currentState = State.Chase;
            agent.speed = chaseSpeed;

            if (enableDebugLogs)
            {
                Debug.Log($"[DETECT] {gameObject.name} detected {currentTarget.name}, switching to Chase");
            }
        }
    }

    protected void CleanupTargetMemory()
    {
        List<Transform> toRemove = new List<Transform>();

        foreach (var kvp in targetLastSeenTime)
        {
            if (kvp.Key == null || Time.time - kvp.Value > 10f)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (Transform t in toRemove)
        {
            targetLastSeenTime.Remove(t);
        }
    }

    protected virtual float GetActualDistance(Transform target)
    {
        if (target == null) return float.MaxValue;

        if (!useEdgeToEdgeDistance)
        {
            return Vector3.Distance(transform.position, target.position);
        }

        Collider myCollider = GetComponent<Collider>();
        Collider targetCollider = target.GetComponent<Collider>();

        if (myCollider != null && targetCollider != null)
        {
            Vector3 closestPointOnMe = myCollider.ClosestPoint(target.position);
            Vector3 closestPointOnTarget = targetCollider.ClosestPoint(transform.position);
            float edgeDistance = Vector3.Distance(closestPointOnMe, closestPointOnTarget);

            if (enableDebugLogs)
            {
                Debug.Log($"[DISTANCE] {gameObject.name} to {target.name}: Edge={edgeDistance:F2}, Center={Vector3.Distance(transform.position, target.position):F2}");
            }

            return edgeDistance;
        }

        return Vector3.Distance(transform.position, target.position);
    }

    protected virtual float CalculateStoppingDistance(Transform target)
    {
        if (target == null || !useEdgeToEdgeDistance) return stoppingDistance;

        float myRadius = 0f;
        float targetRadius = 0f;

        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            if (myCollider is CapsuleCollider capsule)
                myRadius = capsule.radius;
            else if (myCollider is SphereCollider sphere)
                myRadius = sphere.radius;
            else
                myRadius = myCollider.bounds.extents.x;
        }

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            if (targetCollider is CapsuleCollider capsule)
                targetRadius = capsule.radius;
            else if (targetCollider is SphereCollider sphere)
                targetRadius = sphere.radius;
            else
                targetRadius = targetCollider.bounds.extents.x;
        }

        float calculatedDistance = desiredAttackDistance + myRadius + targetRadius;

        if (enableDebugLogs)
        {
            Debug.Log($"[STOP_DISTANCE] {gameObject.name}: myRadius={myRadius:F2}, targetRadius={targetRadius:F2}, stopDist={calculatedDistance:F2}");
        }

        return calculatedDistance;
    }
    #endregion

    #region Damage and Alert System
    // 伤害回调
    public virtual void OnTakeDamage(int damage)
    {
        if (isDead) return;

        // 检查是否在无敌时间内
        if (isInvincible)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[INVINCIBILITY] {gameObject.name} ignored damage during invincibility");
            }
            return;
        }

        // 设置无敌状态
        isInvincible = true;
        lastDamageTime = Time.time;

        if (enableDebugLogs)
        {
            Debug.Log($"[DAMAGE] {gameObject.name} took {damage} damage, invincible for {invincibilityDuration}s");
        }

        // 播放受伤音效
        PlayHurtSound();

        // 触发受击硬直
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(HitStunRoutine());

        // 生成受击粒子效果
        CreateHitParticles();

        // 闪红效果
        StartCoroutine(FlashRed());
    }

    public virtual void OnTakeDamageWithAttacker(int damage, GameObject attacker)
    {
        if (isDead) return;

        // 检查是否在无敌时间内
        if (isInvincible)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[INVINCIBILITY] {gameObject.name} ignored damage from {(attacker != null ? attacker.name : "unknown")} during invincibility");
            }
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[DAMAGE] {gameObject.name} took {damage} damage from {(attacker != null ? attacker.name : "unknown")}");
        }

        if (attacker != null)
        {
            Transform attackerTransform = attacker.transform;

            if (IsValidTarget(attackerTransform))
            {
                SetHateTarget(attackerTransform);
                AlertNearbyAllies(attackerTransform);
            }
        }

        // 调用基础伤害处理
        OnTakeDamage(damage);
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

        ShowAlertEffect();
    }

    protected void ShowAlertEffect()
    {
        if (showAlertEffect && alertEffectPrefab != null)
        {
            // 计算生成位置（敌人上方）
            Vector3 spawnPosition = transform.position + Vector3.up * alertEffectYOffset;

            // 实例化预制体，保持原始旋转和缩放
            GameObject alertEffect = Instantiate(alertEffectPrefab,
                                                spawnPosition,
                                                Quaternion.identity);

            // 设置为敌人的子对象，跟随移动
            alertEffect.transform.SetParent(transform);

            // 重置局部位置（保持预制体原始大小）
            alertEffect.transform.localPosition = new Vector3(0, alertEffectYOffset, 0);

            // 保持预制体原始旋转
            alertEffect.transform.localRotation = Quaternion.identity;

            // 注意：不再修改缩放比例，保持预制体原始大小！

            // 2秒后销毁
            Destroy(alertEffect, 2f);

            if (enableDebugLogs)
            {
                Debug.Log($"[ALERT_EFFECT] Spawned with original scale at local position: {alertEffect.transform.localPosition}");
            }
        }

        if (alertSound != null)
        {
            AudioSource.PlayClipAtPoint(alertSound, transform.position, combatSoundVolume);
        }
    }
    #endregion

    #region State Updates
    protected virtual void PatrolUpdate()
    {
        // 额外的巡逻逻辑
    }

    protected virtual void ChaseUpdate()
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
        else
        {
            // 确保agent没有被停止
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            agent.SetDestination(lastKnownTargetPosition);

            // 检查是否可以进行远程攻击
            CheckRangedAttack(distance);
        }
    }

    protected virtual void CheckRangedAttack(float distance)
    {
        // 由子类覆盖实现远程攻击
    }

    protected virtual void RangedAttackUpdate()
    {
        // 额外的远程攻击逻辑
    }

    protected virtual void MeleeAttackUpdate()
    {
        // 额外的近战攻击逻辑
    }
    #endregion

    #region Combat
    protected virtual IEnumerator MeleeAttackRoutine()
    {
        while (currentState == State.MeleeAttack && currentTarget != null && IsValidTarget(currentTarget))
        {
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance > attackRange * 1.2f)
            {
                break;
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

            // 播放攻击动画
            PerformMeleeAttack();

            yield return new WaitForSeconds(meleeAttackDelay);

            // 检查距离并造成伤害
            currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance <= attackRange * 1.2f && IsValidTarget(currentTarget))
            {
                DealMeleeDamage();
            }

            lastAttackTime = Time.time;
            yield return new WaitForSeconds(0.5f);
        }

        agent.isStopped = false;
        currentState = State.Chase;
        currentAttackRoutine = null;
    }

    protected virtual void PerformMeleeAttack()
    {
        // 播放攻击音效
        PlayAttackSound();

        // 播放攻击动画
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

    #region Sound Effects
    protected virtual void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && CanPlaySound())
        {
            if (Random.value <= attackSoundChance)
            {
                AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
                if (clip != null)
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume);
                    lastSoundPlayTime = Time.time;
                }
            }
        }
    }

    protected virtual void PlayHurtSound()
    {
        if (hurtSounds != null && hurtSounds.Length > 0 && CanPlaySound())
        {
            AudioClip clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume);
                lastSoundPlayTime = Time.time;
            }
        }
    }

    protected virtual void PlayDeathSound()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, combatSoundVolume);
        }
    }

    protected bool CanPlaySound()
    {
        return Time.time - lastSoundPlayTime >= minSoundInterval;
    }
    #endregion

    #region Patrol System
    protected IEnumerator PatrolRoutine()
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
    }
    #endregion

    #region Effects
    protected virtual IEnumerator HitStunRoutine()
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

        if (currentState == State.Chase && currentTarget != null)
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

        if (enableDebugLogs)
        {
            Debug.Log($"[DEATH] {gameObject.name} starting death sequence");
        }

        // 播放死亡音效
        PlayDeathSound();

        StopAllCoroutines();
        currentState = State.Dead;
        agent.isStopped = true;

        if (!string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        agent.enabled = false;

        OnDeath();

        float deathDelay = GetDeathDelay();
        enabled = false;
        Destroy(gameObject, deathDelay);
    }

    protected virtual void OnDeath()
    {
        // 由子类覆盖实现特定的死亡行为
    }

    protected virtual float GetDeathDelay()
    {
        return 2f; // 默认死亡动画时间
    }
    #endregion

    #region Public Methods
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsAttacking(Transform target)
    {
        return currentTarget == target && (currentState == State.MeleeAttack || currentState == State.RangedAttack);
    }

    // 强制设置目标
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

    // 获取是否处于无敌状态
    public bool IsInvincible()
    {
        return isInvincible;
    }
    #endregion

    #region Debug
    protected virtual void OnDestroy()
    {
        // 取消订阅事件以避免内存泄漏
        if (health != null)
        {
            health.OnDamaged -= OnTakeDamage;
            health.OnDeath -= Die;
            health.OnDamagedByAttacker -= OnTakeDamageWithAttacker;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // 绘制检测锥
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Vector3 center = transform.position + Vector3.up * detectionHeightOffset;
        Vector3 forward = transform.forward * detectionDistance;

        int segments = 20;
        float deltaAngle = detectionAngle / segments;
        Vector3 prevPoint = center + Quaternion.Euler(0, -detectionAngle / 2, 0) * forward;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 point = center + Quaternion.Euler(0, -detectionAngle / 2 + deltaAngle * i, 0) * forward;
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

        // 绘制巡逻范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRange);

        // 绘制当前目标连线
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(center, currentTarget.position + Vector3.up * detectionHeightOffset);
        }

        // 绘制无敌状态
        if (isInvincible)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 1f);
        }
    }
    #endregion
}