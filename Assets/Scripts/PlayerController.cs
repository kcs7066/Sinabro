using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI; // UI.Button을 사용하기 위해 반드시 필요합니다.

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public float interactionDistance = 1.5f;

    [Header("Test Items")]
    public ItemData testMushroom;
    public ItemData testAxe;

    [Header("UI References")]
    public GameObject unlockPanel; // Inspector에서 연결할 해금 UI 패널
    public Button unlockButton; // Inspector에서 연결할 해금 버튼

    private Rigidbody2D rb;
    private Vector2 clientInput;
    private WorldManager worldManager;
    private string targetLandIdToUnlock;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        if (IsOwner)
        {
            LocalInstance = this;
            worldManager = FindFirstObjectByType<WorldManager>();

            // UI 연결 및 초기화
            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockButtonPressed);
            }
            if (unlockPanel != null)
            {
                unlockPanel.SetActive(false);
            }
        }

        // 플레이어 시작 위치 설정 (서버에서만 실행)
        if (IsServer && worldManager != null)
        {
            transform.position = new Vector3(worldManager.chunkSize / 2f, worldManager.chunkSize / 2f, 0);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // --- Input Reading ---
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckForUnlockableLand();
        }

        // --- Test Item Cheats ---
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

    // --- Server RPCs ---
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

    [ServerRpc]
    private void FeedSlimeServerRpc(ulong slimeNetworkId, int heldItemID, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client)) return;
        if (!client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory)) return;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(slimeNetworkId, out var slimeObject)) return;

        if (slimeObject.TryGetComponent<HungrySlime>(out var slime))
        {
            ItemData heldItem = ItemDatabase.Instance.GetItemById(heldItemID);
            if (heldItem != null && slime.requiredItem != null && heldItem.itemID == slime.requiredItem.itemID)
            {
                playerInventory.RemoveItem(heldItemID, 1);
                if (slime.dropItem != null)
                {
                    playerInventory.AddItem(slime.dropItem.itemID, 1);
                }
                slime.FeedItem(heldItem);
            }
        }
    }

    [ServerRpc]
    private void UnlockAltarServerRpc(ulong altarNetworkId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client)) return;
        if (!client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory)) return;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(altarNetworkId, out var altarObject)) return;

        if (altarObject.TryGetComponent<UnlockAltar>(out var altar))
        {
            altar.AttemptUnlock(playerInventory);
        }
    }

    [ServerRpc]
    private void RequestUnlockLandServerRpc(string landId)
    {
        worldManager?.UnlockLand(landId);
    }

    // --- Local Functions ---
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
                    FeedSlimeServerRpc(slime.NetworkObjectId, playerEquipment.equippedItemSlot.itemID);
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

    private void CheckForUnlockableLand()
    {
        if (worldManager == null) return;

        Vector2Int currentChunk = worldManager.GetChunkPositionFromWorld(transform.position);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int adjacentChunk = currentChunk + dir;
            string landId = worldManager.GetLandIdAt(adjacentChunk);
            if (!string.IsNullOrEmpty(landId) && !worldManager.IsLandUnlocked(landId))
            {
                targetLandIdToUnlock = landId;
                if (unlockPanel != null) unlockPanel.SetActive(true);
                return;
            }
        }
    }

    private void OnUnlockButtonPressed()
    {
        if (!string.IsNullOrEmpty(targetLandIdToUnlock))
        {
            RequestUnlockLandServerRpc(targetLandIdToUnlock);
            if (unlockPanel != null) unlockPanel.SetActive(false);
        }
    }
}

