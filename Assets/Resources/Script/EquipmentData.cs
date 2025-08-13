using UnityEngine;

public enum EquipmentType 
{ 
    Weapon, 
    Helm, 
    Armor,
    Pants,
    Shoes,
    Necklace,
    Bracelet,
    Ring
} // 장비 부위

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentData : ItemData // 👈 ItemData를 상속받음
{
    [Header("기본 정보")]
    public EquipmentType equipType;
    public int level;

    [Header("기본 능력치")]
    public int attackPower; // 공격력
    public float attackSpeed; // 공격 속도 (초당 공격 횟수)
    public float attackRange; // 사거리

    [Header("특수 옵션")]
    public float moveSpeedBonus; // 이동 속도 증가량
    public int additionalAttackPower; // 추가 공격력
    public float additionalAttackSpeed; // 추가 공격 속도 (보너스 %)
    public float itemDropRateBonus; // 아이템 드롭률 증가 (%)
}