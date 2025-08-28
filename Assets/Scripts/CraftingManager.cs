using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 게임의 전체 제작 시스템을 총괄합니다.
public class CraftingManager : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public List<CraftingRecipe> allRecipes;
    public GameObject craftingPanel;
    public Transform recipeSlotParent;
    public GameObject recipeSlotPrefab;

    private bool isCraftingOpen = false;

    void Start()
    {
        SetupCraftingWindow();
        craftingPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCraftingOpen = !isCraftingOpen;
            craftingPanel.SetActive(isCraftingOpen);
        }
    }

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
        // --- 추가된 방어 코드 ---
        // 제작법이나 결과 아이템이 설정되지 않았으면 오류를 출력하고 중단합니다.
        if (recipe == null || recipe.resultItem == null)
        {
            Debug.LogError("오류: 제작하려는 Recipe 또는 Result Item이 Inspector에 설정되지 않았습니다!");
            return;
        }
        // ---

        // 1. 재료 확인
        foreach (var ingredient in recipe.ingredients)
        {
            if (inventoryManager.GetItemQuantity(ingredient.itemData) < ingredient.quantity)
            {
                Debug.Log("재료 부족: " + ingredient.itemData.itemName);
                return;
            }
        }

        // 2. 재료 소모 (UI 업데이트 없이)
        foreach (var ingredient in recipe.ingredients)
        {
            inventoryManager.RemoveItem(ingredient.itemData, ingredient.quantity, false);
        }

        // 3. 결과 아이템 추가 (UI 업데이트 없이)
        inventoryManager.AddItem(recipe.resultItem, recipe.resultQuantity, false);

        // 4. 모든 작업이 끝난 후 UI를 한 번만 업데이트하여 효율을 높입니다.
        inventoryManager.UpdateInventoryUI();

        Debug.Log(recipe.resultItem.itemName + " 제작 성공!");
    }
}
