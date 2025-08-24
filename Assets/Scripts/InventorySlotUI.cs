using UnityEngine;
using UnityEngine.UI; // 기본 UI(Image)를 사용하기 위해 필요합니다.
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

// 이 스크립트는 인벤토리의 각 UI 슬롯 하나하나를 제어합니다.
public class InventorySlotUI : MonoBehaviour
{
    // 유니티 에디터에서 연결해 줄 UI 요소들
    public Image itemIcon;        // 아이템 아이콘을 표시할 이미지
    public TextMeshProUGUI quantityText; // 아이템 수량을 표시할 텍스트

    // 이 슬롯 UI를 업데이트하는 함수
    public void UpdateSlot(InventorySlot slot)
    {
        // 만약 슬롯이 비어있지 않다면 (아이템이 있다면)
        if (slot.itemData != null)
        {
            itemIcon.sprite = slot.itemData.itemIcon; // 아이콘 이미지를 설정합니다.
            itemIcon.enabled = true; // 아이콘 이미지를 보이게 합니다.
            quantityText.text = slot.quantity.ToString(); // 수량을 텍스트로 변환하여 설정합니다.
        }
        else // 슬롯이 비어있다면
        {
            ClearSlot();
        }
    }

    // 슬롯을 깨끗하게 비우는 함수
    public void ClearSlot()
    {
        itemIcon.sprite = null; // 아이콘 이미지를 비웁니다.
        itemIcon.enabled = false; // 아이콘 이미지를 보이지 않게 합니다.
        quantityText.text = ""; // 수량 텍스트를 비웁니다.
    }
}
