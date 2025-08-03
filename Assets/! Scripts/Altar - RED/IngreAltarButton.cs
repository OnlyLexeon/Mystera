using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class IngreAltarButton : MonoBehaviour
{
    public Image sprite;
    public Image bg;
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

    public void DisableIngredient()
    {
        if (spawnButton == null) return;

        spawnButton.interactable = false;

        ColorBlock cb = spawnButton.colors;
        Color dimGray = new Color(0.4f, 0.4f, 0.4f);

        cb.normalColor = dimGray;
        cb.highlightedColor = dimGray * 1.1f;
        cb.pressedColor = dimGray * 0.9f;
        cb.disabledColor = dimGray;

        spawnButton.colors = cb;

        Button rootButton = GetComponent<Button>();
        if (rootButton != null) rootButton.colors = cb;

        if (sprite != null) sprite.color = dimGray;
        if (ingreName != null) ingreName.color = dimGray;
        if (description != null) description.color = dimGray;
        if (bg != null) bg.color = dimGray;
    }

    public void EnableIngredient()
    {
        if (spawnButton != null)
        {
            spawnButton.interactable = true;
            spawnButton.colors = ColorBlock.defaultColorBlock;
        }

        if (sprite != null) sprite.color = Color.white;
        if (ingreName != null) ingreName.color = Color.white;
        if (description != null) description.color = Color.white;
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
