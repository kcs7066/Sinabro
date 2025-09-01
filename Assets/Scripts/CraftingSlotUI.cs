using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlotUI : MonoBehaviour
{
    public Image resultIcon;
    public TextMeshProUGUI recipeName;
    public Button craftButton;

    private CraftingRecipe currentRecipe;
    private CraftingManager craftingManager;

    // CraftingManager가 이 슬롯을 초기 설정할 때 호출하는 함수
    public void Setup(CraftingRecipe recipe, CraftingManager manager)
    {
        currentRecipe = recipe;
        craftingManager = manager;

        resultIcon.sprite = recipe.resultItem.itemIcon;
        recipeName.text = recipe.resultItem.itemName;

        // 버튼이 클릭되면 CraftItem 함수가 실행되도록 연결
        craftButton.onClick.AddListener(CraftItem);
    }

    // 버튼이 클릭되었을 때 실행되는 함수
    void CraftItem()
    {
        if (currentRecipe != null && craftingManager != null)
        {
            // CraftingManager에게 서버로 제작 요청을 보내달라고 명령합니다.
            craftingManager.AttemptCraftServerRpc(currentRecipe.resultItem.itemID);
        }
    }
}

