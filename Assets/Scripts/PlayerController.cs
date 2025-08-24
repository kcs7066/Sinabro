using UnityEngine;

// 이 스크립트는 플레이어의 움직임을 제어합니다.
public class PlayerController : MonoBehaviour
{
    // public으로 선언된 변수는 유니티 에디터의 Inspector 창에서 값을 직접 수정할 수 있습니다.
    public float moveSpeed = 5f; // 플레이어의 이동 속도

    private Rigidbody2D rb; // 물리 효과를 제어할 Rigidbody2D 컴포넌트
    private Vector2 movement; // 플레이어의 이동 방향을 저장할 변수

    // 게임이 시작될 때 한 번만 호출되는 함수입니다.
    void Start()
    {
        // Player 오브젝트에 붙어있는 Rigidbody2D 컴포넌트를 찾아서 rb 변수에 할당합니다.
        // 이렇게 하면 스크립트에서 물리 효과를 제어할 수 있습니다.
        rb = GetComponent<Rigidbody2D>();
    }

    // 매 프레임마다 호출되는 함수입니다. 주로 입력을 받는 데 사용됩니다.
    void Update()
    {
        // 키보드 입력(WASD 또는 방향키)을 받아서 수평(Horizontal) 및 수직(Vertical) 방향 값을 얻습니다.
        // 값은 -1에서 1 사이로 나옵니다. (예: A키를 누르면 -1, D키를 누르면 1)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    // 고정된 시간 간격으로 호출되는 함수입니다. 물리 효과를 적용할 때 사용해야 정확한 결과를 얻을 수 있습니다.
    void FixedUpdate()
    {
        // Rigidbody2D의 위치를 현재 위치에서 계산된 방향으로 이동시킵니다.
        // Time.fixedDeltaTime을 곱해주는 이유는 컴퓨터의 성능과 상관없이 항상 일정한 속도로 움직이게 하기 위함입니다.
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
