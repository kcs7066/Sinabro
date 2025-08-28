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

public class InventoryManager : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public GameObject inventoryPanel;
    public QuickSlotManager quickSlotManager;

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

        if (quickSlotManager != null)
        {
            quickSlotManager.UpdateAllQuickSlotsUI();
        }
    }

    // 이제 updateUI 매개변수를 받습니다.
    public void AddItem(ItemData itemData, int amount, bool updateUI = true)
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

        if (updateUI) UpdateInventoryUI();
    }

    // 이제 updateUI 매개변수를 받습니다.
    public void RemoveItem(ItemData itemData, int amount, bool updateUI = true)
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

        if (updateUI) UpdateInventoryUI();
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
