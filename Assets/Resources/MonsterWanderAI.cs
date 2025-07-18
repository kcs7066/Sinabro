using UnityEngine;

public class MonsterWanderAI : MonoBehaviour
{
    // 몬스터의 상태 정의
    public enum MonsterState { Wander, ChaseAndAttack }
    public MonsterState currentState = MonsterState.Wander;

    [Header("능력치 및 이동")]
    public float moveSpeed = 2f;

    [Header("배회 관련")]
    public float wanderRadius = 5f;
    public float waitTime = 3f;

    [Header("맵 경계 여백")]
    public float padding = 1.0f;

    // --- 내부 변수들 ---
    private CharacterStats myStats;
    private Transform playerTarget;
    private float lastAttackTime;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float waitTimer;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    private Animator animator; // Animator 컴포넌트 참조


    void Start()
    {
        myStats = GetComponent<CharacterStats>();
        animator = GetComponent<Animator>(); // Animator 연결
        startPosition = transform.position;

        // --- ▼ 맵 경계 계산 코드 다시 추가 ▼ ---
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCamera.transform.position.z)));
        maxBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Mathf.Abs(mainCamera.transform.position.z)));

        minBounds.x += padding;
        minBounds.y += padding;
        maxBounds.x -= padding;
        maxBounds.y -= padding;
        // --- ▲ 경계 계산 끝 ▲ ---

        SetNewRandomDestination();
    }

    void Update()
    {
        // 상태에 따라 다른 행동 실행
        switch (currentState)
        {
            case MonsterState.Wander:
                Wander();
                break;
            case MonsterState.ChaseAndAttack:
                ChaseAndAttack();
                break;
        }

        // --- ▼ 이동 후, 최종 위치를 경계 안으로 제한 ▼ ---
        // 어떤 상태이든, 이동이 끝난 후에는 항상 몬스터의 위치를 경계 안에 있도록 강제합니다.
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        transform.position = clampedPosition;
        // --- ▲ 최종 위치 제한 끝 ▲ ---
    }

    // 평소: 배회하기
    void Wander()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                SetNewRandomDestination();
                waitTimer = waitTime;
            }
        }
    }

    // 전투 중: 추적 및 공격
    void ChaseAndAttack()
    {
        if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            currentState = MonsterState.Wander;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance > myStats.AttackRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            if (Time.time >= lastAttackTime + myStats.AttackCooldown)
            {
                animator.SetTrigger("DoAttack");
                Debug.Log($"{name}이(가) {playerTarget.name}을(를) 반격!");
                playerTarget.GetComponent<CharacterStats>().TakeDamage(myStats.AttackPower);
                lastAttackTime = Time.time;
            }
        }
    }

    // 외부에서 호출하여 전투 상태로 전환하는 함수
    public void OnAttacked(Transform attacker)
    {
        playerTarget = attacker;
        currentState = MonsterState.ChaseAndAttack;
    }

    // 새로운 배회 목적지 설정 (경계 체크 포함)
    void SetNewRandomDestination()
    {
        Vector2 randomPos = startPosition + Random.insideUnitCircle * wanderRadius;

        // 목표 지점 자체도 경계 안으로 제한
        float clampedX = Mathf.Clamp(randomPos.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(randomPos.y, minBounds.y, maxBounds.y);

        targetPosition = new Vector2(clampedX, clampedY);
    }
}