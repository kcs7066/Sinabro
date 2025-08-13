using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    // 장비 부위별로 현재 착용 중인 아이템을 저장
    public Dictionary<EquipmentType, ItemData> equippedItems = new Dictionary<EquipmentType, ItemData>();

    // TODO: 나중에 장비창 UI 업데이트, 플레이어 스탯 업데이트 함수 호출
    // ▼▼▼ UI 패널 참조 변수 추가 ▼▼▼
    public UI_EquipmentPanel uiEquipmentPanel;
    private Inventory inventory; // 인벤토리 참조 변수 추가

    private PlayerStats playerStats; // CharacterStats 참조 변수

    void Awake()
    {
        // 같은 오브젝트에 있는 Inventory 스크립트를 찾아 연결
        inventory = GetComponent<Inventory>();
        playerStats = GetComponent<PlayerStats>(); // 컴포넌트 찾아두기
    }

    // 아이템 장착 함수
    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;

        EquipmentType type = newItem.equipType;

        // ▼▼▼ 장비 교체 로직 추가 ▼▼▼
        // 1. 만약 해당 부위에 이미 다른 아이템을 착용 중이라면
        if (equippedItems.ContainsKey(type) && equippedItems[type] != null)
        {
            // 2. 현재 착용 중인 아이템 정보를 가져옴
            ItemData oldItem = equippedItems[type];
            // 3. 인벤토리에 기존 아이템을 다시 추가
            inventory.AddItem(oldItem);
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // 4. 새로운 아이템을 장착
        equippedItems[type] = newItem;
        Debug.Log(newItem.itemName + "을(를) 장착했습니다.");

        // ▼▼▼ UI 업데이트 함수 호출 ▼▼▼
        if (uiEquipmentPanel != null)
        {
            uiEquipmentPanel.UpdateUI();
        }

        // ▼▼▼ playerStats 변수를 통해 능력치 재계산 함수 호출 ▼▼▼
        if (playerStats != null)
        {
            playerStats.CalculateFinalStats();
        }
    }

    public void Unequip(EquipmentType type)
    {
        if (!equippedItems.ContainsKey(type) || equippedItems[type] == null) return;

        ItemData oldItem = equippedItems[type];
        inventory.AddItem(oldItem);
        equippedItems.Remove(type);
        // 또는 equippedItems[type] = null;

        if (uiEquipmentPanel != null)
        {
            uiEquipmentPanel.UpdateUI();
        }

        // ▼▼▼ playerStats 변수를 통해 함수 호출 ▼▼▼
        if (playerStats != null)
        {
            playerStats.CalculateFinalStats();
        }
    }

}