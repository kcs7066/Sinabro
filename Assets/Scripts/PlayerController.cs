using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public float interactionDistance = 1.5f;

    private Rigidbody2D rb;
    private Vector2 clientInput;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        if (IsOwner)
        {
            LocalInstance = this;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        clientInput.x = Input.GetAxisRaw("Horizontal");
        clientInput.y = Input.GetAxisRaw("Vertical");

        MoveServerRpc(clientInput);

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RequestTileDamageServerRpc(mouseWorldPos);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input)
    {
        Vector2 moveVelocity = input.normalized * moveSpeed;
        rb.linearVelocity = moveVelocity;
    }

    // ## 수정: ServerRpcParams를 받아 요청을 보낸 클라이언트의 ID를 전달합니다. ##
    [ServerRpc]
    private void RequestTileDamageServerRpc(Vector3 worldPosition, ServerRpcParams rpcParams = default)
    {
        // 서버에 있는 TileManager를 찾아, 요청을 보낸 클라이언트의 ID와 함께 타일 파괴 처리를 요청합니다.
        FindFirstObjectByType<TileManager>()?.ProcessTileDamage(worldPosition, rpcParams.Receive.SenderClientId);
    }

    void Interact()
    {
        if (!IsOwner) return;

        PlayerEquipment playerEquipment = GetComponent<PlayerEquipment>();
        if (playerEquipment == null || playerEquipment.equippedItemSlot.itemID == 0) return;

        int heldItemID = playerEquipment.equippedItemSlot.itemID;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionDistance);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<HungrySlime>(out var slime))
            {
                FeedSlimeServerRpc(slime.NetworkObjectId, heldItemID);
                return;
            }
        }
    }

    [ServerRpc]
    private void FeedSlimeServerRpc(ulong slimeNetworkId, int heldItemID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(slimeNetworkId, out var slimeObject))
        {
            if (slimeObject.TryGetComponent<HungrySlime>(out var slime))
            {
                ItemData heldItem = ItemDatabase.Instance.GetItemById(heldItemID);
                slime.FeedItem(heldItem);
            }
        }
    }
}

