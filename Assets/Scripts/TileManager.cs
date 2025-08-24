using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 이 스크립트는 '오브젝트' 레이어의 타일을 관리하고 파괴 시 아이템을 드랍합니다.
public class TileManager : MonoBehaviour
{
    public Tilemap objectTilemap;
    public InventoryManager inventoryManager; // 인벤토리 관리자를 연결할 변수

    private Dictionary<Vector3Int, int> tileHealths = new Dictionary<Vector3Int, int>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = objectTilemap.WorldToCell(mouseWorldPos);

            TileBase tile = objectTilemap.GetTile(cellPosition);

            // 해당 셀에 타일이 있고, 그 타일이 WorldTile 타입인지 확인합니다.
            if (tile is WorldTile)
            {
                DamageTile(cellPosition, (WorldTile)tile);
            }
        }
    }

    // 이제 어떤 타일에 피해를 주는지도 알아야 하므로 WorldTile을 매개변수로 받습니다.
    void DamageTile(Vector3Int cellPosition, WorldTile tile)
    {
        if (!tileHealths.ContainsKey(cellPosition))
        {
            tileHealths.Add(cellPosition, 3);
        }

        tileHealths[cellPosition]--;

        if (tileHealths[cellPosition] > 0)
        {
            Debug.Log(tile.name + " 타일 체력 감소! 남은 체력: " + tileHealths[cellPosition]);
        }
        else
        {
            tileHealths.Remove(cellPosition);
            objectTilemap.SetTile(cellPosition, null);

            // 아이템 드랍 로직!
            // 만약 파괴된 타일이 드랍할 아이템 정보를 가지고 있다면,
            if (tile.dropItemData != null)
            {
                // 인벤토리 매니저에게 아이템을 1개 추가하라고 요청합니다.
                inventoryManager.AddItem(tile.dropItemData, 1);
            }

            Debug.Log(tile.name + " 타일 파괴 완료!");
        }
    }
}
