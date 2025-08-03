using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlotAltar : MonoBehaviour
{
    public int slotIndex;
    public Image spellIcon;
    public TextMeshProUGUI spellName;
    public Image spellArrayStructure;

    public Sprite defaultSprite;
    public string defaultSpellName;

    public void SlotSelected()
    {
        SpellsManager.instance._currentSlotIndex = slotIndex;

        AltarTabManager.instance.OpenLearnedSpellsTab();
    }

}
