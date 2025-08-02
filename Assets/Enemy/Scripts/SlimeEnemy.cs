using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class SlimeEnemy : Enemy
{
    public enum SlimeType 
    {
        Large,
        Medium,
        Small
    }
    
    [Header("Slime Settings")]
    public SlimeType slimeType = SlimeType.Large;
    
    [Header("Split Settings")]
    public GameObject mediumSlimePrefab;
    public GameObject miniSlimePrefab;
    public float splitRadius = 2f;
    public float splitForce = 5f;
    
    [Header("Slime Speed Multipliers")]
    public float miniSlimeSpeedMultiplier = 2f;
    public float mediumSlimeSpeedMultiplier = 1.5f;
    
    [Header("Ranged Attack Settings")]
    public float rangedAttackRange = 8f;
    [Range(0f, 1f)] public float chaseToRangedThreshold = 0.8f;
    [Range(1f, 2f)] public float rangedAbandonThreshold = 1.2f;
    public int maxRangedMisses = 3;
    public float rangedAttackDelay = 0.3f;
    public int rangedDamage = 15;
    
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileArcHeight = 2f;
    public float projectileSpeed = 15f;
    
    [Header("Ranged Effect Settings")]
    public GameObject rangedAttackEffectPrefab;
    public Transform effectSpawnPoint;
    public float effectDuration = 1f;
    public float effectSpawnDelay = 0.15f;
    public AudioClip rangedAttackSound;
    public float soundDelay = 0.1f;
    public string crawlTrigger = "crawl";
    
    [Header("Split Effect Settings")]
    public GameObject splitEffectPrefab;
    public AudioClip splitSound;
    
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public float explosionDestroyDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionDamage = 20f;
    public float explosionForce = 10f;
    public LayerMask explosionDamageLayers;
    
    [Header("Test Settings")]
    public bool enableTestMode = false;
    public KeyCode testAttackKey = KeyCode.F;
    public int testDamagePerHit = 50;
    public GameObject testHitEffectPrefab;
    public AudioClip testHitSound;
    
    // Slime specific variables
    private int rangedMissCount = 0;
    private GameObject currentAttackEffect;
    private int testAttackCount = 0;
    private bool immediateDetection = false;
    
    protected override void Start()
    {
        base.Start();
        SetupSpeedMultiplier();
        
        if (immediateDetection)
        {
            ImmediateTargetDetection();
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        #if UNITY_EDITOR
        if (enableTestMode && Input.GetKeyDown(testAttackKey))
        {
            TestAttack();
        }
        #endif
    }
    
    protected override void SetupInitialState()
    {
        base.SetupInitialState();
        
        // 为不同大小的史莱姆设置不同的参数
        switch (slimeType)
        {
            case SlimeType.Small:
                agent.radius = 0.3f;  // 更小的碰撞半径
                agent.speed = patrolSpeed * miniSlimeSpeedMultiplier;
                break;
            case SlimeType.Medium:
                agent.radius = 0.5f;
                agent.speed = patrolSpeed * mediumSlimeSpeedMultiplier;
                break;
            case SlimeType.Large:
                agent.radius = 0.8f;
                agent.speed = patrolSpeed;
                break;
        }
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
    
    #region Ranged Attack
    protected override void CheckRangedAttack(float distance)
    {
        if (slimeType == SlimeType.Large && distance <= rangedAttackRange && distance > attackRange)
        {
            currentState = State.RangedAttack;
            currentAttackRoutine = StartCoroutine(RangedAttackRoutine());
        }
    }
    
    IEnumerator RangedAttackRoutine()
    {
        if (slimeType != SlimeType.Large) yield break;

        while (currentState == State.RangedAttack && currentTarget != null && IsValidTarget(currentTarget))
        {
            // 简单的距离判断
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
            
            Vector3 lookPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);
            transform.LookAt(lookPos);

            animator.SetTrigger(crawlTrigger);
            
            if (effectSpawnDelay > 0)
            {
                yield return new WaitForSeconds(effectSpawnDelay);
            }
            
            SpawnRangedEffect();
            PlayRangedSound();

            yield return new WaitForSeconds(rangedAttackDelay - effectSpawnDelay);

            // 再次验证目标
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                agent.isStopped = false;
                currentState = State.Chase;
                yield break;
            }

            currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if (currentDistance > rangedAttackRange * rangedAbandonThreshold)
            {
                agent.isStopped = false;
                currentState = State.Chase;
                yield break;
            }

            LaunchProjectile();

            lastAttackTime = Time.time;
            yield return new WaitForSeconds(0.7f);
        }

        agent.isStopped = false;
        currentState = State.Chase;
        rangedMissCount = 0;
        currentAttackRoutine = null;
    }
    
    void SpawnRangedEffect()
    {
        if (rangedAttackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject attackEffect = Instantiate(
                rangedAttackEffectPrefab,
                effectSpawnPoint.position,
                effectSpawnPoint.rotation,
                effectSpawnPoint
            );
            Destroy(attackEffect, effectDuration);
        }
    }
    
    void PlayRangedSound()
    {
        if (rangedAttackSound != null)
        {
            if (soundDelay > 0)
            {
                StartCoroutine(PlaySoundDelayed(rangedAttackSound, soundDelay));
            }
            else
            {
                AudioSource.PlayClipAtPoint(rangedAttackSound, transform.position);
            }
        }
    }
    
    IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource.PlayClipAtPoint(clip, transform.position);
    }
    
    void LaunchProjectile()
    {
        // 发射前再次验证目标
        if (currentTarget == null || !IsValidTarget(currentTarget))
        {
            return;
        }
        
        if (projectilePrefab && projectileSpawnPoint)
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
            proj.attacker = gameObject;
            proj.SetTarget(currentTarget);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[COMBAT] {gameObject.name} launched projectile at {currentTarget.name}");
            }
        }
    }
    #endregion
    
    #region Split Mechanic
    protected override void OnDeath()
    {
        bool shouldSplit = false;
        switch (slimeType)
        {
            case SlimeType.Large:
                shouldSplit = mediumSlimePrefab != null;
                break;
            case SlimeType.Medium:
                shouldSplit = miniSlimePrefab != null;
                break;
            case SlimeType.Small:
                shouldSplit = false;
                break;
        }

        if (shouldSplit)
        {
            PerformSplit();
        }
    }
    
    protected override float GetDeathDelay()
    {
        return 0.1f; // Quick destroy for slimes
    }
    
    void PerformSplit()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SPLIT] {gameObject.name} splitting");
        }
        
        // Explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDestroyDelay);
            
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(explosion, totalDuration);
            }
            
            ApplyExplosionEffects();
        }

        // Split effect
        if (splitEffectPrefab != null)
        {
            GameObject effect = Instantiate(splitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (splitSound != null)
        {
            AudioSource.PlayClipAtPoint(splitSound, transform.position);
        }

        SpawnChildSlimes();
    }
    
    void SpawnChildSlimes()
    {
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

        if (slimePrefabToSpawn != null)
        {
            Transform savedHateTarget = hateTarget;
            float savedHateEndTime = hateEndTime;
            
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
                
                Rigidbody rb = childSlime.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 pushDirection = (spawnPosition - transform.position).normalized;
                    pushDirection.y = 0.5f;
                    rb.AddForce(pushDirection * splitForce, ForceMode.Impulse);
                }

                SlimeEnemy childAI = childSlime.GetComponent<SlimeEnemy>();
                if (childAI != null)
                {
                    childAI.slimeType = childSlimeType;
                    
                    if (savedHateTarget != null && Time.time < savedHateEndTime)
                    {
                        StartCoroutine(SetChildHateTargetDelayed(childAI, savedHateTarget, 0.1f));
                    }
                    else
                    {
                        childAI.SetImmediateDetection();
                    }
                }
            }
        }
    }
    
    IEnumerator SetChildHateTargetDelayed(SlimeEnemy childAI, Transform hateTarget, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (childAI != null && hateTarget != null)
        {
            childAI.SetHateTarget(hateTarget);
            
            // 让子史莱姆自己处理初始状态，因为它会在下一帧的Update中检测距离
            childAI.ForceImmediateAttackCheck();
        }
    }
    
    void ApplyExplosionEffects()
    {
        // 根据史莱姆大小调整爆炸威力
        float actualExplosionDamage = GetExplosionDamage();
        float actualExplosionRadius = GetExplosionRadius();
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, actualExplosionRadius, explosionDamageLayers);
        
        foreach (Collider hit in colliders)
        {
            Health targetHealth = hit.GetComponent<Health>();
            if (targetHealth != null && hit.gameObject != gameObject)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damageMultiplier = 1f - (distance / actualExplosionRadius);
                float actualDamage = actualExplosionDamage * damageMultiplier;
                
                targetHealth.TakeDamage((int)actualDamage, gameObject);
            }
            
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                direction.y = 0.5f;
                
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float forceMultiplier = 1f - (distance / actualExplosionRadius);
                float actualForce = explosionForce * forceMultiplier;
                
                rb.AddForce(direction * actualForce, ForceMode.Impulse);
            }
        }
    }
    
    float GetExplosionDamage()
    {
        switch (slimeType)
        {
            case SlimeType.Large: return explosionDamage;
            case SlimeType.Medium: return explosionDamage * 0.6f;
            case SlimeType.Small: return explosionDamage * 0.3f;
            default: return explosionDamage;
        }
    }
    
    float GetExplosionRadius()
    {
        switch (slimeType)
        {
            case SlimeType.Large: return explosionRadius;
            case SlimeType.Medium: return explosionRadius * 0.7f;
            case SlimeType.Small: return explosionRadius * 0.4f;
            default: return explosionRadius;
        }
    }
    #endregion
    
    #region Special Behaviors
    public void SetImmediateDetection()
    {
        immediateDetection = true;
    }
    
    // 新增：强制立即检查攻击
    public void ForceImmediateAttackCheck()
    {
        if (currentTarget != null && IsValidTarget(currentTarget))
        {
            // 使用简单的距离检查
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange * 1.2f && currentState != State.MeleeAttack)
            {
                // 如果已经很近了，直接进入攻击状态
                StopAllCoroutines();
                currentState = State.MeleeAttack;
                agent.isStopped = true;
                currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
            }
        }
    }
    
    void ImmediateTargetDetection()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SPLIT] {gameObject.name} performing immediate target detection");
        }
        
        DetectTargets();
        
        if (currentTarget != null)
        {
            currentState = State.Chase;
            agent.speed = chaseSpeed;
        }
        else
        {
            StartCoroutine(PatrolRoutine());
        }
    }
    
    void TestAttack()
    {
        if (isDead) return;
        
        testAttackCount++;
        Debug.Log($"[TEST] Attack #{testAttackCount} on {gameObject.name}");
        
        CreateTestHitEffect();
        
        int healthBefore = health.currentHealth;
        GameObject testAttacker = GameObject.FindGameObjectWithTag("Player");
        health.TakeDamage(testDamagePerHit, testAttacker);
        int healthAfter = health.currentHealth;
        
        Debug.Log($"[TEST] {gameObject.name} took {testDamagePerHit} damage: {healthBefore} → {healthAfter} HP");
    }
    
    void CreateTestHitEffect()
    {
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
    }
    
    void CleanupAttackEffect()
    {
        if (currentAttackEffect != null)
        {
            Destroy(currentAttackEffect);
            currentAttackEffect = null;
        }
    }
    #endregion
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw ranged attack range for large slime
        if (slimeType == SlimeType.Large)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        }
        
        // Draw split range
        if (slimeType != SlimeType.Small)
        {
            GameObject prefabToCheck = (slimeType == SlimeType.Large) ? mediumSlimePrefab : miniSlimePrefab;
            if (prefabToCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, splitRadius);
            }
            
            // Draw explosion range with size adjustment
            if (explosionPrefab != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, GetExplosionRadius());
            }
        }
        
        // Draw test attack range
        if (enableTestMode)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 10f);
        }
    }
}