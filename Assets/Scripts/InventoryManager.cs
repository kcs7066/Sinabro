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
    public QuickSlotManager quickSlotManager; // 퀵슬롯 매니저를 참조할 변수

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

        // 인벤토리 UI가 변경될 때마다 퀵슬롯 UI도 업데이트하도록 호출합니다.
        if (quickSlotManager != null)
        {
            quickSlotManager.UpdateAllQuickSlotsUI();
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

    public void RemoveItem(ItemData itemData, int amount)
    {
        InventorySlot slotToRemove = null;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData == itemData)
            {
                slot.RemoveQuantity(amount);
                if (slot.quantity <= 0)
                {
                    slotToRemove = slot;
                }
                break;
            }
        }
        if (slotToRemove != null)
        {
            inventorySlots.Remove(slotToRemove);
        }
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