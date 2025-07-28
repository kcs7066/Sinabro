using System.Collections.Generic;
using UnityEngine;

// 인벤토리 슬롯 하나에 들어갈 데이터 (아이템, 개수)
[System.Serializable] // 이 어트리뷰트가 있어야 인스펙터에서 보입니다.
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class Inventory : MonoBehaviour
{
    // ▼▼▼ UI 인벤토리 스크립트를 연결할 변수 추가 ▼▼▼
    public UI_Inventory uiInventory;

    // 인벤토리 슬롯들을 담을 리스트
    public List<InventorySlot> slots = new List<InventorySlot>();
    private int slotCount = 24; // 예: 총 24칸

    void Awake()
    {
        // 인벤토리 초기화 (빈 슬롯 20개 생성)
        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }

    // 아이템 추가 함수
    public void AddItem(ItemData item)
    {
        // 1. 이미 같은 아이템이 있고, 겹칠 여유 공간이 있는지 확인
        for (int i = 0; i < slots.Count; i++)
        {
            // 슬롯에 아이템이 있고, 그 아이템이 추가하려는 아이템과 같으며, 최대 수량 미만일 때
            if (slots[i].item != null && slots[i].item == item && slots[i].quantity < item.maxStack)
            {
                slots[i].quantity++; // 수량만 1 증가
                Debug.Log($"{item.itemName}의 수량이 증가했습니다. 현재: {slots[i].quantity}");

                // ▼▼▼ 이 부분을 수정합니다 ▼▼▼
                if (uiInventory != null && uiInventory.gameObject.activeInHierarchy)
                {
                    uiInventory.UpdateInventoryUI();
                }
                return; // 아이템을 겹쳤으므로 함수 종료
            }
        }

        // 2. 겹칠 아이템이 없었다면, 빈 슬롯을 찾아 새로 추가
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == null)
            {
                slots[i] = new InventorySlot(item, 1);
                Debug.Log($"{item.itemName}을(를) 새로 추가했습니다.");

                // ▼▼▼ 이 부분도 똑같이 수정합니다 ▼▼▼
                if (uiInventory != null && uiInventory.gameObject.activeInHierarchy)
                {
                    uiInventory.UpdateInventoryUI();
                }
                return; // 아이템을 추가했으므로 함수 종료
            }
        }

        // 3. 여기까지 왔다면 인벤토리가 가득 찬 것
        Debug.Log("인벤토리가 가득 찼습니다.");
    }
}