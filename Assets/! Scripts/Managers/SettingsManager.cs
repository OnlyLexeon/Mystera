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
    public Belt belt;
    public Slider beltOffset;
    public TextMeshProUGUI beltOffsetvalue;
    public Button beltResetButton;
    public float minValueBelt;
    public float maxValueBelt;
    public float defaultValueBelt;

    private void Awake()
    {
        InitializeCameraSlider();
        InitializeBeltSlider();

        cameraResetButton.onClick.AddListener(ResetCameraOffset);
        beltResetButton.onClick.AddListener(ResetBeltOffset);
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
        beltOffset.minValue = minValueBelt;
        beltOffset.maxValue = maxValueBelt;

        float savedValue = PlayerPrefs.GetFloat("BeltOffset", defaultValueBelt);
        beltOffset.value = savedValue;
        UpdateBeltHeight(savedValue);

        beltOffset.onValueChanged.AddListener(OnBeltSliderChanged);
    }

    private void OnBeltSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("BeltOffset", value);
        PlayerPrefs.Save();
        UpdateBeltHeight(value);
    }

    private void UpdateBeltHeight(float value)
    {
        beltOffsetvalue.text = value.ToString("F3");
        belt.yOffset = value;
    }

    public void ResetBeltOffset()
    {
        beltOffset.value = defaultValueBelt; // Triggers OnBeltSliderChanged
    }
}
