using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Combat Settings")]
    public bool onlyDamageOnSelect = true;
    public float attackCooldown = 1f;
    public float chargedDamage = 6f;
    public float unchargedDamage = 1f;

    private float cooldownTimer = 0f;
    private bool wasSelectedLastFrame = false;

    [Header("Selection State")]
    public bool isSelected = false;

    [Header("UI References")]
    public Canvas cooldownCanvas;
    public Slider cooldownSlider;
    public Image sliderFill;

    private bool canDealDamage = true;
    private void Start()
    {
        cooldownSlider.maxValue = attackCooldown;
        cooldownSlider.value = attackCooldown;
        UpdateSliderColor();
        cooldownCanvas.enabled = false;
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
    }


    private void Update()
    {
        //cooldown
        if (cooldownTimer < attackCooldown)
        {
            cooldownTimer += Time.deltaTime;
            cooldownTimer = Mathf.Clamp(cooldownTimer, 0f, attackCooldown);
            cooldownSlider.value = cooldownTimer;
            UpdateSliderColor();
        }

        //ui
        cooldownCanvas.enabled = isSelected;

        //selected = reset timer
        if (isSelected && !wasSelectedLastFrame)
        {
            cooldownTimer = 0f;
            cooldownSlider.value = 0f;
            UpdateSliderColor();
        }

        wasSelectedLastFrame = isSelected;
    }

    private void UpdateSliderColor()
    {
        float t = cooldownSlider.value / cooldownSlider.maxValue;

        if (t >= 0.99f)
        {
            sliderFill.color = Color.yellow;
        }
        else
        {
            // Lerp from light red (0) to bright orange (1)
            Color start = new Color(1f, 0.4f, 0.4f);   // light red
            Color end = new Color(1f, 0.5f, 0f);       // bright orange
            sliderFill.color = Color.Lerp(start, end, t);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canDealDamage) return;

        if (onlyDamageOnSelect)
        {
            if (!isSelected) return;
        }

        float t = cooldownTimer / attackCooldown;
        float damageToApply = (t >= 1f) ? chargedDamage : unchargedDamage;
        int roundedDamage = Mathf.RoundToInt(damageToApply);

        if (collision.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(roundedDamage);
        }

        // reset
        cooldownTimer = 0f;
        cooldownSlider.value = 0f;
        UpdateSliderColor();

        // prevent spam
        canDealDamage = false;
        Invoke(nameof(EnableDamage), 0.05f); // small delay
    }

    private void EnableDamage() => canDealDamage = true;

    public void OnEnemyHitSpecial() { }
}