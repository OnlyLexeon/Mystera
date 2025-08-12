using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // 确保物体有 SpriteRenderer 组件
public class AlwaysFaceCamera : MonoBehaviour
{
    [Tooltip("目标相机（默认主相机）")]
    public Camera targetCamera;
    
    [Tooltip("是否锁定 Y 轴（适合 3D 地面物体）")]
    public bool lockYAxis = true;

    void Start()
    {
        // 如果未手动指定相机，默认使用主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

      
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            // 计算朝向方向
            Vector3 direction = targetCamera.transform.position - transform.position;
            
            // 如果锁定 Y 轴，则忽略垂直方向
            if (lockYAxis)
            {
                direction.y = 0;
            }

            // 避免零向量错误（当物体和相机位置完全重合时）
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}