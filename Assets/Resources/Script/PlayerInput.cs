// PlayerInput.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector3 TargetPosition { get; private set; }
    public Transform CombatTarget { get; private set; } // 공격 대상을 저장할 변수
    public bool IsNewClick { get; private set; }

    void Update()
    {
        IsNewClick = false;
        CombatTarget = null; // 매 프레임 초기화

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 1. 레이캐스트로 클릭한 지점의 오브젝트 확인
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            // 2. 콜라이더에 맞았고, 그게 'Monster' 태그라면
            if (hit.collider != null && hit.collider.CompareTag("Monster"))
            {
                CombatTarget = hit.transform; // 공격 대상으로 설정
            }
            // 3. 몬스터가 아닌 다른 곳을 클릭했다면
            else
            {
                // 1. 임시 변수에 월드 좌표를 저장합니다.
                Vector3 tempPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                // 2. 임시 변수의 z값을 0으로 만듭니다.
                tempPosition.z = 0f;
                // 3. 수정된 임시 변수를 TargetPosition에 할당합니다.
                TargetPosition = tempPosition;
            }
            IsNewClick = true;
        }
    }
}