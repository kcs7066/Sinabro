using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 인벤토리의 한 칸(슬롯)에 들어갈 정보를 담습니다.
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
    }
}

// 이 스크립트는 게임의 전체 인벤토리를 관리하고 UI를 업데이트합니다.
public class InventoryManager : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public GameObject inventoryPanel;
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private bool isInventoryOpen = false;

    void Start()
    {
        slotUIs.AddRange(inventoryPanel.GetComponentsInChildren<InventorySlotUI>());
        UpdateInventoryUI();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

    public void UpdateInventoryUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                slotUIs[i].UpdateSlot(inventorySlots[i]);
            }
            else
            {
                slotUIs[i].ClearSlot();
            }
        }
    }

    public void AddItem(ItemData itemData, int amount)
    {
        bool itemExists = false;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData == itemData)
            {
                slot.AddQuantity(amount);
                itemExists = true;
                break;
            }
        }
        if (!itemExists)
        {
            inventorySlots.Add(new InventorySlot(itemData, amount));
        }
        UpdateInventoryUI();
    }

    // 아이템을 제거하는 함수 (새로 추가됨!)
    public void RemoveItem(ItemData itemData, int amount)
    {
        InventorySlot slotToRemove = null;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData == itemData)
            {
                slot.RemoveQuantity(amount);
                // 만약 아이템 개수가 0 이하가 되면, 이 슬롯을 완전히 제거해야 합니다.
                if (slot.quantity <= 0)
                {
                    slotToRemove = slot;
                }
                break;
            }
        }

        // 루프가 끝난 후, 제거할 슬롯이 있다면 리스트에서 제거합니다.
        if (slotToRemove != null)
        {
            inventorySlots.Remove(slotToRemove);
        }

        // 아이템을 제거한 후, UI를 즉시 업데이트합니다.
        UpdateInventoryUI();
    }

    public int GetItemQuantity(ItemData itemData)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData == itemData)
            {
                return slot.quantity;
            }
        }
        return 0;
    }
}
