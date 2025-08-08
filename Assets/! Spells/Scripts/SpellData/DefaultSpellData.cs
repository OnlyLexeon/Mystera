using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSpellData", menuName = "Scriptable Objects/DefaultSpellData")]
public class DefaultSpellData : ScriptableObject
{
    [Header("Spells Data")]
    public List<Vector2> spellsArrayStructure = new List<Vector2>();
    public int spellMainTargetDamage;
    public int spellAoeDamage;
    public int spellManaCost;
    public float spellChargeTime;
    public float projectileSpeed;
    public int applyStatus;
    public float statusDuration;
    [Tooltip("Radius of the projectile")]
    public float projectileRadius;
    [Tooltip("Damage will be same for all enemy in the AOE using spellAoeDamage if true, else deals spellMainTargetDamage to the target that got hit and splash spellAoeDamage within the AOE")]
    public bool isAoe;
    [Tooltip("If is just single target, set to 0 to make it has no splash damage")]
    public float spellHitBoxRadius;

    [Header("Spell Collider Mask")]
    public LayerMask spellHitBoxMask;
    public LayerMask spellIgnoreMask;

    [Header("Spells Effect Data")]
    //public GameObject projectileObject;
    public float projectileLifeTime;
    public float maximumLifeTime;

    [Header("Hit Effect Data")]
    public GameObject hitEffectObject;
    public float hitEffectLifeTime;

    [Header("Audio")]
    public AudioClip projectileAudio;
    public AudioClip hitEffectAudio;
}
