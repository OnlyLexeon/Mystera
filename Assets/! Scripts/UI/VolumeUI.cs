using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class VolumeUI : MonoBehaviour
{
    [System.Serializable]
    public class VolumeUIControl
    {
        public string parameterName;
        public Slider slider;
        public Button increaseButton;
        public Button decreaseButton;

        public void Initialize()
        {
            Debug.Log("Im initialized!");

            float savedVolume = VolumeManager.Instance.GetVolume(parameterName);

            slider.value = savedVolume;

            slider.onValueChanged.AddListener(value => VolumeManager.Instance.ChangeVolume(parameterName, value));
            increaseButton.onClick.AddListener(() => AdjustVolume(0.05f));
            decreaseButton.onClick.AddListener(() => AdjustVolume(-0.05f));
        }

        public void RemoveListeners()
        {
            slider.onValueChanged.RemoveAllListeners();
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
        }

        private void AdjustVolume(float amount)
        {
            slider.value = Mathf.Clamp(slider.value + amount, 0.0001f, 1f);
        }
    }

    public VolumeUIControl[] uiControls;

    private void OnEnable()
    {
        foreach (var control in uiControls)
        {
            control.Initialize();
        }
    }

    private void OnDisable()
    {
        foreach (var control in uiControls)
        {
            control.RemoveListeners();
        }
    }
}
