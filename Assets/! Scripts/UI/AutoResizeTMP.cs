using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AutoResizeTMP : MonoBehaviour
{
    [Tooltip("Maximum width allowed before text wraps to the next line.")]
    public float maxWidth = 500f;

    private TextMeshProUGUI tmp;
    private RectTransform rectTransform;
    private string lastText = "";

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (tmp.text != lastText)
        {
            lastText = tmp.text;
            ResizeToFit();
        }
    }

    private void ResizeToFit()
    {
        tmp.ForceMeshUpdate();

        // Set a max width constraint for preferred values calculation
        Vector2 preferredValues = tmp.GetPreferredValues(maxWidth, Mathf.Infinity);

        // Clamp width to maxWidth
        float finalWidth = Mathf.Min(preferredValues.x, maxWidth);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredValues.y);
    }
}
