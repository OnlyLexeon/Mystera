using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager instance;

    [Header("Lighting Settings")]
    public Light sunLight;
    public Material skyboxMaterial;
    public float intensity = 15f;

    [Header("Time Settings")]
    public float time = 0; 
    public float timeScale = 1f;

    [Header("Day/Night")]
    public bool isDay;
    public bool isNight;

    [Header("Duration")]
    public float dayLength = 500f;
    public float nightLength = 150f;

    [Header("Ambience")]
    public DayNightAmbience ambience;

    private float fullCycle => dayLength + nightLength;
    private bool previousIsDay;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void SetTimeMorning()
    {
        SetTime(nightLength);
    }   
    
    public void SetTime(float value)
    {
        time = value;
    }

    void Update()
    {
        time += Time.deltaTime * timeScale;
        if (time >= fullCycle)
        {
            time = 0f;
        }

        //Set day/night bools
        isDay = time < dayLength;
        isNight = !isDay;

        // Detect day/night change
        if (isDay != previousIsDay && ambience != null)
        {
            if (isDay)
            {
                ambience.PlayDayAmbience();
                Bed.instance.DisableSleepButton();
            }
            else
            {
                ambience.PlayNightAmbience();
                Bed.instance.EnableSleepButton();
            }
            previousIsDay = isDay;
        }

        UpdateLighting();
        UpdateSkybox();
    }

    void UpdateLighting()
    {
        float sunAngle;
        float normalizedTime;

        if (isDay)
        {
            normalizedTime = time / dayLength;
            sunAngle = Mathf.Lerp(0f, 180f, normalizedTime); // Sunrise (0°) to Sunset (180°)
        }
        else
        {
            normalizedTime = (time - dayLength) / nightLength;
            sunAngle = Mathf.Lerp(180f, 360f, normalizedTime); // Sunset to next sunrise
        }

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 80f, 0f);

        // Adjust light intensity with a smooth day/night curve
        float lightFactor = Mathf.Clamp01(Mathf.Sin(normalizedTime * Mathf.PI));
        float baseIntensity = isDay ? 1f : 0.25f; // Night light can still be dim
        sunLight.intensity = lightFactor * baseIntensity * intensity;

        RenderSettings.ambientIntensity = sunLight.intensity * 0.6f;
    }

    void UpdateSkybox()
    {
        if (skyboxMaterial == null) return;

        float normalizedTime = isDay ? time / dayLength : (time - dayLength) / nightLength;
        float sunFactor = Mathf.Sin(normalizedTime * Mathf.PI);

        // Skybox adjustments
        float atmosphereThickness = Mathf.Lerp(0.3f, 1.2f, sunFactor);
        float exposure = Mathf.Lerp(0.25f, 1.25f, sunFactor);

        skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);
        skyboxMaterial.SetFloat("_Exposure", exposure);
    }
}
