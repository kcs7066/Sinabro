using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 제작에 필요한 재료 하나를 정의합니다. (예: 통나무 5개)
[System.Serializable]
public class CraftingIngredient
{
    public ItemData itemData; // 필요한 아이템
    public int quantity;      // 필요한 수량
}

// 이 스크립트는 하나의 '제작법'을 정의하는 설계도입니다.
[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    // 제작에 필요한 재료들의 리스트입니다.
    public List<CraftingIngredient> ingredients;

    // 제작 결과로 얻게 될 아이템입니다.
    public ItemData resultItem;

    // 제작 결과로 몇 개의 아이템을 얻을지 정합니다.
    public int resultQuantity = 1;
}
