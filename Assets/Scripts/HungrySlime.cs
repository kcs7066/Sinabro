using UnityEngine;
using Unity.Netcode;

public class HungrySlime : NetworkBehaviour
{
    [Header("아이템 설정")]
    public ItemData requiredItem;
    public ItemData dropItem;

    [Header("움직임 설정")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float changeDirectionInterval = 2f;

    private Rigidbody2D rb;
    private Vector2 movementDirection;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;

    // ## 추가: 슬라임이 만족했는지 여부를 동기화하는 변수 ##
    private NetworkVariable<bool> isSatisfied = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        // isSatisfied 변수의 값이 변경될 때마다 OnSatisfiedStateChanged 함수를 실행하도록 등록합니다.
        isSatisfied.OnValueChanged += OnSatisfiedStateChanged;

        // 늦게 접속한 클라이언트를 위해 현재 상태를 즉시 반영합니다.
        OnSatisfiedStateChanged(false, isSatisfied.Value);
    }

    // ## 추가: isSatisfied 상태가 변경되면 클라이언트에서 호출되어 슬라임을 숨깁니다. ##
    private void OnSatisfiedStateChanged(bool previousValue, bool newValue)
    {
        // isSatisfied가 true가 되면 (만족하면)
        if (newValue)
        {
            // 스프라이트와 콜라이더를 비활성화하여 보이지 않고 상호작용도 안 되게 합니다.
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (coll != null) coll.enabled = false;
            // 움직임도 멈춥니다.
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }


    void Update()
    {
        // 움직임 로직은 서버에서만 실행되어야 합니다. 만족한 슬라임은 움직이지 않습니다.
        if (!IsServer || isSatisfied.Value) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // 새로운 랜덤 방향 설정
            float randomAngle = Random.Range(0, 360f) * Mathf.Deg2Rad;
            movementDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            timer = changeDirectionInterval;
        }
    }

    void FixedUpdate()
    {
        // 만족한 슬라임은 움직이지 않습니다.
        if (!IsServer || isSatisfied.Value) return;
        rb.linearVelocity = movementDirection * moveSpeed;
    }

    // 이 함수는 서버에서 PlayerController에 의해 호출됩니다.
    public void FeedItem(ItemData item)
    {
        if (!IsServer) return;

        // 아이템이 일치하는지 다시 한번 확인합니다.
        if (item != null && requiredItem != null && item.itemID == requiredItem.itemID)
        {
            Debug.Log("[Server] Slime is satisfied. Setting isSatisfied to true.");

            // ## 오류 수정: Despawn() 대신 NetworkVariable을 변경하여 상태를 동기화합니다. ##
            isSatisfied.Value = true;

            // 잠시 후 서버에서만 오브젝트를 완전히 파괴하여 자원을 정리합니다.
            // 클라이언트에서는 isSatisfied.OnValueChanged에 의해 이미 사라진 것처럼 보입니다.
            Destroy(gameObject, 1f); // 1초 후 파괴
        }
    }
}

