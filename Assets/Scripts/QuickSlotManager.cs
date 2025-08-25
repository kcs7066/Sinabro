using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 제어하기 위해 필요합니다.

// 이 스크립트는 퀵슬롯 UI와 장착 시스템을 관리합니다.
public class QuickSlotManager : MonoBehaviour
{
    // 유니티 에디터에서 연결해 줄 변수들
    public InventoryManager inventoryManager;
    public PlayerEquipment playerEquipment;
    public GameObject quickSlotPanel; // 퀵슬롯 UI들이 들어있는 Panel

    private List<InventorySlotUI> quickSlotUIs = new List<InventorySlotUI>();
    private int selectedSlotIndex = 0; // 현재 선택된 퀵슬롯의 인덱스

    void Start()
    {
        quickSlotUIs.AddRange(quickSlotPanel.GetComponentsInChildren<InventorySlotUI>());
        UpdateAllQuickSlotsUI(); // 게임 시작 시 인벤토리와 UI 동기화
        UpdateSelectedSlotVisual();
        EquipItemFromSlot(selectedSlotIndex); // 첫 번째 슬롯 아이템 자동 장착
    }

    void Update()
    {
        // 숫자 키 입력을 감지합니다 (퀵슬롯 개수만큼).
        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlotIndex = i;
                UpdateSelectedSlotVisual();
                EquipItemFromSlot(selectedSlotIndex);
                break; // 키 하나만 처리하도록 루프를 빠져나갑니다.
            }
        }
    }

    // 퀵슬롯 전체 UI를 인벤토리 데이터에 맞춰 업데이트하는 함수
    public void UpdateAllQuickSlotsUI()
    {
        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            // 만약 인벤토리에 해당 슬롯의 아이템이 있다면,
            if (i < inventoryManager.inventorySlots.Count)
            {
                quickSlotUIs[i].UpdateSlot(inventoryManager.inventorySlots[i]);
            }
            else // 없다면,
            {
                quickSlotUIs[i].ClearSlot();
            }
        }
        // UI가 변경되었으니, 현재 선택된 아이템을 다시 장착합니다.
        EquipItemFromSlot(selectedSlotIndex);
    }

    // 선택된 슬롯을 시각적으로 표시하는 함수
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

    // 해당 슬롯의 아이템을 장착하는 함수
    void EquipItemFromSlot(int slotIndex)
    {
        // 인벤토리에 해당 슬롯의 아이템이 실제로 존재하는지 확인합니다.
        if (slotIndex < inventoryManager.inventorySlots.Count)
        {
            InventorySlot slotToEquip = inventoryManager.inventorySlots[slotIndex];

            // PlayerEquipment에게 이 슬롯의 아이템을 장착하라고 명령합니다.
            playerEquipment.EquipItem(slotToEquip);
        }
        else
        {
            // 해당 슬롯이 비어있다면, 장착을 해제합니다.
            playerEquipment.UnequipItem();
        }
    }
}
