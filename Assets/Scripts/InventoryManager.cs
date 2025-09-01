using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public QuickSlotManager quickSlotManager;
    private PlayerInventory localPlayerInventory;
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private bool isInventoryOpen = false;

    void Start()
    {
        slotUIs.AddRange(inventoryPanel.GetComponentsInChildren<InventorySlotUI>());
        Debug.Log($"[InventoryManager] Found {slotUIs.Count} UI slots in the inventory panel.");

        inventoryPanel.SetActive(false);
        PlayerInventory.OnLocalInstanceReady += OnLocalInventoryReady;

        if (PlayerInventory.LocalInstance != null)
        {
            OnLocalInventoryReady(PlayerInventory.LocalInstance);
        }
    }

    private void OnLocalInventoryReady(PlayerInventory inventory)
    {
        if (localPlayerInventory != null) return;

        localPlayerInventory = inventory;
        Debug.Log("[Client] InventoryManager received LocalPlayerInventory via event. Subscribing to OnListChanged event.");
        localPlayerInventory.inventorySlots.OnListChanged += OnInventoryChanged;

        // ## 추가: 타이밍 문제를 해결하기 위한 지연된 UI 업데이트 ##
        StartCoroutine(DelayedUIUpdate());
    }

    private void OnDestroy()
    {
        PlayerInventory.OnLocalInstanceReady -= OnLocalInventoryReady;
        if (localPlayerInventory != null)
        {
            localPlayerInventory.inventorySlots.OnListChanged -= OnInventoryChanged;
        }
    }

    void Update()
    {
        if (localPlayerInventory == null) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

    private void OnInventoryChanged(NetworkListEvent<InventorySlot> changeEvent)
    {
        Debug.Log($"[Client] OnInventoryChanged event received! Type: {changeEvent.Type}, Index: {changeEvent.Index}");
        UpdateAllUIs();
    }

    private void UpdateAllUIs()
    {
        UpdateInventoryPanelUI();
        if (quickSlotManager != null)
        {
            quickSlotManager.UpdateAllQuickSlotsUI();
        }
    }

    // ## 추가: 지연된 UI 업데이트를 위한 코루틴 ##
    private IEnumerator DelayedUIUpdate()
    {
        // 0.1초 대기 후 UI를 업데이트하여 동기화 문제를 방지합니다.
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[InventoryManager] Performing delayed UI update.");
        UpdateAllUIs();
    }

    public void UpdateInventoryPanelUI()
    {
        if (localPlayerInventory == null) return;

        Debug.Log($"[Client] Updating Inventory UI. Found {localPlayerInventory.inventorySlots.Count} items.");

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (i < localPlayerInventory.inventorySlots.Count)
            {
                ItemData itemData = ItemDatabase.Instance.GetItemById(localPlayerInventory.inventorySlots[i].itemID);
                if (itemData != null)
                {
                    slotUIs[i].UpdateSlot(itemData, localPlayerInventory.inventorySlots[i].quantity);
                }
                else
                {
                    slotUIs[i].ClearSlot();
                }
            }
            else
            {
                slotUIs[i].ClearSlot();
            }
        }
    }
}

