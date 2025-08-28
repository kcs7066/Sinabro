using UnityEngine;
using Unity.Netcode;

// 이제 슬라임도 NetworkBehaviour를 상속받습니다.
public class HungrySlime : NetworkBehaviour
{
    [Header("슬라임 설정")]
    public float moveSpeed = 1f;
    public float moveInterval = 2f;
    public ItemData requiredItem;
    public ItemData dropItem;

    private Rigidbody2D rb;
    private float moveTimer;
    private Vector2 moveDirection;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        moveTimer = moveInterval;
    }

    // 움직임 로직은 서버에서만 실행되어야 합니다.
    void Update()
    {
        if (!IsServer) return;

        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0)
        {
            float randomAngle = Random.Range(0f, 360f);
            moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            moveTimer = moveInterval;
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // 이 함수는 서버에서만 호출됩니다 (PlayerController의 ServerRpc를 통해).
    public void FeedItem(ItemData itemData)
    {
        if (itemData == requiredItem)
        {
            Satisfy();
        }
    }

    private void Satisfy()
    {
        Debug.Log("슬라임 만족!");
        // TODO: 아이템 드랍 로직 (이것도 서버에서만 실행되어야 함)

        // 네트워크 오브젝트를 파괴할 때는 Destroy 대신 Despawn을 사용해야 합니다.
        GetComponent<NetworkObject>().Despawn();
    }
}
