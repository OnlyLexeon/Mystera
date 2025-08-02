using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("死亡设置")]
    public float fadeToBlackDuration = 2f;
    public float respawnDelay = 1f;
    
    [Header("Vignette效果设置")]
    public Volume globalVolume;
    public AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float vignetteAnimDuration = 0.8f;
    public float maxVignetteIntensity = 0.5f;
    public float lowHealthThreshold = 0.3f;
    public float lowHealthMinIntensity = 0.15f;
    
    private Vignette vignette;
    private Coroutine vignetteCoroutine;
    private bool isDead = false;
    private bool canRespawn = false;
    
    // 事件委托
    public event Action OnDeath;
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action<int, GameObject> OnDamagedByAttacker;
    
    private GameObject lastAttacker;
    public GameObject LastAttacker => lastAttacker;
    
    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        canRespawn = false;
        lastAttacker = null;
        
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            if (vignette != null)
            {
                vignette.intensity.value = 0f;
            }
            else
            {
                Debug.LogWarning("Global Volume Profile中没有找到Vignette效果！");
            }
        }
        else
        {
            Debug.LogWarning("未设置Global Volume！");
        }
    }
    
    void Update()
    {
        if (isDead && canRespawn && Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }
    }
    
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }
    
    public void TakeDamage(int damage, GameObject attacker)
    {
        if (damage <= 0 || isDead) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        int actualDamage = previousHealth - currentHealth;
        
        if (attacker != null)
        {
            lastAttacker = attacker;
        }
        
        OnDamaged?.Invoke(actualDamage);
        OnDamagedByAttacker?.Invoke(actualDamage, attacker);
        
        if (CompareTag("Player") && vignette != null)
        {
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }
            vignetteCoroutine = StartCoroutine(PlayVignetteAnimation());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void SetLastAttacker(GameObject attacker)
    {
        lastAttacker = attacker;
    }
    
    private IEnumerator PlayVignetteAnimation()
    {
        float elapsedTime = 0f;
        float healthPercentage = GetHealthPercentage();
        float minIntensity = healthPercentage < lowHealthThreshold ? lowHealthMinIntensity : 0f;
        
        float[] keyTimes = { 0f, 0.3f, 0.6f, 1f };
        float[] keyValues = { 
            vignette.intensity.value,
            maxVignetteIntensity,
            0.3f,
            minIntensity
        };
        
        while (elapsedTime < vignetteAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / vignetteAnimDuration;
            
            float currentIntensity = 0f;
            for (int i = 0; i < keyTimes.Length - 1; i++)
            {
                if (normalizedTime >= keyTimes[i] && normalizedTime <= keyTimes[i + 1])
                {
                    float segmentProgress = (normalizedTime - keyTimes[i]) / (keyTimes[i + 1] - keyTimes[i]);
                    segmentProgress = vignetteCurve.Evaluate(segmentProgress);
                    currentIntensity = Mathf.Lerp(keyValues[i], keyValues[i + 1], segmentProgress);
                    break;
                }
            }
            
            vignette.intensity.value = currentIntensity;
            yield return null;
        }
        
        vignette.intensity.value = minIntensity;
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"{gameObject.name} 死亡！");
        
        if (lastAttacker != null)
        {
            Debug.Log($"{gameObject.name} 被 {lastAttacker.name} 击杀！");
        }
        
        if (CompareTag("Player"))
        {
            HandlePlayerDeath();
        }
        
        OnDeath?.Invoke();
    }
    
    private void HandlePlayerDeath()
    {
        Debug.Log("玩家死亡！");
        
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        if (BlackScreenController.Instance != null)
        {
            BlackScreenController.Instance.FadeToBlack(fadeToBlackDuration);
        }
        else
        {
            Debug.LogWarning("没有找到BlackScreenController！直接暂停游戏。");
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        StartCoroutine(EnableRespawnAfterDelay());
    }
    
    private IEnumerator EnableRespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        canRespawn = true;
        Debug.Log("按下R键重新开始");
    }
    
    private void RespawnPlayer()
    {
        Debug.Log("重新加载场景...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void Heal(int amount)
    {
        Heal(amount, null);
    }
    
    public void Heal(int amount, GameObject healer)
    {
        if (amount <= 0 || isDead) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int actualHealing = currentHealth - previousHealth;
        
        OnHealed?.Invoke(actualHealing);
        
        if (CompareTag("Player") && vignette != null)
        {
            UpdateLowHealthVignette();
        }
        
        if (healer != null && actualHealing > 0)
        {
            Debug.Log($"{gameObject.name} 被 {healer.name} 治疗了 {actualHealing} 点生命值");
        }
    }
    
    private void UpdateLowHealthVignette()
    {
        if (vignette == null) return;
        
        float healthPercentage = GetHealthPercentage();
        
        if (healthPercentage >= lowHealthThreshold && vignette.intensity.value > 0)
        {
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }
            vignetteCoroutine = StartCoroutine(FadeVignetteToZero());
        }
    }
    
    private IEnumerator FadeVignetteToZero()
    {
        float startIntensity = vignette.intensity.value;
        float elapsedTime = 0f;
        float fadeDuration = 0.5f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            vignette.intensity.value = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }
        
        vignette.intensity.value = 0f;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }
    
    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }
    
    // 新增：重置死亡状态（用于复活机制）
    public void ResetDeathState()
    {
        isDead = false;
        canRespawn = false;
        lastAttacker = null;
        
        // 如果是玩家，重置Vignette效果
        if (CompareTag("Player") && vignette != null)
        {
            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
                vignetteCoroutine = null;
            }
            
            // 根据当前血量设置Vignette
            float healthPercentage = GetHealthPercentage();
            if (healthPercentage < lowHealthThreshold)
            {
                vignette.intensity.value = lowHealthMinIntensity;
            }
            else
            {
                vignette.intensity.value = 0f;
            }
        }
        
        Debug.Log($"{gameObject.name} 死亡状态已重置");
    }
    
    // 新增：设置生命值（用于复活时恢复血量）
    public void SetHealth(int newHealth)
    {
        if (newHealth < 0) newHealth = 0;
        if (newHealth > maxHealth) newHealth = maxHealth;
        
        currentHealth = newHealth;
        
        // 更新玩家的Vignette效果
        if (CompareTag("Player") && vignette != null)
        {
            UpdateLowHealthVignette();
        }
    }
}