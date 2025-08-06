using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSpellData", menuName = "Scriptable Objects/DefaultSpellData")]
public class DefaultSpellData : ScriptableObject
{
    [Header("Spells Data")]
    public List<Vector2> spellsArrayStructure = new List<Vector2>();
    public int spellDamge;
    public int spellManaCost;
    public float spellChargeTime;
    public float projectileSpeed;
    public int applyStatus;
    public float statusDuration;
    public float spellHitBoxRadius;
    public LayerMask spellHitBoxMask;
    public LayerMask spellIgnoreMask;

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
