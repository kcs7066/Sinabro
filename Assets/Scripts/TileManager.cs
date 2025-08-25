using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 이 스크립트는 '오브젝트' 레이어의 타일을 관리하고 파괴 시 아이템을 드랍합니다.
public class TileManager : MonoBehaviour
{
    public Tilemap objectTilemap;
    public InventoryManager inventoryManager;
    public PlayerEquipment playerEquipment; // 플레이어 장비 정보를 참조할 변수

    private Dictionary<Vector3Int, int> tileHealths = new Dictionary<Vector3Int, int>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = objectTilemap.WorldToCell(mouseWorldPos);

            TileBase tile = objectTilemap.GetTile(cellPosition);

            if (tile is WorldTile)
            {
                DamageTile(cellPosition, (WorldTile)tile);
            }
        }
    }

    void DamageTile(Vector3Int cellPosition, WorldTile tile)
    {
        if (!tileHealths.ContainsKey(cellPosition))
        {
            // 이제 타일의 체력을 더 높게 설정해서 테스트해봅시다.
            tileHealths.Add(cellPosition, 10);
        }

        // 플레이어 장비로부터 현재 도구의 채집 능력을 가져와서 피해를 줍니다.
        int damage = playerEquipment.GetCurrentToolPower();
        tileHealths[cellPosition] -= damage;

        if (tileHealths[cellPosition] > 0)
        {
            Debug.Log(tile.name + " 타일 체력 감소! 남은 체력: " + tileHealths[cellPosition]);
        }
        else
        {
            tileHealths.Remove(cellPosition);
            objectTilemap.SetTile(cellPosition, null);

            if (tile.dropItemData != null)
            {
                inventoryManager.AddItem(tile.dropItemData, 1);
            }

            Debug.Log(tile.name + " 타일 파괴 완료!");
        }
    }
}
