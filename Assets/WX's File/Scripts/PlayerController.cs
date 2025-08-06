using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;
    
    [Header("鼠标控制")]
    public float mouseSensitivity = 2f;  // 鼠标灵敏度
    public bool lockCursor = true;  // 是否锁定鼠标
    
    [Header("地面检测")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask = -1;
    
    [Header("攻击设置")]
    public float attackRange = 10f;
    public float attackDamageMultiplier = 0.5f; // 造成一半伤害
    public float attackCooldown = 0.5f;

    [Header("特效和音效")]
    public GameObject attackEffectPrefab;
    public Transform attackEffectSpawnPoint;
    public AudioClip attackSound;
    
    [Header("目标指示器")]
    public GameObject targetIndicatorPrefab;
    public float indicatorHeight = 2f;
    public Color indicatorColor = Color.red;
    
    // 组件
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;

    
    void Start()
    {
        // 获取组件
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // 获取或添加音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 创建攻击效果生成点（如果没有指定）
        if (attackEffectSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("AttackEffectSpawnPoint");
            spawnPoint.transform.parent = transform;
            spawnPoint.transform.localPosition = new Vector3(0, 1.5f, 1f);
            attackEffectSpawnPoint = spawnPoint.transform;
        }
        
        // 设置鼠标锁定
        SetCursorLock(lockCursor);
    }
    
    void Update()
    {

    }

    

    void SetCursorLock(bool locked)
    {
        lockCursor = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
    
   
}