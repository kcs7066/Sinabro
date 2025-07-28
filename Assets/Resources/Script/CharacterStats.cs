using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

// ▼▼▼ 아이템과 드롭률을 한 쌍으로 묶는 새로운 클래스 정의 ▼▼▼
[System.Serializable] // 이 어트리뷰트가 있어야 인스펙터에 노출됩니다.
public class DropItem
{
    public ItemData item;         // 드롭할 아이템
    [Range(0, 100)]               // 인스펙터에서 슬라이더로 조절할 수 있게 함
    public float dropChance;    // 드롭 확률 (0~100%)
}
public class CharacterStats : MonoBehaviour
{
    [Header("기본 능력치")]
    public int Level = 0;
    public int MaxHP = 100;
    public int CurrentHP = 100;
    public int AttackPower = 10;
    public float AttackRange = 1.5f; // 공격 사거리
    public float AttackCooldown = 1f; // 공격 딜레이

    [Header("드롭 아이템")]
    public GameObject fieldItemPrefab; // 1단계에서 만든 FieldItem 프리팹 연결
    public List<DropItem> dropList = new List<DropItem>();

    // ▼▼▼ 사운드 및 애니메이션 관련 변수 추가 ▼▼▼
    public AudioClip deathSound;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false; // 사망 상태 중복 실행 방지
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab; // 1단계에서 만든 DamageText 프리팹 연결
    public Transform damageTextSpawnPoint; // 데미지 텍스트가 생성될 위치

    void Awake()
    {
        // ▼▼▼ 컴포넌트 참조 초기화 ▼▼▼
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // 이미 죽었다면 피해를 받지 않음

        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        Debug.Log($"{gameObject.name}이(가) {damage} 피해를 입음. 현재 HP: {CurrentHP}");

        // --- ▼ 데미지 텍스트 생성 로직 추가 ▼ ---
        if (damageTextPrefab != null)
        {
            // 생성할 위치 결정 (지정된 위치가 없으면 캐릭터의 현재 위치)
            Vector3 spawnPos = (damageTextSpawnPoint != null) ? damageTextSpawnPoint.position : transform.position;

            // 데미지 텍스트 생성
            GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            // 생성된 텍스트에 데미지 값 전달
            damageTextObj.GetComponent<DamageNumber>().Setup(damage);
        }
        // --- ▲ 로직 끝 ▲ ---

        if (CurrentHP <= 0)
        {
            Die();

        }
    }
    private void Die()
    {
        if (isDead) return; // Die 함수가 여러 번 호출되는 것을 방지
        isDead = true;

        Debug.Log($"{gameObject.name}이(가) 쓰러졌습니다.");
        // 여기에 사망 애니메이션, 아이템 드랍, 오브젝트 파괴 등 로직 추가

        // 1. 몬스터의 다른 기능들을 정지 (AI, 충돌 등)
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<MonsterWanderAI>() != null)
        {
            GetComponent<MonsterWanderAI>().enabled = false;
        }

        // 2. 사망 애니메이션 재생
        animator.SetTrigger("IsDead");

        // 3. 사망 사운드 재생
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Debug.Log("몬스터 사망 위치: " + transform.position);

        // 4. 아이템 드롭
        if (fieldItemPrefab != null && dropList.Count > 0)
        {
            // 1. 드롭 리스트에 있는 모든 아이템을 순회합니다.
            foreach (DropItem dropItem in dropList)
            {
                // 2. 0부터 100 사이의 랜덤 숫자를 뽑습니다.
                float randomValue = Random.Range(0f, 100f);

                // 3. 랜덤 숫자가 아이템의 드롭률보다 낮거나 같으면 드롭 성공!
                if (randomValue <= dropItem.dropChance)
                {
                    // 기존과 동일하게 필드 아이템 생성
                    GameObject droppedItem = Instantiate(fieldItemPrefab, transform.position, Quaternion.identity, null);
                    droppedItem.GetComponent<FieldItem>().Setup(dropItem.item, transform.position);
                }
            }
        }

        // 5. 애니메이션과 사운드가 재생될 시간을 준 뒤, 오브젝트 파괴
        Destroy(gameObject, 2f); // 2초 뒤에 파괴 (애니메이션 길이에 맞게 조절)

        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

}