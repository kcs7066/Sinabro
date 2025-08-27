using UnityEngine;

// 이 스크립트는 플레이어가 현재 장착/손에 든 아이템을 관리합니다.
public class PlayerEquipment : MonoBehaviour
{
    // 현재 손에 들고 있는 아이템 슬롯 정보
    public InventorySlot equippedItemSlot;

    // 유니티 에디터에서 연결할, 손에 든 아이템을 표시할 Sprite Renderer
    public SpriteRenderer handSpriteRenderer;

    // 인벤토리 슬롯의 아이템을 장착하는 함수
    public void EquipItem(InventorySlot slot)
    {
        // 아이템 타입이 도구일 때만 장착합니다.
        if (slot != null && slot.itemData != null && slot.itemData.itemType == ItemType.Tool)
        {
            equippedItemSlot = slot;
            // 손에 든 아이템의 스프라이트를 업데이트합니다.
            handSpriteRenderer.sprite = slot.itemData.itemIcon;
            Debug.Log(slot.itemData.itemName + " 장착!");
        }
        else
        {
            // 도구가 아니거나 빈 슬롯이면 장착을 해제합니다.
            UnequipItem();
        }
    }

    // 아이템 장착을 해제하는 함수
    public void UnequipItem()
    {
        equippedItemSlot = null;
        // 손에 든 아이템의 스프라이트를 비웁니다.
        handSpriteRenderer.sprite = null;
        Debug.Log("장착 해제됨");
    }

    // 현재 장착한 도구의 채집 능력을 반환하는 함수
    public int GetCurrentToolPower()
    {
        if (equippedItemSlot == null || equippedItemSlot.itemData == null)
        {
            return 1; // 맨손
        }

        if (equippedItemSlot.itemData.itemType == ItemType.Tool)
        {
            return equippedItemSlot.itemData.gatheringPower;
        }

        return 1; // 맨손
    }
}
