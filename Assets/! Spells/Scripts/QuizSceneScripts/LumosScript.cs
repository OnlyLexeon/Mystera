using UnityEngine;

[RequireComponent(typeof(ToggleParticle))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
public class LumosScript : MonoBehaviour
{
    public AudioClip lumos_on;
    public AudioClip lumos_off;
    public bool lumos_toggle;
    public float light_snuffed_out_speed = 0.1f;
    public Light lumos_light;
    public ToggleParticle toggle_beam;
    public float max_light_radius = 5f;
    public float current_light_radius;

    private AudioSource _audio;
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        toggle_beam = GetComponent<ToggleParticle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lumos_toggle && current_light_radius < max_light_radius)
        {
            current_light_radius += light_snuffed_out_speed;
        }
        else if (!lumos_toggle && current_light_radius > 0)
        {
            current_light_radius -= light_snuffed_out_speed;
        }
        if (current_light_radius != 0)
            lumos_light.range = current_light_radius;
    }

    public void ActivateLumos()
    {
        lumos_toggle = true;
        toggle_beam.Play();
        _audio.clip = lumos_on;
        _audio.Play();
    }

    public void DeactiveLumos()
    {
        lumos_toggle = false;
        toggle_beam.Stop();
        _audio.clip = lumos_off;
        _audio.Play();
    }
}
