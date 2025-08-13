using UnityEngine;

// ▼▼▼ 장비 부위를 나타내는 열거형 추가 ▼▼▼
//public enum EquipmentType
//{
//    Weapon,
//    Helm,
//    Armor,
//    Pants,
//    Shoes,
//    Necklace,
//    Bracelet,
//    Ring
//}

public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;      // 아이템 이름
    public Sprite itemIcon;      // 아이템 아이콘 이미지
    [TextArea] // 인스펙터에서 여러 줄로 편하게 입력하게 해줌
    public string description;   // 아이템 설명
}