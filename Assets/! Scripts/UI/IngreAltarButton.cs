using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngreAltarButton : MonoBehaviour
{
    public Image sprite;
    public TextMeshProUGUI ingreName;
    public TextMeshProUGUI description;
    public GameObject parchmentToSpawn;

    [Header("Assigned in Prefab")]
    public Transform descTab;
    public Button spawnButton;

    private bool isDescVisible;

    [Header("Settings")]
    public float spawnCooldown = 0.15f;

    private void Awake()
    {
        if (spawnButton != null)
            spawnButton.onClick.AddListener(OnSpawnButtonClicked);
    }

    public void ToggleDescription()
    {
        isDescVisible = !isDescVisible;
        if (descTab != null)
            descTab.gameObject.SetActive(isDescVisible);
    }

    private void OnSpawnButtonClicked()
    {
        if (!spawnButton.interactable) return;

        Spawn();
        StartCoroutine(DisableButtonTemporarily());
    }

    public void Spawn()
    {
        if (parchmentToSpawn != null && AltarBook.instance != null)
            AltarBook.instance.DoSpawn(parchmentToSpawn);
    }

    private IEnumerator DisableButtonTemporarily()
    {
        spawnButton.interactable = false;
        yield return new WaitForSeconds(spawnCooldown);
        spawnButton.interactable = true;
    }

    public void HideButton()
    {
        spawnButton.interactable = false;
        spawnButton.gameObject.SetActive(false);
    }
}
