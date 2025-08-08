using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonUIController : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public GameObject stageListPanel;   // 스테이지 목록 패널
    public Image monsterPreviewImage;   // 몬스터 이미지
    public Transform dropItemParent;    // 드롭 아이템 아이콘이 생성될 부모
    public GameObject itemIconPrefab;   // 아이템 아이콘으로 사용할 UI Image 프리팹

    private StageData currentSelectedStage; // 현재 선택된 스테이지 정보

    // 1. 스테이지 선택 버튼을 누르면 목록을 켜고 끄는 함수
    public void ToggleStageList()
    {
        stageListPanel.SetActive(!stageListPanel.activeSelf);
    }

    // 2. 특정 스테이지 버튼을 눌렀을 때 UI를 업데이트하는 함수
    public void OnStageSelected(StageData stageData)
    {
        currentSelectedStage = stageData;
        monsterPreviewImage.sprite = stageData.monsterSprite;

        // 기존 드롭 아이템 아이콘들 삭제
        foreach (Transform child in dropItemParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 드롭 아이템 아이콘들 생성
        foreach (ItemData item in stageData.dropList)
        {
            // 1. 프리팹 생성
            GameObject slot = Instantiate(itemIconPrefab, dropItemParent);

            // 2. 아이콘 설정 (자식 오브젝트에서 Image 컴포넌트를 찾음)
            Image iconImage = slot.transform.Find("ItemIcon").GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = item.itemIcon;
            }

            // 3. 텍스트 설정 (자식 오브젝트에서 TextMeshProUGUI 컴포넌트를 찾음)
            TextMeshProUGUI nameText = slot.transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = item.itemName;
            }
        }
    }

    // 3. "입장하기" 버튼을 눌렀을 때 해당 스테이지 씬으로 이동하는 함수
    public void EnterStage()
    {
        if (currentSelectedStage != null)
        {
            ClosePanel();
            SceneManager.LoadScene(currentSelectedStage.sceneToLoad);
        }
        else
        {
            Debug.Log("스테이지가 선택되지 않았습니다.");
        }
    }

    // 4. 뒤로가기 버튼 함수 (이 패널을 끔)
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}