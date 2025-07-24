using Unity.XR.CoreUtils;
using UnityEngine;

public class SlytherinCrucioScript : AvadaKedavraScript
{
    protected override void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }

    public override void ProjectileHit(GameObject collision_obj)
    {
        _collided = true;
        _hit_effect_obj = Instantiate(hit_effect, transform.position, transform.rotation);

        _rb.linearVelocity = Vector3.zero;
        Destroy(_hit_effect_obj, hit_effect_life_time);
        Destroy(gameObject.GetNamedChild("Ball"), spell_life_time);
        Destroy(gameObject.GetNamedChild("Trail"), trail_life_time);
        Destroy(start_position_obj.gameObject, trail_life_time);
        Destroy(position_2_obj.gameObject, trail_life_time);
        Destroy(position_3_obj.gameObject, trail_life_time);
    }

    // Update is called once per frame
    void Update()
    {
        if (!SpellWandScript.trigger_qte)
        {
            Destroy(gameObject);
        }
    }

    public override void ShootProjectile(Vector3 forward)
    {
        FireProjectile(forward);
    }
}
