using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 이 스크립트는 '오브젝트' 레이어의 타일을 관리하고 파괴 시 아이템을 드랍합니다.
public class TileManager : MonoBehaviour
{
    public InventoryManager inventoryManager;
    // public PlayerEquipment playerEquipment; // 이제 이 줄은 필요 없습니다. 스크립트가 직접 찾아갑니다.

    private static Dictionary<Vector3Int, int> worldTileHealths = new Dictionary<Vector3Int, int>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.GetComponent<Tilemap>() != null)
            {
                Tilemap clickedTilemap = hit.collider.GetComponent<Tilemap>();
                Vector3Int cellPosition = clickedTilemap.WorldToCell(mouseWorldPos);
                TileBase tile = clickedTilemap.GetTile(cellPosition);

                if (tile is WorldTile)
                {
                    DamageTile(clickedTilemap, cellPosition, (WorldTile)tile);
                }
            }
        }
    }

    void DamageTile(Tilemap targetTilemap, Vector3Int cellPosition, WorldTile tile)
    {
        // 로컬 플레이어가 아직 생성되지 않았으면 아무것도 하지 않습니다.
        if (PlayerController.LocalInstance == null) return;

        // PlayerEquipment 컴포넌트를 로컬 플레이어로부터 직접 찾아옵니다.
        PlayerEquipment playerEquipment = PlayerController.LocalInstance.GetComponent<PlayerEquipment>();
        if (playerEquipment == null) return; // 만약 PlayerEquipment가 없다면 중단합니다.

        Vector3Int worldCellPosition = new Vector3Int(
            targetTilemap.cellBounds.x + cellPosition.x,
            targetTilemap.cellBounds.y + cellPosition.y,
            0
        );

        if (!worldTileHealths.ContainsKey(worldCellPosition))
        {
            worldTileHealths.Add(worldCellPosition, 10);
        }

        int damage = playerEquipment.GetCurrentToolPower();
        worldTileHealths[worldCellPosition] -= damage;

        if (worldTileHealths[worldCellPosition] > 0)
        {
            Debug.Log(tile.name + " 타일 체력 감소! 남은 체력: " + worldTileHealths[worldCellPosition]);
        }
        else
        {
            worldTileHealths.Remove(worldCellPosition);
            targetTilemap.SetTile(cellPosition, null);

            if (tile.dropItemData != null)
            {
                inventoryManager.AddItem(tile.dropItemData, 1);
            }
            Debug.Log(tile.name + " 타일 파괴 완료!");
        }
    }
}
