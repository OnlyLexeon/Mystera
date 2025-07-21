using UnityEngine;

public class FlamableObject : MonoBehaviour
{
    [Header("Fire by Collision Settings")]
    public bool flammableByCollision = true;
    public float flammableTime = 1.5f;
    public float flammableTimer = 0f;

    [Header("Fire Settings")]
    public bool isOnFire = false;
    public float health = 5f;
    public bool isBurned = false;

    [Header("Effects")]
    public GameObject particleFire; // fire visual prefab
    public float particleScale = 1f;

    [Header("Audio")]
    public AudioClip burnedSound;
    public AudioClip onFireSound;

    private GameObject spawnedFire;
    private AudioSource audioSource;
    

    public void SetOnFire()
    {
        isOnFire = true;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isOnFire)
        {
            // Spawn fire particle if not already spawned
            if (spawnedFire == null && particleFire != null)
            {
                spawnedFire = Instantiate(particleFire, transform);
                spawnedFire.transform.localScale = Vector3.one * particleScale;

                audioSource.clip = onFireSound;
                audioSource.Play();
            }

            // Burn health over time
            if (!isBurned)
            {
                health -= Time.deltaTime;
                if (health <= 0f)
                {
                    isBurned = true;
                    BurnAndDestroy();
                }
            }
            
        }
    }

    private void BurnAndDestroy()
    {
        if (burnedSound != null)
        {
            audioSource.PlayOneShot(burnedSound);
        }

        // Optional: Delay destroy to allow sound to play fully
        Destroy(gameObject, burnedSound != null ? burnedSound.length : 0f);
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision != null && collision.gameObject.tag == "Fire")
        {
            if (flammableByCollision && !isOnFire)
            {
                if (flammableTimer <= flammableTime)
                {
                    flammableTimer += Time.deltaTime;
                }

                if (flammableTimer >= flammableTime)
                {
                    isOnFire = true;
                }
            }
        }
    }
}
