using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public float interactionDistance = 1.5f;

    [Header("Test Items")]
    public ItemData testMushroom;
    public ItemData testAxe;

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

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (testMushroom != null)
            {
                RequestAddItemServerRpc(testMushroom.itemID);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (testAxe != null)
            {
                RequestAddItemServerRpc(testAxe.itemID);
            }
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input)
    {
        Vector2 moveVelocity = input.normalized * moveSpeed;
        rb.linearVelocity = moveVelocity;
    }

    [ServerRpc]
    private void RequestTileDamageServerRpc(Vector3 worldPosition, ServerRpcParams rpcParams = default)
    {
        FindFirstObjectByType<TileManager>()?.ProcessTileDamage(worldPosition, rpcParams.Receive.SenderClientId);
    }

    [ServerRpc]
    private void RequestAddItemServerRpc(int itemID)
    {
        if (TryGetComponent<PlayerInventory>(out var playerInventory))
        {
            playerInventory.AddItem(itemID, 1);
        }
    }

    void Interact()
    {
        if (!IsOwner) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionDistance);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<HungrySlime>(out var slime))
            {
                PlayerEquipment playerEquipment = GetComponent<PlayerEquipment>();
                if (playerEquipment != null && playerEquipment.equippedItemSlot.itemID != 0)
                {
                    int heldItemID = playerEquipment.equippedItemSlot.itemID;
                    FeedSlimeServerRpc(slime.NetworkObjectId, heldItemID);
                    return;
                }
            }
            else if (hit.TryGetComponent<UnlockAltar>(out var altar))
            {
                UnlockAltarServerRpc(altar.NetworkObjectId);
                return;
            }
        }
    }

    [ServerRpc]
    private void FeedSlimeServerRpc(ulong slimeNetworkId, int heldItemID, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client))
            return;

        if (!client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory))
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(slimeNetworkId, out var slimeObject))
        {
            if (slimeObject.TryGetComponent<HungrySlime>(out var slime))
            {
                ItemData heldItem = ItemDatabase.Instance.GetItemById(heldItemID);

                if (heldItem != null && slime.requiredItem != null && heldItem.itemID == slime.requiredItem.itemID)
                {
                    // 플레이어의 인벤토리에서 아이템 1개를 먼저 제거합니다.
                    playerInventory.RemoveItem(heldItemID, 1);

                    // ## 추가: 슬라임이 드랍하는 보상 아이템을 플레이어에게 줍니다. ##
                    if (slime.dropItem != null)
                    {
                        playerInventory.AddItem(slime.dropItem.itemID, 1);
                    }

                    // 마지막으로 슬라임에게 아이템을 먹입니다. (슬라임이 사라짐)
                    slime.FeedItem(heldItem);
                }
            }
        }
    }

    [ServerRpc]
    private void UnlockAltarServerRpc(ulong altarNetworkId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client))
            return;

        if (!client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory))
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(altarNetworkId, out var altarObject))
            return;

        if (altarObject.TryGetComponent<UnlockAltar>(out var altar))
        {
            altar.AttemptUnlock(playerInventory);
        }
    }
}

