using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReferenceSpellItem : MonoBehaviour
{
    public Image spellArrayRef;
    public TextMeshProUGUI spellName;

    public void SetSpellRef(Sprite spellArrayStructure,string name)
    {
        spellArrayRef.sprite = spellArrayStructure;
        spellName.text = name;
    }
}
