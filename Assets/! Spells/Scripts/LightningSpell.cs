using UnityEngine;

public class LightningSpell : DefaultSpellsScript
{
    public Transform startPoint;

    public override void ShootProjectile(Transform firePoint)
    {
        startPoint.parent = firePoint;
        Destroy(startPoint.gameObject,spellData.maximumLifeTime);
        base.ShootProjectile(firePoint);
    }

    public override void SpellEffectCleanUp()
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
                Destroy(startPoint.gameObject, projectileAudio.length);
            }
            else
            {
                Destroy(gameObject, spellData.projectileLifeTime);
                Destroy(startPoint.gameObject, spellData.projectileLifeTime);
            }
        }
        else
        {
            Destroy(gameObject, spellData.projectileLifeTime);
            Destroy(startPoint.gameObject, spellData.projectileLifeTime);
        }
    }
}
