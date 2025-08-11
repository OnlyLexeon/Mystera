using UnityEngine;

public class DropletSound : MonoBehaviour
{
    public ParticleSystem ps;        // Assign your particle system
    public AudioSource source;       // Assign an AudioSource in the Inspector
    public AudioClip drip;           // Assign your droplet sound
    public float volume = 0.3f;      // Set default volume in Inspector
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    void OnParticleCollision(GameObject other)
    {
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(drip, volume);
    }
}
