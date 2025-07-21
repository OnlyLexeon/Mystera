using UnityEngine;
using UnityEngine.UI;

public class DayNightUI : MonoBehaviour
{
    public Slider dayNightSlider;

    private void Update()
    {
        if (DayNightManager.instance == null || dayNightSlider == null)
            return;

        float cycleTime = DayNightManager.instance.time;
        float totalCycle = DayNightManager.instance.dayLength + DayNightManager.instance.nightLength;

        // Normalize time to 0–1
        float normalizedTime = cycleTime / totalCycle;

        // Make it fill to noon (0.5), then drain to midnight (1.0)
        float sliderValue = Mathf.Sin(normalizedTime * Mathf.PI);

        dayNightSlider.value = sliderValue;
    }
}
