using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

// 이제 TileManager도 NetworkBehaviour가 되어야 합니다.
public class TileManager : NetworkBehaviour
{
    // 타일 체력은 서버에만 존재해야 합니다.
    private Dictionary<Vector3Int, int> worldTileHealths = new Dictionary<Vector3Int, int>();

    // 로컬 플레이어가 타일 파괴를 요청하는 함수
    public void RequestDamageTile(Vector3 mouseWorldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.TryGetComponent<Tilemap>(out var clickedTilemap))
        {
            // 클릭된 타일맵이 있는 청크가 네트워크 오브젝트인지 확인합니다.
            if (clickedTilemap.transform.parent.TryGetComponent<NetworkObject>(out var chunkNetworkObject))
            {
                Vector3Int cellPosition = clickedTilemap.WorldToCell(mouseWorldPos);
                // 서버에 타일 파괴를 요청합니다.
                DamageTileServerRpc(chunkNetworkObject.NetworkObjectId, cellPosition);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DamageTileServerRpc(ulong chunkNetworkId, Vector3Int cellPosition, ServerRpcParams rpcParams = default)
    {
        // 요청을 보낸 클라이언트의 플레이어를 찾습니다.
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        PlayerEquipment playerEquipment = playerObject.GetComponent<PlayerEquipment>();

        // 타겟 청크를 찾습니다.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(chunkNetworkId, out var chunkObject))
        {
            Tilemap targetTilemap = chunkObject.transform.Find("ObjectTilemap").GetComponent<Tilemap>();
            TileBase tile = targetTilemap.GetTile(cellPosition);

            if (tile is WorldTile worldTile)
            {
                Vector3Int worldCellPosition = new Vector3Int(
                    (int)chunkObject.transform.position.x + cellPosition.x,
                    (int)chunkObject.transform.position.y + cellPosition.y, 0);

                if (!worldTileHealths.ContainsKey(worldCellPosition))
                {
                    worldTileHealths.Add(worldCellPosition, 10);
                }

                int damage = playerEquipment.GetCurrentToolPower();
                worldTileHealths[worldCellPosition] -= damage;

                if (worldTileHealths[worldCellPosition] <= 0)
                {
                    worldTileHealths.Remove(worldCellPosition);

                    // 아이템 드랍 로직
                    if (worldTile.dropItemData != null)
                    {
                        // 아이템 ID와 수량을 준비합니다.
                        int itemID = worldTile.dropItemData.itemID;
                        int quantity = 1;

                        // 요청을 보낸 특정 클라이언트에게만 아이템을 주라고 명령합니다.
                        ClientRpcParams clientRpcParams = new ClientRpcParams
                        {
                            Send = new ClientRpcSendParams
                            {
                                TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                            }
                        };
                        AwardItemClientRpc(itemID, quantity, clientRpcParams);
                    }

                    // 모든 클라이언트에게 타일을 파괴하라고 명령합니다.
                    DestroyTileClientRpc(chunkNetworkId, cellPosition);
                }
            }
        }
    }

    // 특정 클라이언트에게 아이템을 지급하는 함수
    [ClientRpc]
    private void AwardItemClientRpc(int itemID, int quantity, ClientRpcParams clientRpcParams = default)
    {
        // 이 코드는 서버가 지정한 타겟 클라이언트에서만 실행됩니다.
        ItemData itemToAdd = ItemDatabase.Instance.GetItemById(itemID);
        if (itemToAdd != null)
        {
            // 씬에 있는 InventoryManager를 찾아 아이템을 추가합니다.
            FindObjectOfType<InventoryManager>().AddItem(itemToAdd, quantity);
        }
    }

    [ClientRpc]
    private void DestroyTileClientRpc(ulong chunkNetworkId, Vector3Int cellPosition)
    {
        // 모든 클라이언트에서 실행됩니다.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(chunkNetworkId, out var chunkObject))
        {
            Tilemap targetTilemap = chunkObject.transform.Find("ObjectTilemap").GetComponent<Tilemap>();
            targetTilemap.SetTile(cellPosition, null);
        }
    }
}
