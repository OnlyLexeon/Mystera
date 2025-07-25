using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

public class BoldemortAvadaScript : AvadaKedavraScript
{
    public float qte_push_strength = 0f;
    public EnemyScript enemy_script;
    protected override void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }
    public override void ProjectileHit(GameObject collision_obj)
    {
        if (collision_obj.tag == "QTE")
        {
            HitQteObject(collision_obj);
        }
        if (collision_obj.tag != "Enemy" && collision_obj.tag != "Projectile" && !_collided && collision_obj.tag != "QTE")
        {
            if (collision_obj.tag == "Player")
            {
                enemy_script = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyScript>();
                enemy_script.Win();
            }
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
    }

    private void Update()
    {
        if (_qte_hit)
        {
            _qte_script.PushProgressor(qte_push_strength);

            transform.position = _qte_object.transform.position;
        }
        if (!SpellWandScript.trigger_qte)
        {
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        _rb.linearVelocity = Vector3.zero;
        Destroy(_hit_effect_obj, hit_effect_life_time);
        Destroy(gameObject.GetNamedChild("Ball"), spell_life_time);
        Destroy(gameObject.GetNamedChild("Trail"), trail_life_time);
        Destroy(start_position_obj.gameObject, trail_life_time);
        Destroy(position_2_obj.gameObject, trail_life_time);
        Destroy(position_3_obj.gameObject, trail_life_time);
        Destroy(gameObject, spell_life_time);
    }

    public override void ShootProjectile(Vector3 forward)
    {
        FireProjectile(forward);
    }
}
