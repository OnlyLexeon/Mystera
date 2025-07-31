using UnityEngine;

[CreateAssetMenu(fileName = "SpellObject", menuName = "Scriptable Objects/SpellObject")]
public class SpellObject : ScriptableObject
{
    [Header("Spell Information")]
    public string spellID;
    public Sprite spellSprite;
    [TextArea] public string spellDescrtipion;

    [Header("Spell Prefab")]
    public GameObject spellPrefab;

    [Header("Spell Data")]
    public DefaultSpellData spellData;
}
