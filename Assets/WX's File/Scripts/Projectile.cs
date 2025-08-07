using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("基本设置")]
    public float speed = 10f;
    public float arcHeight = 2f;
    public float damage = 10f;
    public float lifetime = 5f;
    public float hitRadius = 0.5f;              // 碰撞检测半径
    
    [Header("目标设置")]
    public LayerMask targetLayers;              // 可以击中的层级
    public bool canHitMultipleTargets = false;  // 是否可以穿透击中多个目标
    public int maxHitCount = 1;                 // 最大击中数量
    
    [Header("特效设置")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    public float effectDuration = 2f;
    
    [Header("附加效果")]
    public bool applyKnockback = false;         // 是否造成击退
    public float knockbackForce = 5f;           // 击退力度

    [Header("Wall Ground Layer")]
    public LayerMask groundLayer;

    // 私有变量
    private Transform target;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float progress = 0f;
    private bool isLaunched = false;
    private int currentHitCount = 0;
    private Vector3 lastPosition;
    
    // 发射源信息（用于避免击中自己）
    private GameObject shooter;
    
    // 新增：攻击者信息（用于仇恨系统）
    public GameObject attacker { get; set; }
    
    void Start()
    {
        Destroy(gameObject, lifetime);
        lastPosition = transform.position;
        
        // 如果没有设置目标层级，默认设置玩家和宠物
        if (targetLayers == 0)
        {
            targetLayers = LayerMask.GetMask("Player", "Pet");
        }
    }
    
    // 设置发射者（避免击中自己）
    public void SetShooter(GameObject shooterObject)
    {
        shooter = shooterObject;
        // 如果没有单独设置攻击者，则发射者就是攻击者
        if (attacker == null)
        {
            attacker = shooterObject;
        }
    }
    
    // 设置目标（跟踪模式）
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        startPosition = transform.position;
        targetPosition = target.position;
        isLaunched = true;
    }
    
    // 设置目标位置（固定位置模式）
    public void SetTargetPosition(Vector3 position)
    {
        target = null;  // 清除跟踪目标
        startPosition = transform.position;
        targetPosition = position;
        isLaunched = true;
    }
    
    void Update()
    {
        if (!isLaunched) return;
        
        // 保存上一帧位置（用于射线检测）
        lastPosition = transform.position;
        
        // 更新目标位置（如果目标在移动）
        if (target != null)
        {
            // 检查目标是否还存活
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.currentHealth <= 0)
            {
                // 目标已死亡，继续飞向最后位置
                target = null;
            }
            else
            {
                targetPosition = target.position;
            }
        }
        
        // 计算进度
        float distance = Vector3.Distance(startPosition, targetPosition);
        if (distance > 0)
        {
            progress += (speed * Time.deltaTime) / distance;
        }
        
        if (progress >= 1f)
        {
            // 到达目标
            OnReachTarget();
            return;
        }
        
        // 计算抛物线位置
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
        
        // 添加抛物线高度
        float arc = arcHeight * (1 - Mathf.Pow(2 * progress - 1, 2));
        currentPos.y += arc;
        
        // 更新位置
        transform.position = currentPos;
        
        // 旋转朝向运动方向
        Vector3 moveDirection = transform.position - lastPosition;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        
        // 飞行路径碰撞检测（更精确）
        CheckCollisionAlongPath();
    }
    
    void CheckCollisionAlongPath()
    {
        // 使用射线检测从上一帧到当前帧的路径
        Vector3 direction = transform.position - lastPosition;
        float distance = direction.magnitude;
        
        if (distance > 0)
        {
            RaycastHit[] hits = Physics.RaycastAll(lastPosition, direction.normalized, distance, targetLayers);
            
            foreach (RaycastHit hit in hits)
            {
                // 跳过发射者
                if (hit.collider.gameObject == shooter) continue;
                
                // 检查是否是有效目标
                if (IsValidTarget(hit.collider))
                {
                    OnHitTarget(hit.collider, hit.point);
                    
                    if (!canHitMultipleTargets || currentHitCount >= maxHitCount)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
    }
    
    bool IsValidTarget(Collider col)
    {
        // 检查层级
        if ((targetLayers.value & (1 << col.gameObject.layer)) == 0)
            return false;
        
        // 检查标签
        if (col.CompareTag("Player") || col.CompareTag("Pet"))
        {
            // 检查是否有Health组件且存活
            Health health = col.GetComponent<Health>();
            if (health != null && health.currentHealth > 0)
            {
                return true;
            }
        }
        
        return false;
    }
    
    void OnReachTarget()
    {
        // 到达目标位置时的范围伤害
        Collider[] colliders = Physics.OverlapSphere(transform.position, hitRadius, targetLayers);
        
        foreach (Collider col in colliders)
        {
            // 跳过发射者
            if (col.gameObject == shooter) continue;
            
            if (IsValidTarget(col))
            {
                OnHitTarget(col, transform.position);
            }
        }
        
        // 生成爆炸特效（即使没击中目标）
        CreateHitEffect(transform.position);
        
        // 销毁投射物
        Destroy(gameObject);
    }
    
    void OnHitTarget(Collider targetCollider, Vector3 hitPoint)
    {
        currentHitCount++;
        
        // 造成伤害 - 现在传递攻击者信息
        Health health = targetCollider.GetComponent<Health>();
        if (health != null)
        {
            // 使用新的带攻击者参数的TakeDamage方法
            health.TakeDamage((int)damage, attacker);
            
            // 记录击中信息
            string targetType = targetCollider.CompareTag("Player") ? "玩家" : "宠物";
            string attackerName = attacker != null ? attacker.name : "未知攻击者";
            Debug.Log($"[投射物] 来自 {attackerName} 的投射物击中{targetType} {targetCollider.name}，造成 {damage} 点伤害");
        }
        
        // 应用击退效果
        if (applyKnockback)
        {
            Rigidbody rb = targetCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockbackDirection = (targetCollider.transform.position - transform.position).normalized;
                knockbackDirection.y = 0.5f;  // 添加向上的分量
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
        
        // 生成击中特效
        CreateHitEffect(hitPoint);
    }
    
    void CreateHitEffect(Vector3 position)
    {
        // 生成击中特效
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
        
        // 播放击中音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }
    
    // 碰撞检测备用方案
    void OnTriggerEnter(Collider other)
    {
        // 跳过发射者
        if (other.gameObject == shooter) return;
        
        // 检查是否是有效目标
        if (IsValidTarget(other))
        {
            OnHitTarget(other, transform.position);
            
            if (!canHitMultipleTargets || currentHitCount >= maxHitCount)
            {
                Destroy(gameObject);
            }
        }
        // 击中地面或障碍物
        //else if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        //{
        //    CreateHitEffect(transform.position);
        //    Destroy(gameObject);
        //}

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            CreateHitEffect(transform.position);
            Destroy(gameObject);
        }
    }
    
    // 可视化调试
    void OnDrawGizmos()
    {
        // 绘制碰撞检测范围
        Gizmos.color = Color.red * 0.5f;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
        
        // 绘制飞行路径
        if (isLaunched && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}