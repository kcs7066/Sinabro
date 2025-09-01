using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 이 스크립트는 퀵슬롯 UI와 장착 시스템을 관리합니다.
public class QuickSlotManager : MonoBehaviour
{
    public InventoryManager inventoryManager;
    // public PlayerEquipment playerEquipment; // Inspector 연결을 제거합니다.
    private PlayerEquipment playerEquipment; // 스크립트가 직접 찾을 변수입니다.
    public GameObject quickSlotPanel;

    private List<InventorySlotUI> quickSlotUIs = new List<InventorySlotUI>();
    private int selectedSlotIndex = 0;

    void Start()
    {
        quickSlotUIs.AddRange(quickSlotPanel.GetComponentsInChildren<InventorySlotUI>());
        UpdateSelectedSlotVisual();
    }

    void Update()
    {
        // --- 추가된 부분 ---
        // 아직 로컬 플레이어를 찾지 못했다면, 매 프레임마다 찾아봅니다.
        if (playerEquipment == null && PlayerController.LocalInstance != null)
        {
            // 찾았다면, 그 플레이어의 PlayerEquipment 컴포넌트를 가져옵니다.
            playerEquipment = PlayerController.LocalInstance.GetComponent<PlayerEquipment>();

            // 플레이어를 찾은 이 시점에 UI를 한번 동기화하고 초기 아이템을 장착합니다.
            UpdateAllQuickSlotsUI();
        }
        // ---

        // 플레이어를 찾지 못했다면 키 입력을 받지 않습니다.
        if (playerEquipment == null) return;

        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlotIndex = i;
                UpdateSelectedSlotVisual();
                EquipItemFromSlot(selectedSlotIndex);
                break;
            }
        }
    }

    public void UpdateAllQuickSlotsUI()
    {
        // 플레이어를 찾지 못했다면 UI 업데이트를 하지 않습니다.
        if (playerEquipment == null) return;

        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (i < inventoryManager.inventorySlots.Count)
            {
                quickSlotUIs[i].UpdateSlot(inventoryManager.inventorySlots[i]);
            }
            else
            {
                quickSlotUIs[i].ClearSlot();
            }
        }
        EquipItemFromSlot(selectedSlotIndex);
    }

    void UpdateSelectedSlotVisual()
    {
        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (i == selectedSlotIndex)
            {
                quickSlotUIs[i].GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                quickSlotUIs[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    void EquipItemFromSlot(int slotIndex)
    {
        if (playerEquipment == null) return;

        if (slotIndex < inventoryManager.inventorySlots.Count)
        {
            InventorySlot slotToEquip = inventoryManager.inventorySlots[slotIndex];
            playerEquipment.EquipItem(slotToEquip);
        }
        else
        {
            playerEquipment.UnequipItem();
        }
    }
}
