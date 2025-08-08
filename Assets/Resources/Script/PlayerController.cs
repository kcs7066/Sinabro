using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 이 스크립트의 인스턴스를 저장하는 static 변수
    public static PlayerController instance;

    // 플레이어의 다른 컴포넌트들을 연결할 변수들
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private CharacterStats myStats; // 자신의 스탯 정보
    private Animator animator; // Animator 컴포넌트 참조
    private Inventory inventory; // 인벤토리 컴포넌트 참조

    private AudioSource audioSource;
    public AudioClip pickupSound; // 사운드 관련 변수 추가
    public AudioClip attackSound;

    [Header("UI 연결")]
    public GameObject lootAlertPrefab;      // 1단계에서 만든 LootAlertItem 프리팹
    public Transform lootAlertContainer;    // 2단계에서 만든 LootAlertContainer

    // --- 자동 공격 관련 변수 추가 ---
    [Header("자동 공격 설정")]
    public GameObject targetArrowPrefab;    // 1단계에서 만든 화살표 프리팹
    public float arrowYOffset = 1.0f; //  화살표 높이 조절 변수 추가
    private GameObject currentTargetArrow;  // 생성된 화살표 인스턴스
    private Transform currentTarget;        // 현재 공격 중인 대상
    private float lastAttackTime;
    private float searchInterval = 0.5f;    // 타겟을 찾는 주기 (초)
    private float lastSearchTime;
    private bool isAutoAttacking = false;   // 자동 공격 모드 On/Off

    void Awake()
    {
        // 1. instance가 아직 설정되지 않았다면 (최초의 플레이어라면)
        if (instance == null)
        {
            // instance에 자기 자신을 할당
            instance = this;
            // 씬이 전환되어도 이 게임 오브젝트를 파괴하지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        // 2. instance가 이미 존재한다면 (씬을 되돌아와서 생긴 복제품이라면)
        else
        {
            // 이 게임 오브젝트(복제품)를 파괴
            Destroy(gameObject);
        }

        // 자기 자신에게 붙어있는 다른 스크립트들을 자동으로 찾아 연결
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        myStats = GetComponent<CharacterStats>();
        animator = GetComponent<Animator>(); // Animator 연결
        inventory = GetComponent<Inventory>(); // Inventory 연결
        audioSource = GetComponent<AudioSource>(); // Audio Source 컴포넌트 가져오기
    }

    void Update()
    {
        // 테스트용: 'A' 키를 눌러 자동/수동 모드 전환
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            isAutoAttacking = !isAutoAttacking;
            if (!isAutoAttacking) // 자동 공격을 끄면 타겟 해제
            {
                currentTarget = null;
            }
            Debug.Log("자동 공격 모드: " + isAutoAttacking);
        }

        // 자동 공격 모드일 때
        if (isAutoAttacking)
        {
            // 일정 시간마다 가장 가까운 몬스터를 찾음
            if (Time.time >= lastSearchTime + searchInterval)
            {
                FindAndSetNearestMonster();
                lastSearchTime = Time.time;
            }
        }
        // 수동 모드일 때 (기존 클릭 로직)
        else
        {
            HandleManualInput();
        }

        // 타겟 추적 및 공격 처리 (자동/수동 공통)
        HandleTargeting();
        // 화살표 업데이트
        UpdateTargetArrow();
    }




        // 수동 입력 처리 함수
        void HandleManualInput()
        {
            // 1. 새로운 클릭이 발생했을 때
            if (playerInput.IsNewClick)
            {
                // 1-1. 공격 대상이 지정되었다면
                if (playerInput.CombatTarget != null)
                {
                    currentTarget = playerInput.CombatTarget;
                }
                else if (playerInput.PickupTarget != null)
                {
                    PickupItem(playerInput.PickupTarget);
                    // 아이템을 주웠을 땐 타겟을 초기화하여 불필요한 움직임을 막음
                    currentTarget = null;
                }
                // 1-2. 바닥을 클릭했다면 (이동)
                else
                {
                    currentTarget = null; // 타겟 해제
                    playerMovement.MoveTo(playerInput.TargetPosition);
                }

            }
        }

        // 타겟팅 및 공격 처리 함수
        void HandleTargeting()
        {
            // 2. 공격 대상이 있다면
            if (currentTarget != null)
            {
                // ▼▼▼ 대상의 생사 여부 확인 코드 추가 ▼▼▼
                CharacterStats targetStats = currentTarget.GetComponent<CharacterStats>();
                // 만약 타겟이 죽었다면, currentTarget을 null로 만들고 더 이상 처리하지 않음
                if (targetStats != null && targetStats.CurrentHP <= 0)
                {
                    currentTarget = null;
                    playerMovement.MoveTo(transform.position); // 제자리에 멈추기
                    return;
                }
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                float distance = Vector3.Distance(transform.position, currentTarget.position);

                // 2-1. 사거리 밖이면, 대상에게 이동
                if (distance > myStats.AttackRange)
                {
                    playerMovement.MoveTo(currentTarget.position);
                }
                // 2-2. 사거리 안이면, 이동을 멈추고 공격
                else
                {
                    // 이동 정지 (현재 위치를 목표로 설정)
                    playerMovement.MoveTo(transform.position);

                    // 공격 쿨다운이 지났다면 공격
                    if (Time.time >= lastAttackTime + myStats.AttackCooldown)
                    {
                        Attack(currentTarget);
                        lastAttackTime = Time.time;
                    }
                }
            }
        }

    // 가장 가까운 몬스터를 찾아 타겟으로 설정하는 함수
    void FindAndSetNearestMonster()
    {
        float minDistance = Mathf.Infinity;
        GameObject nearestMonster = null;
        var monsterList = MonsterManager.instance.ActiveMonsters;

        foreach (GameObject monster in monsterList)
        {
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestMonster = monster;
            }
        }

        if (nearestMonster != null)
        {
            currentTarget = nearestMonster.transform;
        }
        else
        {
            currentTarget = null; // 몬스터가 없으면 타겟 해제
        }
    }

    // 타겟 화살표를 관리하는 함수
    void UpdateTargetArrow()
    {
        if (currentTarget != null)
        {
            if (currentTargetArrow == null) // 화살표가 없다면 생성
            {
                currentTargetArrow = Instantiate(targetArrowPrefab);
            }
            // 화살표를 타겟의 자식으로 만들어 따라다니게 함
            currentTargetArrow.transform.SetParent(currentTarget, false);
            currentTargetArrow.transform.localPosition = new Vector3(0, arrowYOffset, 0); // 머리 위로 위치 조정
        }
        else
        {
            if (currentTargetArrow != null) // 타겟이 없으면 화살표 파괴
            {
                Destroy(currentTargetArrow);
            }
        }
    }


    void Attack(Transform target)
        {
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                Debug.Log($"{name}이(가) {target.name}을(를) 공격!");
                animator.SetTrigger("DoAttack");

                // ▼▼▼ 공격 사운드 재생 코드 추가 ▼▼▼
                if (attackSound != null)
                {
                    audioSource.PlayOneShot(attackSound);
                }
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                targetStats.TakeDamage(myStats.AttackPower, myStats.Level);

                // 공격한 대상이 몬스터라면, 몬스터에게 공격받았다고 알려주기
                MonsterWanderAI monsterAI = target.GetComponent<MonsterWanderAI>();
                if (monsterAI != null)
                {
                    monsterAI.OnAttacked(transform); // '나(플레이어)'를 공격자로 알림
                }
            }
        }

        void PickupItem(Transform itemTransform)
        {
            FieldItem fieldItem = itemTransform.GetComponent<FieldItem>();
            if (fieldItem != null)
            {
            // 이제 FieldItem의 공개 함수를 호출하는 역할만 합니다.
            fieldItem.BePickedUp();
        }
        }
    public void AcquireItem(ItemData itemData)
    {
        // 1. 사운드 재생
        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // 2. 인벤토리에 추가
        inventory.AddItem(itemData);

        // 3. 획득 알림 UI 표시
        if (lootAlertPrefab != null && lootAlertContainer != null)
        {
            GameObject alert = Instantiate(lootAlertPrefab, lootAlertContainer); // 알림 오브젝트를 컨테이너의 자식으로 생성
            alert.GetComponent<LootAlertItem>().Setup(itemData); // 알림 오브젝트에 아이템 정보 전달
        }

        Debug.Log($"{itemData.itemName}을(를) (자동)획득했습니다.");
    }

}