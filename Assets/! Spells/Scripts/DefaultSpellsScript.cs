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
    protected GameObject hitEffectObj;

    protected bool _collided = false;

    private void Awake()
    {
        _collided = false;
        _audioSource = GetComponent<AudioSource>();

        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.radius = spellData.projectileRadius;
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

    public virtual void ShootProjectile(Transform firePoint)
    {
        FireProjectile(firePoint.forward);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }

    public virtual void ProjectileHit(GameObject hitObject)
    {
        _collided = true;

        if (spellData.spellHitBoxRadius > 0)
        {
            if (!spellData.isAoe)
            {
                if (hitObject.tag == "Enemy")
                {
                    Debug.Log(gameObject.name + " MAINLY HIT Enemy :" + hitObject.gameObject.name);
                    if (hitObject.TryGetComponent(out Health health))
                    {
                        int roundedDamage = Mathf.RoundToInt(spellData.spellMainTargetDamage);
                        health.TakeDamage(roundedDamage, Player.instance.gameObject);
                    }
                }
            }
            Collider[] aoeHitObjects = Physics.OverlapSphere(transform.position, spellData.spellHitBoxRadius);
            foreach (Collider aoeHitObject in aoeHitObjects)
            {
                Debug.Log("AOE HIT:" + aoeHitObject.name);
                if (aoeHitObject.GetInstanceID() != hitObject.GetInstanceID())
                {
                    if (aoeHitObject.tag == "Enemy")
                    {
                        Debug.Log(gameObject.name + " AOE HIT Enemy :" + hitObject.gameObject.name);
                        if (aoeHitObject.TryGetComponent(out Health health))
                        {
                            int roundedDamage = Mathf.RoundToInt(spellData.spellAoeDamage);
                            health.TakeDamage(roundedDamage, Player.instance.gameObject);
                        }
                    }
                }
            }
        }
        else
        {
            if (hitObject.tag == "Enemy")
            {
                Debug.Log(gameObject.name + " HIT Enemy :" + hitObject.gameObject.name);
                if (hitObject.TryGetComponent(out Health health))
                {
                    int roundedDamage = Mathf.RoundToInt(spellData.spellMainTargetDamage);
                    health.TakeDamage(roundedDamage, Player.instance.gameObject);
                }
            }
        }

        _rigidBody.linearVelocity = Vector3.zero;
        hitEffectObj = Instantiate(spellData.hitEffectObject, transform.position, transform.rotation);

        SpellEffectCleanUp();
    }

    public virtual void SpellEffectCleanUp()
    {
        AudioClip hitEffectAudio = spellData.hitEffectAudio;

        if (hitEffectAudio != null)
        {
            AudioSource hitEffectAudioSource;
            if (hitEffectObj.GetComponent<AudioSource>() == null)
            {
                hitEffectAudioSource = hitEffectObj.AddComponent<AudioSource>();
            }
            else
            {
                hitEffectAudioSource = hitEffectObj.GetComponent<AudioSource>();
            }
            hitEffectAudioSource.clip = hitEffectAudio;
            hitEffectAudioSource.Play();

            if (spellData.hitEffectLifeTime < hitEffectAudio.length)
            {
                Destroy(hitEffectObj, hitEffectAudio.length);
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

        AudioClip projectileAudio = spellData.projectileAudio;

        if (projectileAudio != null)
        {
            if (spellData.projectileLifeTime < projectileAudio.length)
            {
                Destroy(gameObject, projectileAudio.length);
            }
            else
            {
                Destroy(gameObject, spellData.projectileLifeTime);
            }
        }
        else
        {
            Destroy(gameObject, spellData.projectileLifeTime);
        }
    }

    private void Update()
    {
        if (_collided)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, spellData.spellHitBoxRadius);
    }
}
