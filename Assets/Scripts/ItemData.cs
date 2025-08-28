using UnityEngine;

public enum ItemType
{
    Resource,
    Tool,
    Consumable
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public int itemID; // 아이템 고유 ID (절대 중복되면 안 됩니다!)
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string itemDescription;
    public ItemType itemType;

    [Header("도구 정보")]
    public int gatheringPower = 1;
}
