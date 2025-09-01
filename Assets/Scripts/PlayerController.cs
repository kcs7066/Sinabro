using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    public float moveSpeed = 5f;
    public float interactionDistance = 1.5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private PlayerEquipment playerEquipment;
    private TileManager tileManager; // TileManager 참조

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        playerEquipment = GetComponent<PlayerEquipment>();
        tileManager = FindObjectOfType<TileManager>(); // 씬에서 TileManager를 찾습니다.

        if (IsOwner)
        {
            LocalInstance = this;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        // 마우스 클릭으로 타일 파괴를 요청합니다.
        if (Input.GetMouseButtonDown(0))
        {
            tileManager.RequestDamageTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        MoveServerRpc(movementInput);
    }

    private void Interact()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionDistance);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<HungrySlime>(out HungrySlime slime))
            {
                ItemData heldItem = playerEquipment.equippedItemSlot?.itemData;
                int heldItemID = (heldItem != null) ? heldItem.itemID : -1;

                FeedSlimeServerRpc(slime.GetComponent<NetworkObject>().NetworkObjectId, heldItemID);
                break;
            }
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input)
    {
        rb.linearVelocity = input.normalized * moveSpeed;
    }

    [ServerRpc(RequireOwnership = false)]
    private void FeedSlimeServerRpc(ulong slimeNetworkId, int heldItemID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(slimeNetworkId, out NetworkObject slimeObject))
        {
            if (slimeObject.TryGetComponent<HungrySlime>(out HungrySlime slime))
            {
                ItemData itemToFeed = ItemDatabase.Instance.GetItemById(heldItemID);
                slime.FeedItem(itemToFeed);
            }
        }
    }
}
