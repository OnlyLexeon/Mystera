using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent (typeof(SphereCollider))]
public class Spells : MonoBehaviour
{
    public int spell_damage = 1;
    public float projectile_speed = 30;
    public GameObject hit_effect;

    public float arch_range = 1;
    public float min_arch_time = 0.5f;
    public float max_arch_time = 2.0f;

    public float spell_life_time = 0;
    public float hit_effect_life_time = 1;
    public float maximum_life_time = 10;

    public AudioClip spell_incantation_audio;
    public AudioClip spell_sound_effect;

    protected Rigidbody _rb;
    protected GameObject _hit_effect_obj;
    protected bool _collided;
    protected AudioSource _audio_source;

    private void Start()
    {
        SpellLifeTimeEnd();
        if (spell_sound_effect != null)
        {
            _audio_source = hit_effect.GetComponent<AudioSource>();
            _audio_source.clip = spell_sound_effect;
        }
    }

    public virtual void ShootProjectile(Vector3 forward)
    {
        FireProjectile(forward);
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }

    public virtual void ProjectileHit(GameObject hit_object)
    {
        if (hit_object.tag != "Player" && hit_object.tag != "Projectile" && !_collided && hit_object.tag != "QTE")
        {
            if (hit_object.tag == "Enemy")
            {
                //var enemy_script = hit_object.GetComponent<EnemyScript>();
                //enemy_script.BeenHit(spell_damage);
            }
            else if(hit_object.tag == "NPC")
            {
                //var npc_script = hit_object.GetComponent<NPCAnimScript>();
                //npc_script.GetHit();
            }
            _collided = true;
            _hit_effect_obj = Instantiate(hit_effect, transform.position, transform.rotation);

            _rb.linearVelocity = Vector3.zero;
            Destroy(_hit_effect_obj, hit_effect_life_time);
            Destroy(gameObject, spell_life_time);
        }
    }

    protected void FireProjectile(Vector3 forward)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = forward * projectile_speed;

        //iTween.PunchPosition(gameObject, new Vector3(UnityEngine.Random.Range(-arch_range, arch_range), UnityEngine.Random.Range(-arch_range, arch_range), 0), UnityEngine.Random.Range(min_arch_time, max_arch_time));
    }

    protected virtual void SpellLifeTimeEnd()
    {
        Destroy(gameObject,maximum_life_time);
    }
}
