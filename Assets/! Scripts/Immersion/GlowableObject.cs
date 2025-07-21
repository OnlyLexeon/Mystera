using UnityEngine;

public class GlowableObject : MonoBehaviour
{
    private Light glowLight;
    private float glowDuration;
    private float fadeOutTime;

    public void ApplyGlow(float intensity, float range, float duration, float fadeOut = 2f)
    {
        glowDuration = duration;
        fadeOutTime = fadeOut;

        // Add or reuse existing light
        glowLight = GetComponent<Light>();
        if (glowLight == null)
        {
            glowLight = gameObject.AddComponent<Light>();
        }

        glowLight.type = LightType.Point;
        glowLight.intensity = intensity;
        glowLight.range = range;
        glowLight.color = Color.white;
        glowLight.shadows = LightShadows.Soft;

        StopAllCoroutines();
        StartCoroutine(FadeOutAfterDelay());
    }

    private System.Collections.IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(glowDuration);

        float startIntensity = glowLight.intensity;
        float elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            glowLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / fadeOutTime);
            yield return null;
        }

        Destroy(glowLight);
    }
}

