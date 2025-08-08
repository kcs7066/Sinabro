using UnityEngine;
using System.Collections.Generic;

public class UI_EquipmentPanel : MonoBehaviour
{
    // 8개의 장비 슬롯 스크립트들을 담을 리스트
    public List<UI_EquipmentSlot> equipmentSlots;

    // 데이터가 있는 EquipmentManager 참조
    private EquipmentManager equipmentManager;

    void Start()
    {
        // 플레이어에게서 EquipmentManager를 찾아 연결
        equipmentManager = GameObject.FindGameObjectWithTag("Player").GetComponent<EquipmentManager>();
        UpdateUI();
    }

    // 장비창이 켜질 때마다 UI를 업데이트
    void OnEnable()
    {
        // Start보다 늦게 호출되도록 null 체크
        if (equipmentManager != null)
        {
            UpdateUI();
        }
    }

    // 데이터에 맞춰 UI를 새로고침하는 함수
    public void UpdateUI()
    {
        // 모든 장비 슬롯을 순회
        foreach (UI_EquipmentSlot slot in equipmentSlots)
        {
            // 해당 부위에 장착된 아이템이 있는지 데이터 확인
            if (equipmentManager.equippedItems.TryGetValue(slot.equipmentType, out ItemData equippedItem))
            {
                // 아이템이 있으면 아이콘 표시
                slot.itemIcon.sprite = equippedItem.itemIcon;
                slot.itemIcon.enabled = true;
            }
            else
            {
                // 아이템이 없으면 아이콘 숨김
                slot.itemIcon.enabled = false;
            }
        }
    }
}