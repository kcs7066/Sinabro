using UnityEngine;

public enum ConsumableType { HpPotion, AtkBuff, SpeedBuff } // 효과 종류

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    [Header("소모품 정보")]
    public ConsumableType consumableType;
    public float value;         // 효과 수치 (예: 회복량 50, 버프 배율 1.1)
    public float duration;      // 지속 시간 (초)
    public int maxStack = 99;   // 최대 겹치기
}