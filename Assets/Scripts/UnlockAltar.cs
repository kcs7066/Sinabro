using UnityEngine;
using Unity.Netcode;

public class UnlockAltar : NetworkBehaviour
{
    [Header("해금 조건")]
    public ItemData requiredItem;
    public int requiredAmount = 1;

    [Header("해금 대상")]
    public Vector2Int chunkToUnlockOffset = new Vector2Int(1, 0);

    private NetworkVariable<bool> isUsed = new NetworkVariable<bool>(false);
    private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isUsed.OnValueChanged += OnUsedStateChanged;
        OnUsedStateChanged(false, isUsed.Value);
    }

    private void OnUsedStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            spriteRenderer.color = Color.gray;
        }
    }

    public void AttemptUnlock(PlayerInventory playerInventory)
    {
        if (!IsServer || isUsed.Value) return;

        if (playerInventory.GetItemQuantity(requiredItem.itemID) >= requiredAmount)
        {
            playerInventory.RemoveItem(requiredItem.itemID, requiredAmount);
            isUsed.Value = true;

            // 1. 제단의 청크 위치를 찾습니다.
            Vector2Int altarChunkPos = WorldManager.Instance.GetChunkPositionFromWorld(transform.position);
            // 2. 오프셋을 더해 해금할 청크의 위치를 찾습니다.
            Vector2Int chunkToUnlockPos = altarChunkPos + chunkToUnlockOffset;
            // 3. 해당 위치의 Land ID를 찾습니다.
            string landIdToUnlock = WorldManager.Instance.GetLandIdAt(chunkToUnlockPos);

            // 4. 유효한 Land ID를 찾았다면, 해금을 요청합니다.
            if (!string.IsNullOrEmpty(landIdToUnlock))
            {
                WorldManager.Instance.UnlockLand(landIdToUnlock);
            }
        }
    }
}

