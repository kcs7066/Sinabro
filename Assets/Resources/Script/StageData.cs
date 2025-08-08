using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stage", menuName = "Dungeon/StageData")]
public class StageData : ScriptableObject
{
    public string stageName;        // 스테이지 이름 (예: "1-1 고요한 숲")
    public string sceneToLoad;      // 이동할 씬의 이름
    public Sprite monsterSprite;    // 보여줄 몬스터 이미지
    public List<ItemData> dropList; // 드롭 아이템 목록

    public GameObject monsterPrefab; // 이 스테이지에 등장할 몬스터 프리팹
    public int monsterCount;     // 등장할 몬스터 수
}