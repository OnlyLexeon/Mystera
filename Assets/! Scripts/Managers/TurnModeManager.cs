using TMPro;
using UnityEngine;
using UnityEngine.UI; // Use TMPro if you're using TMP_Dropdown
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class TurnModeManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown turnModeDropdown; // Replace with TMP_Dropdown if needed

    [Header("XR Turn Providers (on another GameObject)")]
    public SnapTurnProvider snapTurnProvider;
    public ContinuousTurnProvider continuousTurnProvider;

    private const string TurnModeKey = "TurnMode"; // PlayerPrefs key

    private enum TurnMode { Snap, Continuous }

    void Start()
    {
        LoadTurnMode();
        turnModeDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    public void OnDropdownChanged(int index)
    {
        SetTurnMode((TurnMode)index);
        SaveTurnMode(index);
    }

    private void SetTurnMode(TurnMode mode)
    {
        switch (mode)
        {
            case TurnMode.Snap:
                if (snapTurnProvider != null) snapTurnProvider.enabled = true;
                if (continuousTurnProvider != null) continuousTurnProvider.enabled = false;
                break;
            case TurnMode.Continuous:
                if (snapTurnProvider != null) snapTurnProvider.enabled = false;
                if (continuousTurnProvider != null) continuousTurnProvider.enabled = true;
                break;
        }
    }

    private void SaveTurnMode(int index)
    {
        PlayerPrefs.SetInt(TurnModeKey, index);
        PlayerPrefs.Save();
    }

    private void LoadTurnMode()
    {
        int savedIndex = PlayerPrefs.GetInt(TurnModeKey, (int)TurnMode.Snap);
        turnModeDropdown.value = savedIndex;
        SetTurnMode((TurnMode)savedIndex);
    }
}
