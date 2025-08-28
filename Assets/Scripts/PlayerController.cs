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
    private ItemDatabase itemDatabase; // 아이템 DB 참조

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        playerEquipment = GetComponent<PlayerEquipment>();
        itemDatabase = FindObjectOfType<ItemDatabase>(); // 씬에서 ItemDatabase를 찾습니다.

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

                // 아이템이 있으면 ID를, 없으면 -1을 보냅니다.
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

    // 이제 ItemData 대신 int (itemID)를 받습니다.
    [ServerRpc(RequireOwnership = false)]
    private void FeedSlimeServerRpc(ulong slimeNetworkId, int heldItemID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(slimeNetworkId, out NetworkObject slimeObject))
        {
            if (slimeObject.TryGetComponent<HungrySlime>(out HungrySlime slime))
            {
                // 서버에서 ID를 이용해 실제 ItemData를 찾아옵니다.
                ItemData itemToFeed = itemDatabase.GetItemById(heldItemID);
                slime.FeedItem(itemToFeed);
            }
        }
    }
}
