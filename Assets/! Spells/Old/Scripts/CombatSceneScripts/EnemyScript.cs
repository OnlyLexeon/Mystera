using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    public AvadaQTEScript qte;

    public Slider health_bar;
    public int health_points;

    public int current_health;
    public Animator animator;

    //public SceneLoaderScript scene_loader;

    public bool defeated = false;
    public bool dead = false;
    public TextMeshProUGUI enemy_info;
    public string defeated_text = "FINISH HIM!";
    public Animator harry_animator;
    public GameObject finish_text;
    public GameObject incantation_text;
    public GameObject npc_team;
    public List<NPCAnimScript> npc_list = new List<NPCAnimScript>();

    public AudioClip hurt_audio;
    public AudioClip dead_audio;
    public AudioClip win_audio;
    public AudioClip win_bgm;
    public AudioSource bgm_obj;

    private AudioSource _audio;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        incantation_text.SetActive(true);
        finish_text.SetActive(false);
        _audio = GetComponent<AudioSource>();
        dead = false;
        defeated = false;
        health_bar.enabled = true;
        health_bar.maxValue = health_points;
        health_bar.value = health_points;
        current_health = health_points;
        animator.SetBool("Defeated", defeated);
    }

    public void BeenHit(int damage)
    {
        if (!dead)
        {
            _audio.clip = hurt_audio;
            _audio.volume = 0.15f;
            _audio.Play();
            animator.SetTrigger("Hit");
            current_health -= damage;
            if (current_health <= 0)
            {
                Defeated();
            }
            health_bar.value = current_health;
        }
    }

    public void Dead()
    {
        Defeated();
        if (qte.triggered && qte.current_qte_percentage > 0)
        {
            qte.QTEFinish(0);
        }
        enemy_info.text = "";
        health_bar.value = current_health;
        _audio.clip = dead_audio;
        _audio.volume = 0.5f;
        _audio.Play();
        bgm_obj.clip = win_bgm;
        bgm_obj.Play();
        harry_animator.SetTrigger("Win");
        for (int i = 0; i < npc_list.Count; i++)
        {
            npc_list[i].Retreat();
        }
        finish_text.SetActive(true);
        incantation_text.SetActive(false);
        dead = true;
        animator.SetTrigger("Dead");
    }

    private void Defeated()
    {
        enemy_info.text = defeated_text;
        current_health = 0;
        defeated = true;
        animator.SetBool("Defeated", defeated);
    }

    public void Win()
    {
        _audio.clip = win_audio;
        _audio.volume = 0.5f;
        _audio.Play();
        //scene_loader.StartReloadScene();
    }
}
