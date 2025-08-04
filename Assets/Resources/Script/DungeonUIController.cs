using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
            GameObject icon = Instantiate(itemIconPrefab, dropItemParent);
            icon.GetComponent<Image>().sprite = item.itemIcon;
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