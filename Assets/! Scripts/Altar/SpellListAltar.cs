using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellListAltar : MonoBehaviour
{
    public int spellIndex;
    public Image spellIcon;
    public TextMeshProUGUI spellName;

    public void SpellSelected()
    {
        SpellsManager.instance.EquipSpell(spellIndex);

        AltarTabManager.instance.OpenSpellsTab();
    }

    public void UnequipSpellPressed()
    {
        SpellsManager.instance.UnequipSpell();

        AltarTabManager.instance.OpenSpellsTab();
    }
}
