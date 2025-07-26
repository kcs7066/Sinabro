using UnityEngine;

// 프로젝트 창에서 우클릭 > Create > Inventory > Item 으로 아이템을 생성할 수 있게 해주는 메뉴
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // 아이템 이름
    public Sprite itemIcon;      // 아이템 아이콘 이미지
    public string description;   // 아이템 설명
    // public int maxStack;      // 최대 겹치기 개수 등 추가 가능
}