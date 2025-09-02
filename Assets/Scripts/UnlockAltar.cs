using UnityEngine;
using Unity.Netcode;

// 이 스크립트는 맵의 특정 청크를 해금하는 제단(Altar)의 역할을 합니다.
public class UnlockAltar : NetworkBehaviour
{
    [Header("해금 조건")]
    public ItemData requiredItem; // 해금에 필요한 아이템
    public int requiredAmount = 1; // 필요한 아이템 수량

    [Header("해금 대상")]
    // 이 제단을 기준으로 어느 방향의 청크를 해금할지 결정합니다. (예: X=1, Y=0 이면 오른쪽 청크)
    public Vector2Int chunkToUnlockOffset = new Vector2Int(1, 0);

    // 제단이 이미 사용되었는지 동기화하는 변수
    private NetworkVariable<bool> isUsed = new NetworkVariable<bool>(false);

    private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // isUsed 변수의 값이 변경될 때마다 OnUsedStateChanged 함수를 실행하도록 등록합니다.
        isUsed.OnValueChanged += OnUsedStateChanged;
        // 늦게 접속한 클라이언트를 위해 현재 상태를 즉시 반영합니다.
        OnUsedStateChanged(false, isUsed.Value);
    }

    // isUsed 상태가 변경되면 호출되어 제단의 색상을 바꿉니다.
    private void OnUsedStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // 사용된 제단은 회색으로 변경
            spriteRenderer.color = Color.gray;
        }
    }

    // 서버에서만 실행되어 해금을 시도하는 함수
    public void AttemptUnlock(PlayerInventory playerInventory)
    {
        if (!IsServer || isUsed.Value) return;

        // 플레이어가 필요한 아이템을 충분히 가지고 있는지 확인합니다.
        if (playerInventory.GetItemQuantity(requiredItem.itemID) >= requiredAmount)
        {
            // 아이템을 소모하고 제단을 '사용됨' 상태로 만듭니다.
            playerInventory.RemoveItem(requiredItem.itemID, requiredAmount);
            isUsed.Value = true;

            // WorldManager에게 청크를 해금하라고 명령합니다.
            // Vector2Int는 Vector3로 자동 변환되지 않으므로 명시적으로 변환합니다.
            Vector3 altarWorldPosition = transform.position;
            FindFirstObjectByType<WorldManager>()?.UnlockChunkAt(altarWorldPosition, chunkToUnlockOffset);
        }
    }
}
