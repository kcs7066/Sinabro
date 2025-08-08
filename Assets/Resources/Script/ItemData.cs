using UnityEngine;
using UnityEngine.Rendering;

// ▼▼▼ 장비 부위를 나타내는 열거형 추가 ▼▼▼
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
}

// 프로젝트 창에서 우클릭 > Create > Inventory > Item 으로 아이템을 생성할 수 있게 해주는 메뉴
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // 아이템 이름
    public Sprite itemIcon;      // 아이템 아이콘 이미지
    public string description;   // 아이템 설명
    public int maxStack = 99;      // 최대 겹치기 개수 등 추가 가능

    [Header("장비 정보")]
    public bool isEquipment; // 이 아이템이 장비인가?
    public EquipmentType equipmentType; // 어떤 부위의 장비인가?

    // ▼▼▼ 장비 아이템이 제공하는 능력치 추가 ▼▼▼
    public int attackBonus;
    public int attackspeedBonus;
    public int levelBonus;
    // (필요에 따라 체력, 공격속도 등 추가 가능)
}