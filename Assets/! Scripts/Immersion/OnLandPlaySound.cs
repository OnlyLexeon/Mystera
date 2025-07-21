using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OnVelocity))]
[RequireComponent(typeof(AudioSource))]
public class OnLandPlaySound : MonoBehaviour
{
    public bool isVelocity = false;

    [Tooltip("Layers considered as 'ground'.")]
    public LayerMask groundLayers;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] clips;

    [Header("Sound Variation")]
    [Tooltip("Random pitch range (min/max) for land sound.")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Cooldown Settings")]
    [Tooltip("Cooldown time between sounds in seconds.")]
    public float playCooldown = 0.5f;

    private float lastSoundTime = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetVelocity(bool state)
    {
        isVelocity = state;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!this.enabled) return;

        // Check if collision was with a ground layer
        if (IsInLayerMask(collision.gameObject, groundLayers))
        {   
            if (isVelocity && Time.time >= lastSoundTime + playCooldown)
            {
                PlaySound();
                lastSoundTime = Time.time;
            }
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }


    public void PlaySound()
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;

        Debug.Log("Sound played");

        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
        float randomPitch = Random.Range(pitchRange.x, pitchRange.y);

        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(clipToPlay);

        audioSource.pitch = 1f;
    }
}
