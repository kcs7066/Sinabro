using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;

public class QuickSlotManager : MonoBehaviour
{
    private PlayerInventory localPlayerInventory;
    private PlayerEquipment localPlayerEquipment;
    public GameObject quickSlotPanel;
    public Color highlightColor = Color.yellow;

    private List<InventorySlotUI> quickSlotUIs = new List<InventorySlotUI>();
    private List<Image> quickSlotBackgrounds = new List<Image>();
    private int selectedSlotIndex = -1;

    void Start()
    {
        quickSlotUIs.AddRange(quickSlotPanel.GetComponentsInChildren<InventorySlotUI>());
        foreach (var slotUI in quickSlotUIs)
        {
            quickSlotBackgrounds.Add(slotUI.GetComponent<Image>());
        }
        UpdateHighlight(); // 초기 하이라이트 설정
    }

    void Update()
    {
        // 로컬 플레이어 인스턴스를 동적으로 찾아 연결합니다.
        if (localPlayerInventory == null && PlayerInventory.LocalInstance != null)
        {
            localPlayerInventory = PlayerInventory.LocalInstance;
        }
        if (localPlayerEquipment == null && PlayerInventory.LocalInstance != null)
        {
            if (PlayerInventory.LocalInstance.TryGetComponent<PlayerEquipment>(out var equipment))
            {
                localPlayerEquipment = equipment;
            }
        }

        // 플레이어를 찾은 후에만 입력을 처리합니다.
        if (localPlayerInventory == null || localPlayerEquipment == null) return;

        // 숫자 키 입력으로 슬롯을 선택합니다.
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                break;
            }
        }
    }

    // InventoryManager가 호출하여 퀵슬롯 UI를 새로고침합니다.
    public void UpdateAllQuickSlotsUI()
    {
        if (localPlayerInventory == null) return;

        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (i < localPlayerInventory.inventorySlots.Count)
            {
                ItemData itemData = ItemDatabase.Instance.GetItemById(localPlayerInventory.inventorySlots[i].itemID);
                if (itemData != null)
                {
                    quickSlotUIs[i].UpdateSlot(itemData, localPlayerInventory.inventorySlots[i].quantity);
                }
            }
            else
            {
                quickSlotUIs[i].ClearSlot();
            }
        }

        // 인벤토리 변경으로 인해 현재 장착한 아이템 정보가 바뀌었을 수 있으므로, 장착 상태를 다시 확인합니다.
        EquipItemFromSelectedSlot();
    }

    // 특정 슬롯을 선택하는 함수
    void SelectSlot(int slotIndex)
    {
        // 이미 선택된 슬롯을 다시 누르면 선택을 해제합니다.
        if (selectedSlotIndex == slotIndex)
        {
            selectedSlotIndex = -1;
        }
        else
        {
            selectedSlotIndex = slotIndex;
        }

        EquipItemFromSelectedSlot();
        UpdateHighlight();
    }

    // 현재 선택된 슬롯의 아이템을 장착하는 함수
    void EquipItemFromSelectedSlot()
    {
        if (localPlayerEquipment == null || localPlayerInventory == null) return;

        // 유효한 슬롯이 선택되었고, 그 슬롯에 아이템이 있다면 장착합니다.
        if (selectedSlotIndex >= 0 && selectedSlotIndex < localPlayerInventory.inventorySlots.Count)
        {
            localPlayerEquipment.EquipItem(localPlayerInventory.inventorySlots[selectedSlotIndex]);
        }
        else
        {
            // 빈 슬롯이거나 선택이 해제되면 장착을 해제합니다.
            localPlayerEquipment.UnequipItem();
        }
    }

    // 선택된 슬롯에 하이라이트를 표시하는 함수
    void UpdateHighlight()
    {
        for (int i = 0; i < quickSlotBackgrounds.Count; i++)
        {
            quickSlotBackgrounds[i].color = (i == selectedSlotIndex) ? highlightColor : Color.white;
        }
    }
}

