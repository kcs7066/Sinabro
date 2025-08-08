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

    void Awake()
    {
        // 같은 오브젝트에 있는 Inventory 스크립트를 찾아 연결
        inventory = GetComponent<Inventory>();
    }

    // 아이템 장착 함수
    public void Equip(ItemData newItem)
    {
        // 장비 아이템이 아니면 함수 종료
        if (!newItem.isEquipment) return;

        EquipmentType type = newItem.equipmentType;

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
        // TODO: UI 업데이트 및 스탯 재계산
    }
}