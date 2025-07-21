using UnityEngine;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;

    [System.Serializable]
    public class VolumeControl
    {
        public string parameterName;
        public AudioMixer mixer;
        private float savedVolume = 1;

        public void LoadVolume()
        {
            savedVolume = PlayerPrefs.GetFloat(parameterName, 1);
            bool isMuted = PlayerPrefs.GetInt(parameterName + "_Mute", 0) == 1;

            if (isMuted)
                mixer.SetFloat(parameterName, -80f);
            else
                mixer.SetFloat(parameterName, Mathf.Log10(savedVolume) * 20);
        }

        public void SetVolume(float value)
        {
            savedVolume = Mathf.Clamp(value, 0.0001f, 1f); // prevent Log10(0)
            mixer.SetFloat(parameterName, Mathf.Log10(savedVolume) * 20);
            PlayerPrefs.SetFloat(parameterName, savedVolume);
            PlayerPrefs.Save();
        }

        public float GetSavedVolume() => savedVolume;
        public bool IsMuted() => PlayerPrefs.GetInt(parameterName + "_Mute", 0) == 1;
    }

    public VolumeControl[] volumeControls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        foreach (var control in volumeControls) control.LoadVolume();
    }

    public void ChangeVolume(string parameter, float value)
    {
        foreach (var control in volumeControls)
        {
            if (control.parameterName == parameter)
            {
                control.SetVolume(value);
                break;
            }
        }
    }

    public float GetVolume(string parameter)
    {
        foreach (var control in volumeControls)
            if (control.parameterName == parameter) return control.GetSavedVolume();
        return 0.75f;
    }
}
