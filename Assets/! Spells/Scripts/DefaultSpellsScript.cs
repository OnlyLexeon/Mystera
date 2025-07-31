using UnityEngine;

[RequireComponent (typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class DefaultSpellsScript : MonoBehaviour
{
    public DefaultSpellData spellData;

    protected Rigidbody _rigidBody;
    protected SphereCollider _sphereCollider;
    protected AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        SpellLifeTimeLimit();
    }

    public virtual void ShootProjectile(Vector3 fireDirection)
    {
        FireProjectile(fireDirection);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }

    public virtual void ProjectileHit(GameObject hitObject)
    {
        if(hitObject.tag == "Enemy")
        {
            Debug.Log("HIT!");
        }
    }

    protected void FireProjectile(Vector3 fireDirection)
    {
        _rigidBody = GetComponent<Rigidbody>();
        _sphereCollider = GetComponent<SphereCollider>();
        _rigidBody.linearVelocity = fireDirection * spellData.projectileSpeed;
        _sphereCollider.radius = spellData.spellHitBoxRadius;
    }

    protected virtual void SpellLifeTimeLimit()
    {
        Destroy(gameObject,spellData.maximumLifeTime);
    }
}
