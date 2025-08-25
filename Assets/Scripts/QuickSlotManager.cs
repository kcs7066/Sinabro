using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image ������Ʈ�� �����ϱ� ���� �ʿ��մϴ�.

// �� ��ũ��Ʈ�� ������ UI�� ���� �ý����� �����մϴ�.
public class QuickSlotManager : MonoBehaviour
{
    // ����Ƽ �����Ϳ��� ������ �� ������
    public InventoryManager inventoryManager;
    public PlayerEquipment playerEquipment;
    public GameObject quickSlotPanel; // ������ UI���� ����ִ� Panel

    private List<InventorySlotUI> quickSlotUIs = new List<InventorySlotUI>();
    private int selectedSlotIndex = 0; // ���� ���õ� �������� �ε���

    void Start()
    {
        quickSlotUIs.AddRange(quickSlotPanel.GetComponentsInChildren<InventorySlotUI>());
        UpdateAllQuickSlotsUI(); // ���� ���� �� �κ��丮�� UI ����ȭ
        UpdateSelectedSlotVisual();
        EquipItemFromSlot(selectedSlotIndex); // ù ��° ���� ������ �ڵ� ����
    }

    void Update()
    {
        // ���� Ű �Է��� �����մϴ� (������ ������ŭ).
        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlotIndex = i;
                UpdateSelectedSlotVisual();
                EquipItemFromSlot(selectedSlotIndex);
                break; // Ű �ϳ��� ó���ϵ��� ������ ���������ϴ�.
            }
        }
    }

    // ������ ��ü UI�� �κ��丮 �����Ϳ� ���� ������Ʈ�ϴ� �Լ�
    public void UpdateAllQuickSlotsUI()
    {
        for (int i = 0; i < quickSlotUIs.Count; i++)
        {
            // ���� �κ��丮�� �ش� ������ �������� �ִٸ�,
            if (i < inventoryManager.inventorySlots.Count)
            {
                quickSlotUIs[i].UpdateSlot(inventoryManager.inventorySlots[i]);
            }
            else // ���ٸ�,
            {
                quickSlotUIs[i].ClearSlot();
            }
        }
        // UI�� ����Ǿ�����, ���� ���õ� �������� �ٽ� �����մϴ�.
        EquipItemFromSlot(selectedSlotIndex);
    }

    // ���õ� ������ �ð������� ǥ���ϴ� �Լ�
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

    // �ش� ������ �������� �����ϴ� �Լ�
    void EquipItemFromSlot(int slotIndex)
    {
        // �κ��丮�� �ش� ������ �������� ������ �����ϴ��� Ȯ���մϴ�.
        if (slotIndex < inventoryManager.inventorySlots.Count)
        {
            InventorySlot slotToEquip = inventoryManager.inventorySlots[slotIndex];

            // PlayerEquipment���� �� ������ �������� �����϶�� ����մϴ�.
            playerEquipment.EquipItem(slotToEquip);
        }
        else
        {
            // �ش� ������ ����ִٸ�, ������ �����մϴ�.
            playerEquipment.UnequipItem();
        }
    }
}
