using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    // 이 매니저의 단 하나뿐인 인스턴스 (싱글톤)
    public static MonsterManager instance;

    [Header("스테이지 데이터")]
    public StageData currentStageData; // 씬마다 다른 StageData 파일을 연결

    [Header("몬스터 설정")]
    public List<Transform> spawnPoints; // 1단계에서 만든 스포너들의 위치


    // 현재 살아있는 몬스터들을 추적하는 리스트
    private List<GameObject> activeMonsters = new List<GameObject>();

    // ▼▼▼ 외부에서 몬스터 리스트를 읽을 수 있도록 프로퍼티 추가 ▼▼▼
    public List<GameObject> ActiveMonsters { get { return activeMonsters; } }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    void Awake()
    {
        // 싱글톤 패턴 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 몬스터 생성
        SpawnMonsters();
    }

    // 모든 스폰 포인트에 몬스터를 생성하는 함수
    void SpawnMonsters()
    { 
        // --- 안전장치: 필요한 데이터가 연결되었는지 확인 ---
        if (currentStageData == null)
        {
            Debug.LogError("MonsterManager에 StageData가 연결되지 않았습니다!");
            return;
        }
        if (currentStageData.monsterPrefab == null)
        {
            Debug.LogError(currentStageData.name + " 에셋에 몬스터 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 이전에 있던 몬스터가 남아있을 수 있으니 리스트를 비움
        activeMonsters.Clear();

        // 1. StageData로부터 생성할 몬스터의 프리팹을 가져옴
        GameObject monsterToSpawn = currentStageData.monsterPrefab;

        // 2. 생성할 몬스터 수를 결정 (단, 스폰 포인트 개수를 초과할 수 없음)
        int monsterCount = Mathf.Min(currentStageData.monsterCount, spawnPoints.Count);

        // 3. 결정된 수만큼 몬스터를 생성하는 반복문
        for (int i = 0; i < monsterCount; i++)
        {
            // i번째 스폰 포인트를 가져옴
            Transform spawnPoint = spawnPoints[i];

            // 해당 스폰 포인트의 위치에 몬스터를 생성(Instantiate)
            GameObject newMonster = Instantiate(monsterToSpawn, spawnPoint.position, Quaternion.identity);

            // 생성된 몬스터를 추적 리스트에 추가
            activeMonsters.Add(newMonster);
        }

        Debug.Log($"[{currentStageData.stageName}] {activeMonsters.Count}마리의 {monsterToSpawn.name}이(가) 생성되었습니다.");
    }

    // 몬스터가 죽었을 때 호출될 함수
    public void OnMonsterDied(GameObject deadMonster)
    {
        // 죽은 몬스터를 리스트에서 제거
        if (activeMonsters.Contains(deadMonster))
        {
            activeMonsters.Remove(deadMonster);
        }

        // 만약 살아있는 몬스터가 한 마리도 없다면
        if (activeMonsters.Count == 0)
        {
            // 3초 뒤에 리젠 시퀀스 시작
            StartCoroutine(RespawnSequence());
        }
    }

    // 리젠 시퀀스 코루틴
    IEnumerator RespawnSequence()
    {
        Debug.Log("모든 몬스터가 사망했습니다. 3초 뒤에 리젠됩니다.");
        yield return new WaitForSeconds(3f);
        SpawnMonsters();
    }
}