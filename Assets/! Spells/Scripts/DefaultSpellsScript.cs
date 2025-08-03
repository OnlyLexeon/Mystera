using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class DefaultSpellsScript : MonoBehaviour
{
    public DefaultSpellData spellData;

    protected Rigidbody _rigidBody;
    protected SphereCollider _sphereCollider;
    protected AudioSource _audioSource;

    protected bool _collided = false;

    private void Awake()
    {
        _collided = false;
        _audioSource = GetComponent<AudioSource>();

        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.radius = spellData.spellHitBoxRadius;
        _sphereCollider.includeLayers = spellData.spellHitBoxMask;
        _sphereCollider.excludeLayers = spellData.spellIgnoreMask;
    }
    private void Start()
    {
        SpellLifeTimeLimit();
        if (spellData.projectileAudio != null)
        {
            _audioSource.clip = spellData.projectileAudio;
            _audioSource.Play();
        }
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
        _collided = true;
        if (hitObject.tag == "Enemy")
        {
            Debug.Log("HIT! :" + hitObject.gameObject.name);
            Enemy enemyScript = hitObject.GetComponent<Enemy>();
            GameObject attacker = GameObject.FindGameObjectWithTag("Player");
            enemyScript.OnTakeDamageWithAttacker(spellData.spellDamge,attacker);
        }

        _rigidBody.linearVelocity = Vector3.zero;
        var hitEffectObj = Instantiate(spellData.hitEffectObject, transform.position, transform.rotation);

        if (spellData.hitEffectAudio != null)
        {
            AudioSource hitEffectAudio;
            if (hitEffectObj.GetComponent<AudioSource>() == null)
            {
                hitEffectAudio = hitEffectObj.AddComponent<AudioSource>();
            }
            else
            {
                hitEffectAudio = hitEffectObj.GetComponent<AudioSource>();
            }
            hitEffectAudio.clip = spellData.hitEffectAudio;
            hitEffectAudio.Play();

            if (spellData.hitEffectLifeTime < spellData.hitEffectAudio.length)
            {
                Destroy(hitEffectObj, spellData.hitEffectAudio.length);
            }
            else
            {
                Destroy(hitEffectObj, spellData.hitEffectLifeTime);
            }
        }
        else
        {
            Destroy(hitEffectObj, spellData.hitEffectLifeTime);
        }

        Destroy(gameObject, spellData.spellLifeTime);
    }

    private void Update()
    {
        if(_collided)
        {
            _sphereCollider.enabled = false;
        }
    }

    protected void FireProjectile(Vector3 fireDirection)
    {
        _rigidBody.linearVelocity = fireDirection * spellData.projectileSpeed;
    }

    protected virtual void SpellLifeTimeLimit()
    {
        Destroy(gameObject, spellData.maximumLifeTime);
    }
}
