using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BlackScreenController : MonoBehaviour
{
    private static BlackScreenController instance;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private float fadeSpeed = 1f;
    private Texture2D blackTexture;
    private bool isPaused = false;
    
    public static BlackScreenController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BlackScreenController>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        instance = this;
        
        // 创建一个1x1的黑色纹理
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }
    
    void Update()
    {
        // 平滑过渡到目标透明度
        if (currentAlpha != targetAlpha)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            
            // 完全黑屏后暂停游戏
            if (currentAlpha >= 0.99f && !isPaused && targetAlpha >= 1f)
            {
                currentAlpha = 1f; // 确保完全不透明
                isPaused = true;
                Invoke("PauseGame", 0.5f); // 0.5秒后暂停
            }
        }
    }
    
    public void FadeToBlack(float duration)
    {
        targetAlpha = 1f;
        fadeSpeed = 1f / duration;
        Debug.Log("开始黑屏效果");
    }
    
    public void FadeFromBlack(float duration)
    {
        targetAlpha = 0f;
        fadeSpeed = 1f / duration;
        isPaused = false;
        Time.timeScale = 1f; // 恢复游戏
    }
    
    void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("游戏已暂停");
    }
    
    void OnGUI()
    {
        if (currentAlpha > 0f)
        {
            // 设置GUI颜色和透明度
            GUI.color = new Color(0, 0, 0, currentAlpha);
            
            // 绘制全屏黑色矩形
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            
            // 恢复GUI颜色
            GUI.color = Color.white;
        }
    }
    
    // 备用方案：使用OnRenderImage
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
        
        if (currentAlpha > 0f)
        {
            // 创建临时的RenderTexture
            RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height);
            
            // 清空为黑色
            Graphics.Blit(source, temp);
            
            // 设置渲染目标
            RenderTexture.active = destination;
            GL.Clear(true, true, new Color(0, 0, 0, 1));
            
            // 绘制原图像（带透明度）
            GL.PushMatrix();
            GL.LoadOrtho();
            
            // 绘制原始画面（根据alpha值调整）
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), source, new Rect(0, 0, 1, 1), 0, 0, 0, 0, new Color(1, 1, 1, 1 - currentAlpha));
            
            GL.PopMatrix();
            
            RenderTexture.ReleaseTemporary(temp);
        }
    }
    
    void OnDestroy()
    {
        if (blackTexture != null)
        {
            DestroyImmediate(blackTexture);
        }
    }
    
    // 调试信息
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // 在Scene视图中显示当前黑屏透明度
            UnityEditor.Handles.Label(transform.position, $"Black Screen Alpha: {currentAlpha:F2}");
        }
    }
}