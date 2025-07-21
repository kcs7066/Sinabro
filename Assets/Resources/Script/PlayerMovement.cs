using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public bool IsMoving { get; private set; } // 애니메이터가 참고할 상태
    public Vector3 TargetPosition { get; private set; }

    void Start()
    {
        // 시작할 땐 움직이지 않으므로 현재 위치를 목표로 설정
        TargetPosition = transform.position;
    }

    void Update()
    {
        // 목표 위치와 현재 위치의 거리가 0.01보다 크면 움직이는 중
        if (Vector3.Distance(transform.position, TargetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, moveSpeed * Time.deltaTime);
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }

    // PlayerController가 호출할 공개 함수
    public void MoveTo(Vector3 destination)
    {
        TargetPosition = destination;
    }
}