using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotUIManager : MonoBehaviour
{
    public Transform iconsHolder;
    public GameObject imagePrefab;

    public GameObject stirBar;
    public GameObject timeBar;
    public GameObject effectBar;

    public Slider timeSlider;
    public Slider effectSlider;
    public Slider stirSlider;

    public TextMeshProUGUI timeMultText;
    public TextMeshProUGUI effectMultText;

    private void Start()
    {
        timeSlider.maxValue = Pot.instance.maxDurationMult - 1f;
        effectSlider.maxValue = Pot.instance.maxEffectMult - 1f;
        stirSlider.maxValue = Pot.instance.stirTime * Pot.instance.timesToStir;
    }

    public void AddIcon(Ingredient ingredient)
    {
        GameObject prefab = Instantiate(imagePrefab, iconsHolder);

        Image imageComponent = prefab.GetComponent<Image>();
        imageComponent.sprite = ingredient.sprite;
    }

    public void ClearIcons()
    {
        foreach (Transform child in iconsHolder)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateEffect()
    {
        float value = Pot.instance.effectMultiplier;
        effectSlider.value = value - 1f;

        effectMultText.text = "x" + value.ToString("F2");
    }

    public void UpdateTime()
    {
        float value = Pot.instance.durationMultiplier;

        timeSlider.value = value - 1f;
        timeMultText.text = "x" + value.ToString("F2");
    }

    public void UpdateStir()
    {
        stirSlider.value = Pot.instance.stirProgress + (Pot.instance.stirredTimes * Pot.instance.stirTime);
    }
}
