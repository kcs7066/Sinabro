using UnityEngine;

// 이 스크립트는 게임 내 모든 아이템의 '설계도' 역할을 합니다.
// ScriptableObject를 사용하면, 이 스크립트를 기반으로 실제 아이템 에셋을 만들 수 있습니다.
[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // 아이템이 가질 기본적인 정보들입니다.
    public string itemName; // 아이템 이름 (예: "돌멩이")
    public Sprite itemIcon; // 인벤토리 슬롯에 표시될 아이템 아이콘 이미지

    [TextArea] // 여러 줄로 설명을 쓸 수 있게 해주는 기능입니다.
    public string itemDescription; // 아이템 설명
}
