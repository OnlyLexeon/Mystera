using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Sprite icon;
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

        icon = data.icon;
        if (slot.GetDeserializedData() is StoredPotionData potionData)
        {
            Recipe recipe = RecipeManager.Instance.GetRecipeByName(potionData.recipeID);
            if (recipe != null && recipe.icon != null)
            {
                icon = recipe.icon;
            }
        }

        countText.text = slot.stackCount.ToString();
        button.onClick.AddListener(SpawnItem);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hatUI != null && data != null)
        {
            hatUI.tooltipText.text = data.displayName;
            hatUI.tooltip.SetActive(true);

            //resize tooltip image backgroun with text
            LayoutRebuilder.ForceRebuildLayoutImmediate(hatUI.tooltipText.rectTransform);
            Vector2 textSize = hatUI.tooltipText.rectTransform.sizeDelta;
            hatUI.tooltipBackground.sizeDelta = textSize + new Vector2(1f, 1f); //padding

            //position
            RectTransform tooltipRect = hatUI.tooltip.GetComponent<RectTransform>();
            RectTransform myRect = GetComponent<RectTransform>();
            if (tooltipRect != null && myRect != null)
            {
                Vector3 worldAbove = myRect.position + Vector3.up * 0.12f;
                tooltipRect.position = worldAbove;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
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

        if (HatInventoryManager.instance.TryRemoveItem(itemID))
        {
            GameObject obj = Instantiate(data.prefab, hatUI.hat.spawnPos.position, Quaternion.identity);

            //only for potions
            var potion = obj.GetComponent<PotionStorable>();
            var storedData = mySlot.GetDeserializedData();
            if (potion != null && storedData is StoredPotionData potionData)
                potion.LoadPotionData(potionData);

            //prevent same item from getting back into hat
            var storable = obj.GetComponent<StorableObject>();
            if (storable != null)
                storable.SetFreshSpawn(2.5f);

            //disable buttons
            hatUI.DisableAllButtons(1.5f);


            //Sound & particles
            hatUI.hat.SpawnedDoEffects();

        }
    }
}
