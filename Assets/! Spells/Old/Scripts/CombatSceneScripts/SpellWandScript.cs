using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[RequireComponent(typeof(AudioSource))]
public class SpellWandScript : MonoBehaviour
{
    public int basic_cast_index = 0;
    public List<IncantationData> incantations = new List<IncantationData>();
    public Transform fire_point;
    public LineRenderer aim_line;

    //public float projectile_speed = 20;
    //public float fire_rate = 4;

    private string _current_voice_input;
    private GameObject _spell_to_cast;
    private bool _spell_found;

    //private Spells _spell_script;
    private GameObject _spell_obj;
    private AudioSource _audio;

    static public bool trigger_qte;
    public bool mic_recording = false;
    public bool qte_ongoing = false;

    //private MicRecorder _mic_recorder;
    public AvadaQTEScript _avada_qte;
    public ToggleParticle _toggle_particle;

    public float qte_push_strength = 0f;

    private GameObject current_spell;
    public EnemyScript enemy_status;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        //if (GetComponent<MicRecorder>() != null)
        //    _mic_recorder = GetComponent<MicRecorder>();
        if (GetComponent<AvadaQTEScript>() != null)
            _avada_qte = GetComponent<AvadaQTEScript>();
        if (GetComponent<ToggleParticle>() != null)
            _toggle_particle = GetComponent<ToggleParticle>();
        aim_line.enabled = false;
        trigger_qte = false;
        mic_recording = false;
        qte_ongoing = false;
    }

    private void Update()
    {
        //if (MicRecorder.voice_input.Count > 0)
        //{
            _spell_found = false;
            //_current_voice_input = MicRecorder.voice_input.Dequeue();
            for (int i = 0; i < incantations.Count && !_spell_found; i++)
            {
                foreach (var incantation in incantations[i].incantations)
                {
                    if (_current_voice_input.Contains(incantation))
                    {
                        _spell_found = true;
                        _spell_to_cast = incantations[i].spell;
                    }
                }
            }
            if (!_spell_found)
            {
                _spell_to_cast = incantations[basic_cast_index].spell;
            }

            current_spell = CastSpell(_spell_to_cast);
        //}
        //if (trigger_qte && !qte_ongoing)
        //{
        //    qte_ongoing = true;
        //    _avada_qte.TriggerQte(fire_point, current_spell);
        //}
    }

    public void ActivateSpellWand()
    {
        if (!trigger_qte)
        {
            if (!mic_recording && !enemy_status.dead)
            {
                ActivateAim();
                //_mic_recorder.ActivateRecording();
                mic_recording = true;
            }
            _toggle_particle.Play();
        }
        else
        {
            _avada_qte.PushProgressor(-qte_push_strength);
        }
    }

    public void DeactivateSpellWand()
    {
        if (!trigger_qte)
        {
            if (mic_recording && !enemy_status.dead)
            {
                //_mic_recorder.FinishRecording();
                DeactivateAim();
                mic_recording = false;
            }
            _toggle_particle.Stop();
        }
        else
        {
            _avada_qte.PushProgressor(-qte_push_strength);
        }
    }

    private GameObject CastSpell(GameObject spell)
    {
        _spell_obj = Instantiate(spell, fire_point.position, fire_point.rotation);
        _spell_obj.GetComponent<Spells>().ShootProjectile(fire_point.forward);
        _audio.clip = _spell_obj.GetComponent<Spells>().spell_incantation_audio;
        _audio.Play();

        return _spell_obj;
    }

    public void ActivateAim()
    {
        aim_line.enabled = true;
    }

    public void DeactivateAim()
    {
        aim_line.enabled = false;
    }
}
