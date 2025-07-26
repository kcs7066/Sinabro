// MonsterSpawner.cs 예시 코드
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab; // 스폰할 몬스터 프리팹
    public int numberOfMonsters;     // 스폰할 몬스터 수
    public float spawnRadius;        // 스폰 반경

    void Start()
    {
        for (int i = 0; i < numberOfMonsters; i++)
        {
            SpawnMonster();
        }
    }

    void SpawnMonster()
    {
        // 중심 위치에서 랜덤한 위치에 몬스터 생성
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
    }
}