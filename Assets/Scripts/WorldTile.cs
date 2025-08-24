using UnityEngine;
using UnityEngine.Tilemaps;

// 이 스크립트는 일반 타일에 추가적인 데이터를 담을 수 있게 해줍니다.
// CreateAssetMenu를 사용하면 유니티 에디터에서 이 타일을 에셋으로 만들 수 있습니다.
[CreateAssetMenu(fileName = "New World Tile", menuName = "Tiles/World Tile")]
public class WorldTile : Tile
{
    // 이 타일이 파괴되었을 때 드랍할 아이템의 데이터입니다.
    public ItemData dropItemData;
}
