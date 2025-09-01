using UnityEngine;
using Unity.Netcode;

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

    public void FeedItem(ItemData itemData)
    {
        // --- 디버그 로그 추가 ---
        string receivedItemName = (itemData != null) ? itemData.itemName : "아무것도 없음";
        string requiredItemName = (requiredItem != null) ? requiredItem.itemName : "설정 안됨";
        Debug.Log("[서버] FeedItem 함수 실행됨. 받은 아이템: " + receivedItemName + " / 필요한 아이템: " + requiredItemName);
        // ---

        // --- 수정된 부분 ---
        // 이제 아이템 에셋 자체를 비교하는 대신, 고유한 itemID를 비교합니다.
        // 양쪽 다 null이 아니고, ID가 같은지 확인합니다.
        if (itemData != null && requiredItem != null && itemData.itemID == requiredItem.itemID)
        {
            Satisfy();
        }
        else
        {
            Debug.Log("[서버] 아이템이 일치하지 않아 아무 일도 일어나지 않았습니다.");
        }
    }

    private void Satisfy()
    {
        Debug.Log("[서버] 아이템 일치! 슬라임 만족!");
        GetComponent<NetworkObject>().Despawn();
    }
}
