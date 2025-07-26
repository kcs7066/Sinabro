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
    // 인벤토리 슬롯들을 담을 리스트
    public List<InventorySlot> slots = new List<InventorySlot>();
    private int slotCount = 24; // 예: 총 24칸

    void Start()
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
        // 비어있는 슬롯을 찾아 아이템 추가
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == null)
            {
                slots[i] = new InventorySlot(item, 1);
                Debug.Log($"{item.itemName}을(를) 추가했습니다.");
                return;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다.");
    }
}