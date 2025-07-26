using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // TextMeshPro를 사용하기 위해 필요

public class UI_Inventory : MonoBehaviour
{
    public Inventory inventory; // 데이터가 담긴 인벤토리 스크립트 연결
    public GameObject slotPrefab; // 슬롯 UI 프리팹 연결
    public Transform slotParent;  // 슬롯들이 생성될 부모 오브젝트(Grid Layout Group이 있는 곳)

    private List<UI_Slot> uiSlots = new List<UI_Slot>();

    void Start()
    {
        // 인벤토리 데이터의 슬롯 개수만큼 UI 슬롯 생성
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            uiSlots.Add(newSlot.GetComponent<UI_Slot>());
        }
    }

    // 인벤토리 UI를 최신 데이터로 업데이트하는 함수
    void UpdateInventoryUI()
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            // 데이터 슬롯에 아이템이 있으면 UI 슬롯에 정보 표시
            if (inventory.slots[i].item != null)
            {
                uiSlots[i].icon.sprite = inventory.slots[i].item.itemIcon;
                uiSlots[i].icon.gameObject.SetActive(true);
                uiSlots[i].quantityText.text = inventory.slots[i].quantity.ToString();
            }
            // 데이터 슬롯이 비어있으면 UI 슬롯도 비움
            else
            {
                uiSlots[i].icon.gameObject.SetActive(false);
                uiSlots[i].quantityText.text = "";
            }
        }
    }

    // 인벤토리 창이 활성화될 때마다 UI 업데이트
    void OnEnable()
    {
        UpdateInventoryUI();
    }
}