using UnityEngine;
using Unity.Netcode;

// 이 스크립트는 플레이어의 애니메이션 상태를 제어합니다.
// Player 프리팹에 PlayerController와 함께 있어야 합니다.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : NetworkBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    // 애니메이터 파라미터의 이름을 상수로 저장하여 오타를 방지합니다.
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 이 캐릭터의 주인이 아니면 애니메이션을 제어하지 않습니다.
        if (!IsOwner) return;

        // Rigidbody의 속도(velocity) 크기를 확인하여 걷고 있는지 판단합니다.
        // 속도의 크기가 0.1보다 크면 걷는 것으로 간주합니다.
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        // Animator의 IsWalking 파라미터 값을 설정합니다.
        animator.SetBool(isWalkingHash, isMoving);
    }
}
