using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DynamicText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;

    [Header("Padding Settings")]
    public float paddingTop = 0f;
    public float paddingBottom = 0f;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()     
    {
        ResizeToFitText();
    }

    private void ResizeToFitText()
    {
        if (textComponent == null || rectTransform == null)
            return;

        // Force TMP to update its layout
        textComponent.ForceMeshUpdate();

        float preferredHeight = textComponent.textBounds.size.y;

        // Apply padding and resize height
        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x,
            preferredHeight + paddingTop + paddingBottom
        );
    }
}
