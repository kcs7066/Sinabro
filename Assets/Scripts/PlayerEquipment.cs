using Unity.Netcode;
using UnityEngine;

// 이 스크립트는 플레이어가 현재 장착/손에 든 아이템을 관리합니다.
public class PlayerEquipment : NetworkBehaviour
{
    // InventorySlot은 이제 struct이므로 null이 될 수 없습니다.
    // 비어있는 상태는 itemID가 0인 것으로 판단합니다.
    public InventorySlot equippedItemSlot;

    // "Hand" 자식 오브젝트의 SpriteRenderer를 연결할 변수입니다.
    public SpriteRenderer handSpriteRenderer;

    // 인벤토리 슬롯의 아이템을 장착하는 함수
    public void EquipItem(InventorySlot slot)
    {
        // 아이템 ID가 0보다 크면 유효한 아이템으로 간주합니다.
        if (slot.itemID > 0)
        {
            equippedItemSlot = slot;
            ItemData data = ItemDatabase.Instance.GetItemById(slot.itemID);
            if (data != null)
            {
                Debug.Log(data.itemName + " 장착!");
                if (handSpriteRenderer != null)
                {
                    handSpriteRenderer.sprite = data.itemIcon;
                }
            }
        }
        else
        {
            // 유효하지 않은 아이템이면 장착을 해제합니다.
            UnequipItem();
        }
    }

    // 아이템 장착을 해제하는 함수
    public void UnequipItem()
    {
        // null 대신 비어있는 새 struct를 할당합니다.
        equippedItemSlot = new InventorySlot();
        if (handSpriteRenderer != null)
        {
            handSpriteRenderer.sprite = null;
        }
        Debug.Log("장착 해제됨");
    }

    // 현재 장착한 도구의 채집 능력을 반환하는 함수
    public int GetCurrentToolPower()
    {
        // itemID가 0 이하면 비어있는 슬롯(맨손)으로 간주합니다.
        if (equippedItemSlot.itemID <= 0)
        {
            return 1; // 맨손
        }

        ItemData data = ItemDatabase.Instance.GetItemById(equippedItemSlot.itemID);
        if (data != null && data.itemType == ItemType.Tool)
        {
            return data.gatheringPower;
        }

        return 1; // 맨손
    }
}

