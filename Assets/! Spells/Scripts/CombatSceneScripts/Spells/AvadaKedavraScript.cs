using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.VFX;

public class AvadaKedavraScript : Spells
{
    public float trail_life_time = 5f;
    public bool going_to_trigger_qte = false;

    public string start_position_obj_name;
    public Transform start_position_obj;

    public string position_2_obj_name;
    public Transform position_2_obj;

    public string position_3_obj_name;
    public Transform position_3_obj;

    public VisualEffect electric_arc;

    private GameObject[] fire_points;
    private Transform start_position_fire_point;
    private Transform pos_2_fire_point;
    private Transform pos_3_fire_point;

    //protected AvadaQTEScript _qte_script;

    protected GameObject _qte_object;

    protected bool _qte_hit = false;
    public float sphere_cast_radius = 0.5f;

    private void Start()
    {
        //_qte_script = null;
        _qte_hit = false;
        if (spell_sound_effect != null)
        {
            _audio_source = hit_effect.GetComponent<AudioSource>();
            _audio_source.clip = spell_sound_effect;
        }

        //sphere_cast_radius = GetComponent<SphereCollider>().radius;
        fire_points = GameObject.FindGameObjectsWithTag("Firepoints");

        for (int i = 0; i < fire_points.Length; i++)
        {
            if (fire_points[i].name == start_position_obj_name)
            {
                start_position_fire_point = fire_points[i].transform;
            }
            if (fire_points[i].name == position_2_obj_name)
            {
                pos_2_fire_point = fire_points[i].transform;
            }
            if (fire_points[i].name == position_3_obj_name)
            {
                pos_3_fire_point = fire_points[i].transform;
            }
        }

        if (going_to_trigger_qte)
        {
            start_position_obj.SetParent(start_position_fire_point.transform);
            position_2_obj.SetParent(start_position_fire_point.transform);
            position_3_obj.SetParent(start_position_fire_point.transform);
        }
        else
        {
            start_position_obj.SetParent(start_position_obj.parent.parent);
            position_2_obj.SetParent(position_2_obj.parent.parent);
            position_3_obj.SetParent(position_3_obj.parent.parent);
        }
        start_position_obj.position = start_position_fire_point.transform.position;
        position_2_obj.position = pos_2_fire_point.transform.position;
        position_3_obj.position = pos_3_fire_point.transform.position;
    }

    private void Update()
    {
        //if (!SpellWandScript.trigger_qte)
        //{
        //    _qte_hit = false;
        //}
        //if (_qte_hit)
        //{
        //    transform.position = _qte_object.transform.position;
        //}
    }
    public override void ShootProjectile(Vector3 forward)
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphere_cast_radius, forward * projectile_speed * maximum_life_time, out hit))
        //if (Physics.Raycast(transform.position, forward * projectile_speed * maximum_life_time, out hit))
        {
            if (hit.collider.gameObject.tag == "Enemy")
            {
                //if (!hit.collider.gameObject.GetComponent<EnemyScript>().defeated)
                //    going_to_trigger_qte = true;
                //else
                //    going_to_trigger_qte = false;
            }
            else
                going_to_trigger_qte = false;
        }
        if (going_to_trigger_qte)
        {
            electric_arc.SetFloat("LifeTime", float.PositiveInfinity);
            //Debug.Log("Trigger QTE");
            //SpellWandScript.trigger_qte = true;
        }
        else
        {
            SpellLifeTimeEnd();
            electric_arc.SetFloat("LifeTime", trail_life_time);
        }
        FireProjectile(forward);
    }
    protected override void OnCollisionEnter(Collision collision)
    {
        ProjectileHit(collision.gameObject);
    }

    public override void ProjectileHit(GameObject hit_object)
    {
        if (hit_object.tag == "QTE")
        {
            HitQteObject(hit_object);
            //_qte_script.PushProgressor(qte_push_strength);
        }
        if (hit_object.tag != "Player" && hit_object.tag != "Projectile" && !_collided && hit_object.tag != "QTE")
        {
            if (hit_object.tag == "Enemy")
            {
                //var enemy_script = hit_object.GetComponent<EnemyScript>();
                //enemy_script.Dead();
            }
            else if (hit_object.tag == "NPC")
            {
                //var npc_script = hit_object.GetComponent<NPCAnimScript>();
                //npc_script.GetKilled();
            }
            _collided = true;
            _hit_effect_obj = Instantiate(hit_effect, transform.position, transform.rotation);

            _rb.linearVelocity = Vector3.zero;
            electric_arc.SetFloat("LifeTime", trail_life_time);
            electric_arc.Play();
            //SpellWandScript.trigger_qte = false;
            Destroy(gameObject);
        }
    }
    protected void HitQteObject(GameObject collision_obj)
    {
        _rb.linearVelocity = Vector3.zero;
        _qte_object = collision_obj;
        //_qte_script = collision_obj.GetComponent<AvadaQTEScript>();
        _qte_hit = true;
    }

    protected override void SpellLifeTimeEnd()
    {
        base.SpellLifeTimeEnd();
        if (start_position_obj.parent == null)
            Destroy(start_position_obj.gameObject, maximum_life_time);
        if (position_2_obj.parent == null)
            Destroy(position_2_obj.gameObject, maximum_life_time);
        if (position_3_obj.parent == null)
            Destroy(position_3_obj.gameObject, maximum_life_time);
    }

    private void OnDestroy()
    {
        Destroy(_hit_effect_obj, hit_effect_life_time);
        Destroy(gameObject.GetNamedChild("Ball"), spell_life_time);
        Destroy(gameObject.GetNamedChild("ElectricArc_vfx"), trail_life_time);
        Destroy(start_position_obj.gameObject, trail_life_time);
        Destroy(position_2_obj.gameObject, trail_life_time);
        Destroy(position_3_obj.gameObject, trail_life_time);
        Destroy(gameObject, spell_life_time);
    }
}
