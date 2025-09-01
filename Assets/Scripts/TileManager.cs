using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

public class TileManager : NetworkBehaviour
{
    private Dictionary<Vector3Int, int> tileHealths = new Dictionary<Vector3Int, int>();
    private const int TILE_MAX_HEALTH = 3;

    public void ProcessTileDamage(Vector3 worldPosition, ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"[Server] Received tile damage request from client: {clientId} at position: {worldPosition}");

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        Tilemap targetTilemap = null;
        WorldTile tile = null;
        Vector3Int cellPosition = Vector3Int.zero;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Tilemap>(out targetTilemap))
            {
                if (targetTilemap.name != "ObjectTilemap") continue;
                cellPosition = targetTilemap.WorldToCell(worldPosition);
                if (targetTilemap.GetTile(cellPosition) is WorldTile worldTile)
                {
                    tile = worldTile;
                    break;
                }
            }
        }

        if (targetTilemap != null && tile != null)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                tileHealths.TryGetValue(cellPosition, out int currentHealth);
                if (currentHealth == 0) currentHealth = TILE_MAX_HEALTH;

                int damage = 1;
                if (client.PlayerObject.TryGetComponent<PlayerEquipment>(out var equipment))
                {
                    damage = equipment.GetCurrentToolPower();
                }

                currentHealth -= damage;
                tileHealths[cellPosition] = currentHealth;

                if (currentHealth <= 0)
                {
                    NetworkObject tilemapNetworkObject = targetTilemap.GetComponentInParent<NetworkObject>();
                    RemoveTileClientRpc(tilemapNetworkObject.NetworkObjectId, cellPosition);
                    tileHealths.Remove(cellPosition);

                    if (tile.dropItemData != null)
                    {
                        Debug.Log($"[Server] Tile destroyed. Attempting to give item '{tile.dropItemData.itemName}' to client: {clientId}");
                        if (client.PlayerObject.TryGetComponent<PlayerInventory>(out var playerInventory))
                        {
                            Debug.Log($"[Server] Found PlayerInventory for client {clientId}. Adding item.");
                            playerInventory.AddItem(tile.dropItemData.itemID, 1);
                        }
                        else
                        {
                            Debug.LogError($"[Server] FAILED to find PlayerInventory on player object for client: {clientId}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"[Server] FAILED to find client with ID: {clientId}");
            }
        }
        else
        {
            Debug.LogWarning($"[Server] No valid tile found at position {worldPosition} for damage request.");
        }
    }

    [ClientRpc]
    private void RemoveTileClientRpc(ulong chunkNetworkId, Vector3Int cellPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(chunkNetworkId, out var networkObject))
        {
            Transform objectTilemapTransform = networkObject.transform.Find("ObjectTilemap");
            if (objectTilemapTransform != null)
            {
                Tilemap targetTilemap = objectTilemapTransform.GetComponent<Tilemap>();
                if (targetTilemap != null)
                {
                    targetTilemap.SetTile(cellPosition, null);
                }
            }
        }
    }
}

