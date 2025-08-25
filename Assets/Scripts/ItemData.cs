using UnityEngine;

// 아이템의 종류를 구분하기 위한 열거형(Enum)입니다.
public enum ItemType
{
    Resource, // 자원 (통나무, 돌멩이 등)
    Tool,     // 도구 (도끼, 곡괭이 등)
    Consumable // 소모품 (음식, 포션 등)
}

// 이 스크립트는 게임 내 모든 아이템의 '설계도' 역할을 합니다.
[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")] // Inspector 창에서 구분을 위한 헤더입니다.
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string itemDescription;
    public ItemType itemType; // 이 아이템의 종류

    [Header("도구 정보")] // 이 아래의 변수들은 itemType이 Tool일 때만 의미가 있습니다.
    public int gatheringPower = 1; // 채집 능력 (높을수록 더 강한 피해를 줌)
}
