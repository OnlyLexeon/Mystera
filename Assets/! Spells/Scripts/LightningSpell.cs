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
}
