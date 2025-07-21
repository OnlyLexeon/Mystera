using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DayNightAmbience : MonoBehaviour
{
    [Header("Audio Sources Outside Windows")]
    public List<AudioSource> ambienceSources;

    [Header("Day Ambience")]
    public AudioClip dayClip;
    public float dayMinVolume = 0.5f;
    public float dayMaxVolume = 1f;
    public float dayMinPitch = 0.95f;
    public float dayMaxPitch = 1.05f;

    [Header("Night Ambience")]
    public AudioClip nightClip;
    public float nightMinVolume = 0.3f;
    public float nightMaxVolume = 0.8f;
    public float nightMinPitch = 0.9f;
    public float nightMaxPitch = 1.1f;

    [Header("Volume Update Interval")]
    public float modifyVolumeInterval = 5f;

    private Coroutine ambienceRoutine = null;
    private enum AmbienceType { None, Day, Night }
    private AmbienceType currentAmbience;

    private void Start()
    {
        currentAmbience = AmbienceType.None;
    }

    public void PlayDayAmbience()
    {
        Debug.Log("Playing day ambience");
        if (currentAmbience == AmbienceType.Day) return;
        currentAmbience = AmbienceType.Day;
        StartAmbience(dayClip, dayMinVolume, dayMaxVolume, dayMinPitch, dayMaxPitch);
    }

    public void PlayNightAmbience()
    {
        Debug.Log("Playing night ambience");
        if (currentAmbience == AmbienceType.Night) return;
        currentAmbience = AmbienceType.Night;
        StartAmbience(nightClip, nightMinVolume, nightMaxVolume, nightMinPitch, nightMaxPitch);
    }

    private void StartAmbience(AudioClip clip, float minVol, float maxVol, float minPitch, float maxPitch)
    {
        if (ambienceRoutine != null)
            StopCoroutine(ambienceRoutine);

        // Start ambience on all sources with staggered delays
        foreach (AudioSource src in ambienceSources)
        {
            StartCoroutine(StartWithDelay(src, clip));
        }

        ambienceRoutine = StartCoroutine(ModifyAmbience(minVol, maxVol, minPitch, maxPitch));
    }

    private IEnumerator StartWithDelay(AudioSource src, AudioClip clip)
    {
        float delay = Random.Range(0f, 0.3f);
        yield return new WaitForSeconds(delay);

        src.clip = clip;
        src.loop = true;
        src.Play();
    }

    private IEnumerator ModifyAmbience(float minVol, float maxVol, float minPitch, float maxPitch)
    {
        while (true)
        {
            foreach (AudioSource src in ambienceSources)
            {
                src.volume = Random.Range(minVol, maxVol);
                src.pitch = Random.Range(minPitch, maxPitch);
            }
            yield return new WaitForSeconds(modifyVolumeInterval);
        }
    }
}
