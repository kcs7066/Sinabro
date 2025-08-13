using System.Collections.Generic;
using UnityEngine;

// ▼▼▼ 아이템과 드롭률을 한 쌍으로 묶는 새로운 클래스 정의 ▼▼▼
[System.Serializable] // 이 어트리뷰트가 있어야 인스펙터에 노출됩니다.
public class DropItem
{
    public ItemData item;         // 드롭할 아이템
    [Range(0, 100)]               // 인스펙터에서 슬라이더로 조절할 수 있게 함
    public float dropChance;    // 드롭 확률 (0~100%)
}
public class MonsterStats : CharacterStats
{
    [Header("몬스터 고유 능력치")]
    public int AttackPower = 10;
    public float AttackRange = 1.5f;    
    public float AttackCooldown = 2f; 

    [Header("드롭 아이템")]
    public GameObject fieldItemPrefab;
    public List<DropItem> dropList = new List<DropItem>(); 
    public float itemDropSpread = 0.5f; //  아이템이 퍼지는 반경 변수 추가 

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab;
    public Transform damageTextSpawnPoint; // 데미지 텍스트가 생성될 위치

    [Header("사운드 및 애니메이션")]
    public AudioClip deathSound;
    private Animator animator;
    private AudioSource audioSource;
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        CurrentHP = MaxHP;
    }
    // 몬스터는 데미지를 입을 때 텍스트를 띄워야 하므로 TakeDamage를 재정의
    public override void TakeDamage(int baseDamage, int attackerLevel)
    {
        // 먼저 부모의 TakeDamage 로직을 그대로 실행 (체력 계산 등)
        base.TakeDamage(baseDamage, attackerLevel);

        // 몬스터만의 추가 기능: 데미지 텍스트 생성
        if (damageTextPrefab != null && !isDead)
        {
            // 데미지 계산은 부모 클래스에서 이미 했지만, 최종 데미지를 다시 계산해야 함
            int levelDifference = attackerLevel - this.Level;
            float damageModifier = 1.0f + (levelDifference * 0.1f);
            if (levelDifference <= -10) { damageModifier = 0f; }
            else if (levelDifference >= 10) { damageModifier = 2f; }
            int finalDamage = Mathf.RoundToInt(baseDamage * damageModifier);

            // 생성할 위치 결정 (지정된 위치가 없으면 캐릭터의 현재 위치)
            Vector3 spawnPos = (damageTextSpawnPoint != null) ? damageTextSpawnPoint.position : transform.position;
            // 데미지 텍스트 생성
            GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            // 생성된 텍스트에 데미지 값 전달
            damageTextObj.GetComponent<DamageNumber>().Setup(finalDamage);
        }


    }

    // 몬스터의 사망 로직 (부모 함수 재정의)
    protected override void Die()
    {
        // 부모의 Die 로직(isDead=true 설정 등)을 먼저 실행
        base.Die();

        // 몬스터 매니저에게 사망 사실 알리기
        if (gameObject.CompareTag("Monster"))
        {
            MonsterManager.instance.OnMonsterDied(gameObject);
        }

        // 몬스터의 다른 기능들을 정지
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        if (GetComponent<MonsterWanderAI>() != null)
        {
            GetComponent<MonsterWanderAI>().enabled = false;
        }

        // 사망 애니메이션 및 사운드 재생
        animator.SetTrigger("IsDead");
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 아이템 드롭
        if (fieldItemPrefab != null && dropList.Count > 0)
        {
            // 1. 드롭 리스트에 있는 모든 아이템을 순회합니다.
            foreach (DropItem dropItem in dropList)
            {
                // 3. 랜덤 숫자가 아이템의 드롭률보다 낮거나 같으면 드롭 성공!
                if (Random.Range(0f, 100f) <= dropItem.dropChance)
                {
                    // 1. 몬스터 위치를 기준으로 무작위 오프셋 계산
                    // Random.insideUnitCircle은 반지름 1짜리 원 안의 랜덤한 위치를 반환합니다.
                    Vector2 randomOffset = Random.insideUnitCircle * itemDropSpread;
                    Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                    // 2. 계산된 위치에 아이템 생성
                    GameObject droppedItem = Instantiate(fieldItemPrefab, spawnPosition, Quaternion.identity, null);
                    // 3. Setup 함수에도 동일한 최종 위치를 전달
                    droppedItem.GetComponent<FieldItem>().Setup(dropItem.item, spawnPosition);
                }
            }
        }

        // 오브젝트 파괴
        Destroy(gameObject, 2f);// 2초 뒤에 파괴 (애니메이션 길이에 맞게 조절)
    }

}
