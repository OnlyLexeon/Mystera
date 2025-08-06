using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveEffectsSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI timeLeft;
    public TextMeshProUGUI effectName;

    public void SetSlot(Sprite _icon, float _timeLeft, string _effectName)
    {
        icon.sprite = _icon;
        effectName.text = _effectName;

        int minutes = Mathf.FloorToInt(_timeLeft / 60f);
        int seconds = Mathf.FloorToInt(_timeLeft % 60f);
        timeLeft.text = $"{minutes:00}:{seconds:00}";
    }
}
