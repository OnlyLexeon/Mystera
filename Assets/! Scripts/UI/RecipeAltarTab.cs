using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeAltarButton : MonoBehaviour
{
    public Image sprite;
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI description;

    [Header("Assign in Prefab")]
    public Transform ingredientsHolder;
    public Transform descTab;

    private bool isDescVisible = false;

    public void ToggleDescription()
    {
        isDescVisible = !isDescVisible;
        if (descTab != null)
            descTab.gameObject.SetActive(isDescVisible);
    }
}
