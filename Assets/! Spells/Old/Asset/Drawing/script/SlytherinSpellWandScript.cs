using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SlytherinSpellWandScript : MonoBehaviour
{
    public Transform fire_point;

    private GameObject _spell_obj;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
    
    }

    public GameObject CastSpell(GameObject spell)
    {
        _spell_obj = Instantiate(spell, fire_point.position, fire_point.rotation);
        _spell_obj.GetComponent<Spells>().ShootProjectile(fire_point.forward);
        _audio.clip = _spell_obj.GetComponent<Spells>().spell_incantation_audio;
        _audio.Play();

        return _spell_obj;
    }
}
