using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    public void UpdateSlot(ItemData data, int quantity)
    {
        // ## 추가: 매우 상세한 디버그 로그 ##
        if (data == null)
        {
            Debug.LogError($"[SlotUI] UpdateSlot called with NULL ItemData on {gameObject.name}");
            ClearSlot();
            return;
        }

        // ## 추가: 어떤 아이콘을 그리려 하는지 정확히 확인합니다. ##
        Debug.Log($"[SlotUI] Updating {gameObject.name}. Item: {data.itemName}, Icon: {(data.itemIcon == null ? "NULL" : data.itemIcon.name)}, Quantity: {quantity}");

        // ## 추가: UI Image 컴포넌트가 연결되었는지 확인합니다. ##
        if (itemIcon == null)
        {
            Debug.LogError($"[SlotUI] itemIcon reference is NULL on {gameObject.name}!");
            return;
        }

        itemIcon.sprite = data.itemIcon;
        itemIcon.color = new Color(1, 1, 1, 1);

        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0);
        }
        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(false);
        }
    }
}

