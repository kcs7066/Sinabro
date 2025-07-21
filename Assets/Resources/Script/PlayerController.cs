using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이 스크립트의 인스턴스를 저장하는 static 변수
    public static PlayerController instance;

    // 플레이어의 다른 컴포넌트들을 연결할 변수들
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private CharacterStats myStats; // 자신의 스탯 정보
    private Animator animator; // Animator 컴포넌트 참조

    private Transform currentTarget; // 현재 추적/공격 중인 대상
    private float lastAttackTime; // 마지막 공격 시간


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
    }

    void Update()
    {
        // 1. 새로운 클릭이 발생했을 때
        if (playerInput.IsNewClick)
        {
            // 1-1. 공격 대상이 지정되었다면
            if (playerInput.CombatTarget != null)
            {
                currentTarget = playerInput.CombatTarget;
            }
            // 1-2. 바닥을 클릭했다면 (이동)
            else
            {
                currentTarget = null; // 타겟 해제
                playerMovement.MoveTo(playerInput.TargetPosition);
            }
        }

        // 2. 공격 대상이 있다면
        if (currentTarget != null)
        {
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

        void Attack(Transform target)
        {
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                Debug.Log($"{name}이(가) {target.name}을(를) 공격!");
                animator.SetTrigger("DoAttack");
                targetStats.TakeDamage(myStats.AttackPower);

                // 공격한 대상이 몬스터라면, 몬스터에게 공격받았다고 알려주기
                MonsterWanderAI monsterAI = target.GetComponent<MonsterWanderAI>();
                if (monsterAI != null)
                {
                    monsterAI.OnAttacked(transform); // '나(플레이어)'를 공격자로 알림
                }
            }
        }


    }
}