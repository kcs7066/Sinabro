// PlayerInput.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    public Vector3 TargetPosition { get; private set; }
    public Transform CombatTarget { get; private set; } // 공격 대상을 저장할 변수
    public Transform PickupTarget { get; private set; } // 주울 아이템을 저장할 변수
    public bool IsNewClick { get; private set; }

    void Update()
    {
        IsNewClick = false;
        PickupTarget = null;
        CombatTarget = null; // 매 프레임 초기화

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // ▼▼▼ UI 클릭 방지 코드 추가 ▼▼▼
            // 현재 마우스 포인터가 UI 요소 위에 있다면, 아무것도 하지 않고 함수를 즉시 종료
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
            IsNewClick = true;

            // 1. 레이캐스트로 클릭한 지점의 오브젝트 확인
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                // 1. 몬스터를 클릭했다면
                if (hit.collider.CompareTag("Monster"))
                {
                    CombatTarget = hit.transform;
                    return;
                }
                // 2. 아이템을 클릭했다면
                else if (hit.collider.CompareTag("Item"))
                {
                    PickupTarget = hit.transform;
                    return;
                }
                // 3. 바닥을 클릭했다면

            }
         
            
                // 1. 임시 변수에 월드 좌표를 저장합니다.
                Vector3 tempPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                // 2. 임시 변수의 z값을 0으로 만듭니다.
                tempPosition.z = 0f;
                // 3. 수정된 임시 변수를 TargetPosition에 할당합니다.
                TargetPosition = tempPosition;
            
        }
    }
}