using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CraftingManager : NetworkBehaviour
{
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    public GameObject craftingPanel;
    public GameObject recipeSlotPrefab;
    public Transform recipeSlotParent;

    void Start()
    {
        craftingPanel.SetActive(false);
        PopulateCraftingUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            craftingPanel.SetActive(!craftingPanel.activeSelf);
        }
    }

    // 제작 UI를 채우는 함수 (시작 시 한 번만 실행)
    void PopulateCraftingUI()
    {
        foreach (var recipe in allRecipes)
        {
            GameObject slotGO = Instantiate(recipeSlotPrefab, recipeSlotParent);
            CraftingSlotUI slotUI = slotGO.GetComponent<CraftingSlotUI>();
            if (slotUI != null)
            {
                // CraftingManager 자신을 슬롯에 알려줍니다.
                slotUI.Setup(recipe, this);
            }
        }
    }

    // 클라이언트가 제작을 요청하면 서버에서 실행되는 함수
    [ServerRpc(RequireOwnership = false)]
    public void AttemptCraftServerRpc(int resultItemID, ServerRpcParams rpcParams = default)
    {
        CraftingRecipe recipeToCraft = FindRecipeByResultID(resultItemID);
        if (recipeToCraft == null) return;

        // 요청을 보낸 플레이어를 찾습니다.
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client))
            return;

        if (!client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory))
            return;

        // 1. 재료가 충분한지 서버에서 확인합니다.
        bool canCraft = true;
        foreach (var ingredient in recipeToCraft.ingredients)
        {
            if (playerInventory.GetItemQuantity(ingredient.itemData.itemID) < ingredient.quantity)
            {
                canCraft = false;
                break;
            }
        }

        // 2. 재료가 충분하면, 재료를 제거하고 결과물을 추가합니다.
        if (canCraft)
        {
            foreach (var ingredient in recipeToCraft.ingredients)
            {
                // [ServerRpc]가 아닌 일반 함수를 호출합니다.
                playerInventory.RemoveItem(ingredient.itemData.itemID, ingredient.quantity);
            }
            // [ServerRpc]가 아닌 일반 함수를 호출합니다.
            playerInventory.AddItem(recipeToCraft.resultItem.itemID, recipeToCraft.resultQuantity);
        }
    }

    // 아이템 ID로 제작법을 찾는 함수
    private CraftingRecipe FindRecipeByResultID(int id)
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe.resultItem.itemID == id)
            {
                return recipe;
            }
        }
        return null;
    }
}

