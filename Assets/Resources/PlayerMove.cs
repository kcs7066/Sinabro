using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private Animator animator;

    void Start()
    {
        targetPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 마우스 클릭 감지 (새 Input System)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            clickWorldPosition.z = 0f;
            targetPosition = clickWorldPosition;
            isMoving = true;

           
        }

        if (isMoving)
        {

            // 방향 벡터 계산
            Vector3 direction = targetPosition - transform.position;

            // x방향에 따라 반전
            if (direction.x > 0)
            {
                // 오른쪽
                transform.localScale = new Vector3(-2, 2, 1);
            }
            else if (direction.x < 0)
            {
                // 왼쪽
                transform.localScale = new Vector3(2, 2, 1);
            }

            // 위치 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalk", true);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                animator.SetBool("IsWalk", false);
            }
        }
        else
        {
            animator.SetBool("IsWalk", false);
        }    
    }
}
