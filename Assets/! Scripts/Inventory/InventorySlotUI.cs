using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public Image subIcon;
    public TextMeshProUGUI countText;
    public Button button;

    private string itemID;
    private StorableData data;
    private HatInventoryUI hatUI;

    private HatInventoryManager.InventorySlot mySlot;

    public void SetSlot(HatInventoryManager.InventorySlot slot)
    {
        mySlot = slot;
        itemID = slot.itemID;
        data = StorableDatabase.Instance.GetDataByID(itemID);
        hatUI = FindFirstObjectByType<HatInventoryUI>();

        icon.sprite = data.icon;
        if (slot.GetDeserializedData() is StoredPotionData potionData)
        {
            Recipe recipe = RecipeManager.Instance.GetRecipeByName(potionData.recipeID);
            if (recipe != null && recipe.icon != null)
            {
                icon.sprite = PotionMaterialsManager.instance.emptyGlassSprite;
                subIcon.gameObject.SetActive(true);
                subIcon.sprite = recipe.icon;
            }
        }

        countText.text = slot.stackCount.ToString();
        button.onClick.AddListener(SpawnItem);
    }

    public void OnEnterXR()
    {
        if (hatUI != null && data != null)
        {
            hatUI.tooltipText.text = data.displayName;
            hatUI.tooltip.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(hatUI.tooltipText.rectTransform);
            Vector2 textSize = hatUI.tooltipText.rectTransform.sizeDelta;
            hatUI.tooltipBackground.sizeDelta = textSize + new Vector2(1f, 1f);

            RectTransform tooltipRect = hatUI.tooltip.GetComponent<RectTransform>();
            RectTransform myRect = GetComponent<RectTransform>();
            if (tooltipRect != null && myRect != null)
            {
                Vector3 worldAbove = myRect.position + Vector3.up * 0.12f;
                tooltipRect.position = worldAbove;
            }
        }
    }

    public void OnExitXR()
    {
        if (hatUI != null)
        {
            hatUI.tooltip.SetActive(false);
            hatUI.tooltipText.text = "";
        }
    }


    private void SpawnItem()
    {
        if (hatUI.buttonsDisabled || data == null || hatUI == null) return;

        var storedData = mySlot.GetDeserializedData();

        if (HatInventoryManager.instance.TryRemoveItem(itemID, storedData))
        {
            GameObject obj = Instantiate(data.prefab, hatUI.hat.spawnPos.position, Quaternion.identity);

            //potions
            var potion = obj.GetComponent<PotionStorable>();
            if (potion != null && storedData is StoredPotionData potionData)
                potion.LoadPotionData(potionData);

            //lockout the object from being stored again
            StorableObject storable = obj.GetComponent<PotionStorable>() ?? obj.GetComponent<StorableObject>();
            if (storable != null)
                storable.SetFreshSpawn(2.5f);

            //rigidbody
            //make item float MAKE SURE ONHOVER WILL MAKE THEM iskinematic false and gravity true
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            //disable all buttons
            hatUI.DisableAllButtons(1f);

            //effects
            hatUI.hat.SpawnedDoEffects();

            //UI
            hatUI.RefreshUI();
            OnExitXR();
        }

    }
}
