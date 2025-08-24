using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 이 스크립트는 제작 창의 각 UI 슬롯(버튼) 하나하나를 제어합니다.
public class CraftingSlotUI : MonoBehaviour
{
    // 유니티 에디터에서 연결해 줄 UI 요소들
    public Image resultIcon;
    public TextMeshProUGUI recipeName;
    public Button craftButton;

    private CraftingRecipe currentRecipe;
    private CraftingManager craftingManager; // CraftingManager를 참조할 변수

    // 이제 CraftingManager도 함께 받아서 저장합니다.
    public void Setup(CraftingRecipe recipe, CraftingManager manager)
    {
        currentRecipe = recipe;
        craftingManager = manager; // 참조 저장

        resultIcon.sprite = recipe.resultItem.itemIcon;
        recipeName.text = recipe.resultItem.itemName;

        craftButton.onClick.RemoveAllListeners(); // 혹시 모를 중복 등록을 방지하기 위해 기존 리스너를 모두 제거합니다.
        craftButton.onClick.AddListener(OnCraftButtonClick);
    }

    // 제작 버튼이 클릭되었을 때 호출될 함수
    private void OnCraftButtonClick()
    {
        // currentRecipe와 craftingManager가 모두 정상적으로 설정되었는지 확인합니다.
        if (currentRecipe != null && craftingManager != null)
        {
            // 저장해둔 CraftingManager에게 이 제작법으로 제작을 요청합니다.
            craftingManager.AttemptCraft(currentRecipe);
        }
    }
}
