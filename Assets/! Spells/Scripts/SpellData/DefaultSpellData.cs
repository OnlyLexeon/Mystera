using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSpellData", menuName = "Scriptable Objects/DefaultSpellData")]
public class DefaultSpellData : ScriptableObject
{
    [Header("Spells Data")]
    public List<Vector2> spellsArrayStructure = new List<Vector2>();
    public float spellDamge;
    public float spellChargeTime;
    public float projectileSpeed;
    public int applyStatus;
    public float statusDuration;
    public float spellHitBoxRadius;

    [Header("Spells Effect Data")]
    //public GameObject projectileObject;
    public float spellLifeTime;
    public float maximumLifeTime;

    [Header("Hit Effect Data")]
    public GameObject hitEffectObject;
    public float hitEffectLifeTime;

    [Header("Audio")]
    public AudioClip projectileAudio;
    public AudioClip hitEffectAudio;
}
