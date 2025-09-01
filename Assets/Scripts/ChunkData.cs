using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System; // IEquatable을 사용하기 위해 추가
using System.Linq; // SequenceEqual을 사용하기 위해 추가

// NetworkVariable이 byte[]를 다룰 수 있도록 만드는 래퍼 구조체입니다.
public struct ByteArr : INetworkSerializable, IEquatable<ByteArr>
{
    public byte[] Value;

    public ByteArr(byte[] value)
    {
        Value = value;
    }

    // 데이터를 네트워크로 보내거나 받을 때 호출됩니다.
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // 데이터를 쓸 때 (보낼 때)
        if (serializer.IsWriter)
        {
            if (Value == null)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(0); // 길이가 0임을 알림
            }
            else
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Value.Length);
                serializer.GetFastBufferWriter().WriteBytesSafe(Value, Value.Length);
            }
        }
        // 데이터를 읽을 때 (받을 때)
        else
        {
            serializer.GetFastBufferReader().ReadValueSafe(out int length);
            if (length > 0)
            {
                Value = new byte[length];
                serializer.GetFastBufferReader().ReadBytesSafe(ref Value, length);
            }
            else
            {
                Value = null;
            }
        }
    }

    // 두 ByteArr가 같은지 비교하는 방법을 Netcode에게 알려줍니다.
    public bool Equals(ByteArr other)
    {
        if (Value == null && other.Value == null) return true;
        if (Value == null || other.Value == null) return false;
        return Value.SequenceEqual(other.Value);
    }
}


// 이 스크립트는 각 청크에 부착되어, 자신의 타일 데이터를 네트워크로 동기화하는 역할을 합니다.
public class ChunkData : NetworkBehaviour
{
    // NetworkVariable의 타입을 수정한 부분입니다.
    private NetworkVariable<ByteArr> groundTileData = new NetworkVariable<ByteArr>();
    private NetworkVariable<ByteArr> objectTileData = new NetworkVariable<ByteArr>();

    private Tilemap groundTilemap;
    private Tilemap objectTilemap;
    private WorldManager worldManager;

    public override void OnNetworkSpawn()
    {
        groundTilemap = transform.Find("GroundTilemap").GetComponent<Tilemap>();
        objectTilemap = transform.Find("ObjectTilemap").GetComponent<Tilemap>();
        // 구식 FindObjectOfType을 최신 FindFirstObjectByType으로 변경했습니다.
        worldManager = FindFirstObjectByType<WorldManager>();

        groundTileData.OnValueChanged += OnGroundTileDataChanged;
        objectTileData.OnValueChanged += OnObjectTileDataChanged;

        if (!IsServer && groundTileData.Value.Value != null)
        {
            BuildTiles(groundTilemap, groundTileData.Value.Value, worldManager.groundTile);
        }
        if (!IsServer && objectTileData.Value.Value != null)
        {
            BuildTiles(objectTilemap, objectTileData.Value.Value, worldManager.objectTile);
        }
    }

    public override void OnNetworkDespawn()
    {
        groundTileData.OnValueChanged -= OnGroundTileDataChanged;
        objectTileData.OnValueChanged -= OnObjectTileDataChanged;
    }

    public void SetTileData(List<Vector3Int> groundPositions, List<Vector3Int> objectPositions)
    {
        if (!IsServer) return;
        // 데이터를 설정할 때 새로운 ByteArr 구조체로 감싸줍니다.
        groundTileData.Value = new ByteArr(SerializePositions(groundPositions));
        objectTileData.Value = new ByteArr(SerializePositions(objectPositions));
    }

    private void OnGroundTileDataChanged(ByteArr previousValue, ByteArr newValue)
    {
        // 실제 byte[] 데이터는 newValue.Value에 들어있습니다.
        BuildTiles(groundTilemap, newValue.Value, worldManager.groundTile);
    }

    private void OnObjectTileDataChanged(ByteArr previousValue, ByteArr newValue)
    {
        BuildTiles(objectTilemap, newValue.Value, worldManager.objectTile);
    }

    private void BuildTiles(Tilemap tilemap, byte[] data, TileBase tile)
    {
        if (tilemap == null || tile == null || data == null || data.Length == 0) return;

        List<Vector3Int> positions = DeserializePositions(data);
        TileBase[] tiles = new TileBase[positions.Count];
        for (int i = 0; i < tiles.Length; i++) { tiles[i] = tile; }
        tilemap.SetTiles(positions.ToArray(), tiles);
    }

    // --- 데이터 압축/해제 함수들은 변경사항이 없습니다 ---
    private byte[] SerializePositions(List<Vector3Int> positions)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(positions.Count);
                foreach (var pos in positions)
                {
                    writer.Write((short)pos.x);
                    writer.Write((short)pos.y);
                }
            }
            return stream.ToArray();
        }
    }

    private List<Vector3Int> DeserializePositions(byte[] data)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        if (data == null || data.Length == 0) return positions;
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int x = reader.ReadInt16();
                    int y = reader.ReadInt16();
                    positions.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        return positions;
    }
}

