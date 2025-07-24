using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoldemortSpellScript : MonoBehaviour
{
    public Transform fire_point;
    public GameObject boldemort_avada;

    //private Spells _spell_script;
    private GameObject _spell_obj;
    private AudioSource _audio;
    private BoldemortAvadaScript _spell_obj_script;

    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
    }
    void Update()
    {
    }

    public GameObject CastSpell()
    {
        _animator.SetTrigger("Attack");
        _spell_obj = Instantiate(boldemort_avada, fire_point.position, fire_point.rotation);
        _spell_obj_script = _spell_obj.GetComponent<BoldemortAvadaScript>();
        _spell_obj_script.ShootProjectile(fire_point.forward);
        _audio.clip = _spell_obj_script.spell_incantation_audio;
        _audio.Play();
        return _spell_obj;
    }
}
