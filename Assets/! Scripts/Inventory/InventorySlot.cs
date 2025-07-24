using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;
    public Button button;

    private string itemID;
    private StorableData data;
    private HatInventoryUI hatUI;

    public void SetSlot(HatInventoryManager.InventorySlot slot)
    {
        itemID = slot.itemID;
        data = StorableDatabase.Instance.GetDataByID(itemID);
        icon.sprite = data.icon;
        countText.text = slot.stackCount.ToString();

        hatUI = FindFirstObjectByType<HatInventoryUI>();
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

        if (HatInventoryManager.Instance.TryRemoveItem(itemID))
        {
            GameObject obj = Instantiate(data.prefab, transform.position + Vector3.forward, Quaternion.identity);

            //prevent same item from getting back into hat
            var storable = obj.GetComponent<StorableObject>();
            if (storable != null)
                storable.SetFreshSpawn(2f);

            //disable buttons
            hatUI.DisableAllButtons(1f);
        }
    }
}
