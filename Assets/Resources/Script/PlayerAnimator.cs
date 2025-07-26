using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // PlayerMovement의 상태에 따라 'IsWalk' 파라미터 제어
        animator.SetBool("IsWalk", playerMovement.IsMoving);

        // 이동 방향에 따라 캐릭터 좌우 반전
        Vector3 direction = playerMovement.TargetPosition - transform.position;
        if (direction.x > 0.01f) // 오른쪽
        {
            transform.localScale = new Vector3(-2, 2, 1); // X축 스케일만 -1로
        }
        else if (direction.x < -0.01f) // 왼쪽
        {
            transform.localScale = new Vector3(2, 2, 1); // 기본값
        }
    }
}