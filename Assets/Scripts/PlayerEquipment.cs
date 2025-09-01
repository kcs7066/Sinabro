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
        // --- 수정된 부분 ---
        // 이제 아이템 타입과 상관없이 모든 아이템을 손에 들 수 있습니다.
        if (slot != null && slot.itemData != null)
        {
            equippedItemSlot = slot;
            handSpriteRenderer.sprite = slot.itemData.itemIcon;
            Debug.Log(slot.itemData.itemName + " 손에 듦!");
        }
        else
        {
            // 빈 슬롯이면 장착을 해제합니다.
            UnequipItem();
        }
    }

    // 아이템 장착을 해제하는 함수
    public void UnequipItem()
    {
        equippedItemSlot = null;
        handSpriteRenderer.sprite = null;
        Debug.Log("손 비움");
    }

    // 현재 손에 든 도구의 채집 능력을 반환하는 함수
    public int GetCurrentToolPower()
    {
        // 손에 든 아이템이 없거나, 아이템이 도구가 아니라면 기본 채집 능력(맨손)인 1을 반환합니다.
        if (equippedItemSlot == null || equippedItemSlot.itemData == null || equippedItemSlot.itemData.itemType != ItemType.Tool)
        {
            return 1; // 맨손
        }

        // 손에 든 아이템이 도구라면, 해당 도구의 gatheringPower를 반환합니다.
        return equippedItemSlot.itemData.gatheringPower;
    }
}
