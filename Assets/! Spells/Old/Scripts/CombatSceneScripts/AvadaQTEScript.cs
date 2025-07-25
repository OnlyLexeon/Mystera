using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class AvadaQTEScript : MonoBehaviour
{
    public BoldemortSpellScript boldemort_spell;
    //public Transform qte_point;
    public GameObject qte_hit_effect;
    public GameObject overdrive_effect;
    public GameObject player_obj;

    //public Transform qte_hit_position;

    //public GameObject boldemort_avada;

    //public float max_qte_percentage;
    public float current_qte_percentage = 0.5f;

    private Transform _player_fire_point;
    public bool triggered = false;
    public GameObject player_avada_obj;
    public GameObject boldemort_avada_obj;

    public Slider progress_bar;
    public TextMeshProUGUI text_obj;
    public float text_life_time = 5f;

    private SphereCollider _collder;

    public HapticImpulsePlayer controller;
    public float impulse_force = 0.7f;
    public float impulse_duration = 0.1f;

    private void Start()
    {
        _collder = GetComponent<SphereCollider>();
        triggered = false;
        gameObject.SetActive(false);
        if (overdrive_effect != null)
            overdrive_effect.SetActive(false);
        if (qte_hit_effect != null)
            qte_hit_effect.SetActive(false);
    }

    private void Update()
    {
        if (triggered && current_qte_percentage > 0 && current_qte_percentage < 1)
        {
            controller.SendHapticImpulse(impulse_force, impulse_duration);
            transform.position = Vector3.Lerp(boldemort_spell.transform.position + new Vector3(0, 2, 0), _player_fire_point.position, current_qte_percentage);
            progress_bar.value = 1 - current_qte_percentage;
        }
    }

    public void TriggerQte(Transform player_fire_point, GameObject _spell_to_cast)
    {
        gameObject.SetActive(true);
        player_avada_obj = _spell_to_cast;
        triggered = true;
        _player_fire_point = player_fire_point;
        transform.position = Vector3.Lerp(boldemort_spell.fire_point.position, _player_fire_point.position, current_qte_percentage);

        Instantiate(qte_hit_effect, transform);
        qte_hit_effect.SetActive(true);
        overdrive_effect.SetActive(true);

        boldemort_avada_obj = boldemort_spell.CastSpell();
        Destroy(text_obj, text_life_time);
    }

    public void QTEFinish(float new_qte_percentage)
    {
        triggered = false;
        _collder.enabled = false;
        current_qte_percentage = new_qte_percentage;
        SpellWandScript.trigger_qte = false;
        Destroy(qte_hit_effect);
        Destroy(overdrive_effect);
        if (current_qte_percentage >= 1)
        {
            Destroy(player_avada_obj);
            boldemort_avada_obj.GetComponent<BoldemortAvadaScript>().ProjectileHit(player_obj);
        }
        else if (current_qte_percentage <= 0)
        {
            Destroy(boldemort_avada_obj);
            player_avada_obj.GetComponent<AvadaKedavraScript>().ProjectileHit(boldemort_spell.gameObject);
        }
        Destroy(gameObject);
    }

    public void PushProgressor(float progress)
    {
        if (current_qte_percentage > 0 && current_qte_percentage < 1)
            current_qte_percentage += progress;
        if (current_qte_percentage <= 0 || current_qte_percentage >= 1)
            QTEFinish(current_qte_percentage);
    }
}
