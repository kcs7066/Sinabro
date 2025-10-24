using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

// 각 타일 데이터의 네트워크 전송을 위한 구조체
public struct TileDataNetwork : INetworkSerializable, System.IEquatable<TileDataNetwork>
{
    public byte[] data;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // ## 오류 수정: 복잡한 byte 처리 대신, BufferSerializer의 배열 직렬화 기능을 직접 사용합니다. ##
        // 이 한 줄이 길이 정보와 바이트 데이터를 모두 안전하게 처리해 줍니다.
        serializer.SerializeValue(ref data);
    }

    public bool Equals(TileDataNetwork other)
    {
        // 두 byte 배열이 같은지 비교합니다.
        if (data == null && other.data == null) return true;
        if (data == null || other.data == null) return false;
        return System.Linq.Enumerable.SequenceEqual(data, other.data);
    }
}

public class ChunkData : NetworkBehaviour
{
    private NetworkVariable<TileDataNetwork> groundTileData = new NetworkVariable<TileDataNetwork>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<TileDataNetwork> objectTileData = new NetworkVariable<TileDataNetwork>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Tilemap groundTilemap;
    private Tilemap objectTilemap;

    public override void OnNetworkSpawn()
    {
        groundTilemap = transform.Find("GroundTilemap").GetComponent<Tilemap>();
        objectTilemap = transform.Find("ObjectTilemap").GetComponent<Tilemap>();

        groundTileData.OnValueChanged += OnGroundTileDataChanged;
        objectTileData.OnValueChanged += OnObjectTileDataChanged;

        if (!IsServer)
        {
            OnGroundTileDataChanged(default, groundTileData.Value);
            OnObjectTileDataChanged(default, objectTileData.Value);
        }
    }

    public void SetTileData(List<Vector3Int> groundPositions, List<Vector3Int> objectPositions)
    {
        if (!IsServer) return;
        groundTileData.Value = new TileDataNetwork { data = SerializePositions(groundPositions) };
        objectTileData.Value = new TileDataNetwork { data = SerializePositions(objectPositions) };
    }

    private void OnGroundTileDataChanged(TileDataNetwork previousValue, TileDataNetwork newValue)
    {
        BuildTiles(groundTilemap, newValue.data, WorldManager.Instance.groundTile);
    }

    private void OnObjectTileDataChanged(TileDataNetwork previousValue, TileDataNetwork newValue)
    {
        BuildTiles(objectTilemap, newValue.data, WorldManager.Instance.objectTile);
    }

    private void BuildTiles(Tilemap tilemap, byte[] data, TileBase tile)
    {
        if (tilemap == null || tile == null || data == null || data.Length == 0) return;

        List<Vector3Int> positions = DeserializePositions(data);
        TileBase[] tiles = new TileBase[positions.Count];
        for (int i = 0; i < tiles.Length; i++) { tiles[i] = tile; }
        tilemap.SetTiles(positions.ToArray(), tiles);
    }

    private byte[] SerializePositions(List<Vector3Int> positions)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(positions.Count);
            foreach (var pos in positions)
            {
                writer.Write((short)pos.x);
                writer.Write((short)pos.y);
            }
            return stream.ToArray();
        }
    }

    private List<Vector3Int> DeserializePositions(byte[] data)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        if (data == null || data.Length == 0) return positions;
        using (MemoryStream stream = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            if (reader.BaseStream.Length < 4) return positions;
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                if (reader.BaseStream.Position + 4 > reader.BaseStream.Length) break;
                int x = reader.ReadInt16();
                int y = reader.ReadInt16();
                positions.Add(new Vector3Int(x, y, 0));
            }
        }
        return positions;
    }
}

