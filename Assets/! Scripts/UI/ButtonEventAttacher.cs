using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonEventAttacher : MonoBehaviour
{
    [Header("Optional: Assign a specific root or leave empty to use this GameObject")]
    public GameObject root;

    [Header("Custom Event")]
    public UnityEvent onButtonClicked;

    private void Start()
    {
        if (root == null)
        {
            root = gameObject;
        }

        AddListenersToButtons(root);
    }

    private void AddListenersToButtons(GameObject obj)
    {
        // Get all buttons in this object and its children
        Button[] buttons = obj.GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnAnyButtonClicked(btn));
        }
    }

    private void OnAnyButtonClicked(Button btn)
    {
        onButtonClicked?.Invoke();
    }
}
