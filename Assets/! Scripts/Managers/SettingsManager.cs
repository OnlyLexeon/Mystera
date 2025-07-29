using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("CameraOffset Settings")]
    public XROrigin xrOrigin;
    public Slider cameraOffset;
    public TextMeshProUGUI cameraOffsetvalue;
    public Button cameraResetButton;
    public float minValueCamera;
    public float maxValueCamera;
    public float defaultValueCamera;

    [Header("BeltOffset Settings")]
    public HatOffset hat;
    public Slider hatOffset;
    public TextMeshProUGUI hatOffsetvalue;
    public Button hatResetButton;
    public float minValueHat;
    public float maxValueHat;
    public float defaultValueHat;

    private void Awake()
    {
        InitializeCameraSlider();
        InitializeBeltSlider();

        cameraResetButton.onClick.AddListener(ResetCameraOffset);
        hatResetButton.onClick.AddListener(ResetBeltOffset);
    }

    // === CAMERA ===
    public void InitializeCameraSlider()
    {
        cameraOffset.minValue = minValueCamera;
        cameraOffset.maxValue = maxValueCamera;

        float savedValue = PlayerPrefs.GetFloat("CameraOffset", defaultValueCamera);
        cameraOffset.value = savedValue;
        UpdateCameraHeight(savedValue);

        cameraOffset.onValueChanged.AddListener(OnCameraSliderChanged);
    }

    private void OnCameraSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("CameraOffset", value);
        PlayerPrefs.Save();
        UpdateCameraHeight(value);
    }

    private void UpdateCameraHeight(float value)
    {
        cameraOffsetvalue.text = value.ToString("F3");
        xrOrigin.CameraYOffset = value;
    }

    public void ResetCameraOffset()
    {
        cameraOffset.value = defaultValueCamera; // Triggers OnCameraSliderChanged
    }

    // === BELT ===
    public void InitializeBeltSlider()
    {
        hatOffset.minValue = minValueHat;
        hatOffset.maxValue = maxValueHat;

        float savedValue = PlayerPrefs.GetFloat("BeltOffset", defaultValueHat);
        hatOffset.value = savedValue;
        UpdateBeltHeight(savedValue);

        hatOffset.onValueChanged.AddListener(OnBeltSliderChanged);
    }

    private void OnBeltSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("BeltOffset", value);
        PlayerPrefs.Save();
        UpdateBeltHeight(value);
    }

    private void UpdateBeltHeight(float value)
    {
        hatOffsetvalue.text = value.ToString("F3");
        hat.yOffset = value;
    }

    public void ResetBeltOffset()
    {
        hatOffset.value = defaultValueHat; // Triggers OnBeltSliderChanged
    }
}
