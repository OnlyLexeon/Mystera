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
    public float soundDelay = 0.1f;
    public string crawlTrigger = "crawl";
    
    [Header("Slime Combat Sound Effects")]
    public AudioClip[] slimeMeleeAttackSounds;  // 史莱姆专用近战攻击音效
    public AudioClip[] slimeRangedAttackSounds; // 史莱姆专用远程攻击音效
    public bool useSlimeSpecificSounds = true;  // 是否使用史莱姆专用音效
    
    [Header("Slime Movement Sound")]
    public AudioClip slimeMovementSound;        // 史莱姆移动音效
    public float movementSoundVolume = 0.5f;    // 移动音效音量
    public bool playMovementSound = true;       // 是否播放移动音效
    
    [Header("Split Effect Settings")]
    public GameObject splitEffectPrefab;
    public AudioClip splitSound;
    public float splitEffectYOffset = 1f;  // 分裂特效Y轴偏移
    
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public float explosionDestroyDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionDamage = 20f;
    public float explosionForce = 10f;
    public LayerMask explosionDamageLayers;
    public float explosionEffectYOffset = 1f;  // 爆炸特效Y轴偏移
    
    
    // Slime specific variables
    private int rangedMissCount = 0;
    private GameObject currentAttackEffect;
    private int testAttackCount = 0;

    
    // Movement sound management
    private AudioSource movementAudioSource;
    private bool wasMoving = false;
    
    protected override void Start()
    {
        base.Start();
        SetupSpeedMultiplier();
        SetupMovementSound();
        
        if (immediateDetection)
        {
            ImmediateTargetDetection();
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 更新移动音效
        UpdateMovementSound();
    }
    
    protected override void SetupInitialState()
    {
        base.SetupInitialState();
        
        // 为不同大小的史莱姆设置不同的参数
        switch (slimeType)
        {
            case SlimeType.Small:
                agent.radius = 0.3f;
                agent.height = 0.6f;
                // 小史莱姆 - 基于实际碰撞体大小设置合理的停止距离
                desiredAttackDistance = 1.2f;  // 停止距离
                attackRange = 1.8f;            // 攻击范围要大于停止距离
                agent.speed = patrolSpeed * miniSlimeSpeedMultiplier;
                break;
            case SlimeType.Medium:
                agent.radius = 0.5f;
                agent.height = 1.0f;
                // 中型史莱姆
                desiredAttackDistance = 1.8f;  // 停止距离
                attackRange = 2.5f;            // 攻击范围
                agent.speed = patrolSpeed * mediumSlimeSpeedMultiplier;
                break;
            case SlimeType.Large:
                agent.radius = 0.8f;
                agent.height = 1.6f;
                // 大型史莱姆 - 缩小一倍
                desiredAttackDistance = 4f;  // 停止距离（4.5 / 2）
                attackRange = 5f;             // 攻击范围（6.0 / 2）
                agent.speed = patrolSpeed;
                break;
        }
        
        // 设置NavMeshAgent的初始停止距离
        agent.stoppingDistance = desiredAttackDistance;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SLIME] {gameObject.name} ({slimeType}) setup:\n" +
                     $"  - desiredAttackDistance: {desiredAttackDistance}\n" +
                     $"  - attackRange: {attackRange}\n" +
                     $"  - agent.stoppingDistance: {agent.stoppingDistance}\n" +
                     $"  - agent.radius: {agent.radius}");
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
    
    #region Movement Sound
    void SetupMovementSound()
    {
        if (slimeMovementSound != null && playMovementSound)
        {
            // 创建一个子对象来放置 AudioSource
            GameObject soundObject = new GameObject("MovementSound");
            soundObject.transform.SetParent(transform);
            soundObject.transform.localPosition = Vector3.zero;
            
            // 添加并配置 AudioSource
            movementAudioSource = soundObject.AddComponent<AudioSource>();
            movementAudioSource.clip = slimeMovementSound;
            movementAudioSource.loop = true;
            movementAudioSource.volume = movementSoundVolume;
            movementAudioSource.spatialBlend = 1f; // 3D 音效
            movementAudioSource.minDistance = 1f;
            movementAudioSource.maxDistance = 20f;
            movementAudioSource.rolloffMode = AudioRolloffMode.Linear;
            movementAudioSource.playOnAwake = false;
        }
    }
    
    void UpdateMovementSound()
    {
        if (movementAudioSource == null || !playMovementSound || isDead || isStunned) 
        {
            if (movementAudioSource != null && movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
            return;
        }
        
        // 检查是否在移动
        bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
        
        // 根据移动状态调整音量（追击时音量更大）
        if (currentState == State.Chase)
        {
            movementAudioSource.volume = movementSoundVolume * 1.5f;
        }
        else
        {
            movementAudioSource.volume = movementSoundVolume;
        }
        
        // 开始或停止播放
        if (isMoving && !wasMoving)
        {
            movementAudioSource.Play();
            if (enableDebugLogs)
            {
                Debug.Log($"[SLIME SOUND] {gameObject.name} started moving sound");
            }
        }
        else if (!isMoving && wasMoving)
        {
            movementAudioSource.Stop();
            if (enableDebugLogs)
            {
                Debug.Log($"[SLIME SOUND] {gameObject.name} stopped moving sound");
            }
        }
        
        wasMoving = isMoving;
    }
    #endregion
    
    #region Combat Sounds Override
    // 重写基类的攻击音效方法
    protected override void PlayAttackSound()
    {
        // 如果使用史莱姆专用音效且有设置近战音效
        if (useSlimeSpecificSounds && slimeMeleeAttackSounds != null && slimeMeleeAttackSounds.Length > 0)
        {
            if (CanPlaySound())
            {
                AudioClip clip = slimeMeleeAttackSounds[Random.Range(0, slimeMeleeAttackSounds.Length)];
                if (clip != null)
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume);
                    lastSoundPlayTime = Time.time;
                }
            }
        }
        else
        {
            // 否则使用基类的攻击音效
            base.PlayAttackSound();
        }
    }
    
    // 播放远程攻击音效的方法
    void PlayRangedAttackSound()
    {
        if (useSlimeSpecificSounds && slimeRangedAttackSounds != null && slimeRangedAttackSounds.Length > 0)
        {
            AudioClip clip = slimeRangedAttackSounds[Random.Range(0, slimeRangedAttackSounds.Length)];
            if (clip != null)
            {
                if (soundDelay > 0)
                {
                    StartCoroutine(PlaySoundDelayed(clip, soundDelay));
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume);
                }
            }
        }
    }
    #endregion
    
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
            // 使用边缘到边缘距离判断
            float currentDistance = GetActualDistance(currentTarget);
            
            if (currentDistance <= desiredAttackDistance)
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
            PlayRangedAttackSound(); // 使用新的远程攻击音效方法

            yield return new WaitForSeconds(rangedAttackDelay - effectSpawnDelay);

            // 再次验证目标
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                agent.isStopped = false;
                currentState = State.Chase;
                yield break;
            }

            currentDistance = GetActualDistance(currentTarget);
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
    
    IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource.PlayClipAtPoint(clip, transform.position, combatSoundVolume);
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
    
    protected override void Die()
    {
        // 停止移动音效
        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }
        
        base.Die();
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
        
        // Explosion effect - 添加Y轴偏移
        if (explosionPrefab != null)
        {
            Vector3 explosionPosition = transform.position + Vector3.up * explosionEffectYOffset;
            GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            Destroy(explosion, explosionDestroyDelay);
            
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(explosion, totalDuration);
            }
            
            ApplyExplosionEffects();
        }

        // Split effect - 添加Y轴偏移
        if (splitEffectPrefab != null)
        {
            Vector3 splitEffectPosition = transform.position + Vector3.up * splitEffectYOffset;
            GameObject effect = Instantiate(splitEffectPrefab, splitEffectPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (splitSound != null)
        {
            AudioSource.PlayClipAtPoint(splitSound, transform.position, combatSoundVolume);
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
    
    // 修改：强制立即检查攻击
    public void ForceImmediateAttackCheck()
    {
        if (currentTarget != null && IsValidTarget(currentTarget))
        {
            // 使用实际边缘距离检查
            float distance = GetActualDistance(currentTarget);
            if (distance <= desiredAttackDistance * 1.2f && currentState != State.MeleeAttack)
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
    #endregion
    
    protected override void OnDestroy()
    {
        // 清理音效资源
        if (movementAudioSource != null)
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
            Destroy(movementAudioSource.gameObject);
        }
        
        base.OnDestroy();
    }
    
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
        
        // Draw desired attack distance
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, desiredAttackDistance);
        }
    }
}