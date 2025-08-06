using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    [Range(0, 0.5f)] public float moveSmoothTime = 0.1f;

    [Header("视角设置")]
    public float mouseSensitivity = 100f;
    [Range(-90, 0)] public float minVerticalAngle = -85f;
    [Range(0, 90)] public float maxVerticalAngle = 85f;
    [Range(0, 0.5f)] public float lookSmoothTime = 0.1f;

    private float cameraPitch = 0f;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 currentDirectionVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    void Start()
    {
        // 直接使用当前物体（相机）的Transform
        cameraPitch = transform.localEulerAngles.x;
        
        // 初始化光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        // 获取输入
        Vector2 targetDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 平滑输入
        currentDirection = Vector2.SmoothDamp(
            currentDirection,
            targetDirection,
            ref currentDirectionVelocity,
            moveSmoothTime
        );

        // 计算移动速度（跑步/行走）
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 基于自身朝向移动
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = transform.right;

        Vector3 movement = (right * currentDirection.x + forward * currentDirection.y) * currentSpeed;
        transform.Translate(movement * Time.deltaTime, Space.World);
    }

    void HandleMouseLook()
    {
        // 获取鼠标输入
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        ) * mouseSensitivity * Time.deltaTime;

        // 平滑鼠标输入
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            lookSmoothTime
        );

        // 水平旋转（左右看）
        transform.Rotate(Vector3.up * currentMouseDelta.x);

        // 垂直旋转（上下看）
        cameraPitch -= currentMouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, minVerticalAngle, maxVerticalAngle);
        transform.localEulerAngles = new Vector3(cameraPitch, transform.localEulerAngles.y, 0);
    }
}