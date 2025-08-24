using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 게임의 전체 제작 시스템을 총괄합니다.
public class CraftingManager : MonoBehaviour
{
    // 유니티 에디터에서 연결해 줄 변수들
    public InventoryManager inventoryManager;
    public List<CraftingRecipe> allRecipes;
    public GameObject craftingPanel;          // 제작 창 Panel UI
    public Transform recipeSlotParent;
    public GameObject recipeSlotPrefab;

    private bool isCraftingOpen = false; // 제작 창의 현재 상태 (열림/닫힘)

    void Start()
    {
        // 게임이 시작되면 모든 제작법을 UI에 표시하고,
        SetupCraftingWindow();
        // 제작 창을 닫아둡니다.
        craftingPanel.SetActive(false);
    }

    // 매 프레임마다 호출됩니다.
    void Update()
    {
        // 만약 'C' 키를 눌렀다면,
        if (Input.GetKeyDown(KeyCode.C))
        {
            // isCraftingOpen 상태를 반전시킵니다 (true -> false, false -> true).
            isCraftingOpen = !isCraftingOpen;
            // 제작 패널을 현재 상태에 맞게 켜거나 끕니다.
            craftingPanel.SetActive(isCraftingOpen);
        }
    }

    // 제작 창 UI를 설정하는 함수
    void SetupCraftingWindow()
    {
        foreach (CraftingRecipe recipe in allRecipes)
        {
            GameObject slotGO = Instantiate(recipeSlotPrefab, recipeSlotParent);
            CraftingSlotUI slotUI = slotGO.GetComponent<CraftingSlotUI>();

            slotUI.Setup(recipe, this);
        }
    }

    // 제작을 시도하는 함수
    public void AttemptCraft(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (inventoryManager.GetItemQuantity(ingredient.itemData) < ingredient.quantity)
            {
                Debug.Log("재료 부족: " + ingredient.itemData.itemName);
                return;
            }
        }

        foreach (var ingredient in recipe.ingredients)
        {
            inventoryManager.RemoveItem(ingredient.itemData, ingredient.quantity);
        }

        inventoryManager.AddItem(recipe.resultItem, recipe.resultQuantity);

        Debug.Log(recipe.resultItem.itemName + " 제작 성공!");
    }
}
