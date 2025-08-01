using UnityEngine;

[CreateAssetMenu(fileName = "SpellObject", menuName = "Scriptable Objects/SpellObject")]
public class SpellObject : ScriptableObject
{
    [Header("Spell Information")]
    public string spellID;
    public Sprite spellIconSprite;
    public Sprite spellArrayStructure;
    [TextArea] public string spellDescrtipion;
    public bool defaultLearned;

    [Header("Spell Prefab")]
    public GameObject spellPrefab;

    [Header("Spell Data")]
    public DefaultSpellData spellData;
}
