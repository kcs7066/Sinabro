using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

// 이제 WorldManager도 NetworkBehaviour가 되어야 합니다.
public class WorldManager : NetworkBehaviour
{
    [Header("월드 설정")]
    public Transform player;
    public int chunkSize = 16;
    public int viewDistance = 1;

    [Header("청크 프리팹")]
    public GameObject chunkPrefab;

    [Header("타일 에셋")]
    public TileBase groundTile;
    public WorldTile objectTile;

    [Header("지형 생성 설정")]
    public float noiseScale = 0.1f;
    public float objectThreshold = 0.7f;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerChunkPosition;

    // NetworkBehaviour의 시작 함수입니다.
    public override void OnNetworkSpawn()
    {
        // 이 코드는 서버(호스트)에서만 실행됩니다.
        if (!IsServer) return;

        // 게임 시작 시 플레이어 주변의 청크를 즉시 로드합니다.
        // Start() 대신 여기서 호출하여 네트워크가 준비된 후에 실행되도록 보장합니다.
        UpdateChunks();
    }

    void Update()
    {
        // 청크 업데이트 로직도 서버에서만 실행되어야 합니다.
        if (!IsServer) return;

        // 플레이어의 현재 청크 좌표를 계산합니다.
        // 참고: 현재는 첫번째 플레이어(호스트)만 추적합니다.
        // 나중에는 모든 플레이어 위치를 고려하여 청크를 로드해야 할 수 있습니다.
        Vector2Int currentPlayerChunkPosition = GetChunkPositionFromWorld(player.position);
        if (currentPlayerChunkPosition != lastPlayerChunkPosition)
        {
            UpdateChunks();
        }
    }

    void UpdateChunks()
    {
        lastPlayerChunkPosition = GetChunkPositionFromWorld(player.position);

        // 1. 필요한 청크 로드하기
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkPos = new Vector2Int(lastPlayerChunkPosition.x + x, lastPlayerChunkPosition.y + y);
                if (!activeChunks.ContainsKey(chunkPos))
                {
                    LoadChunk(chunkPos);
                }
            }
        }

        // 2. 불필요한 청크 언로드하기
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            int distanceX = Mathf.Abs(chunk.Key.x - lastPlayerChunkPosition.x);
            int distanceY = Mathf.Abs(chunk.Key.y - lastPlayerChunkPosition.y);

            if (distanceX > viewDistance || distanceY > viewDistance)
            {
                chunksToUnload.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
        }
    }

    void LoadChunk(Vector2Int chunkPosition)
    {
        Vector3 worldPosition = new Vector3(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize, 0);
        GameObject newChunk = Instantiate(chunkPrefab, worldPosition, Quaternion.identity, this.transform);

        // 네트워크 오브젝트를 서버에서 스폰(Spawn)합니다.
        // 이렇게 해야 모든 클라이언트에게 청크가 생성됩니다.
        newChunk.GetComponent<NetworkObject>().Spawn();

        newChunk.name = $"Chunk ({chunkPosition.x}, {chunkPosition.y})";

        Tilemap groundTilemap = newChunk.transform.Find("GroundTilemap").GetComponent<Tilemap>();
        Tilemap objectTilemap = newChunk.transform.Find("ObjectTilemap").GetComponent<Tilemap>();

        GenerateChunk(chunkPosition, groundTilemap, objectTilemap);

        activeChunks.Add(chunkPosition, newChunk);
    }

    void UnloadChunk(Vector2Int chunkPosition)
    {
        if (activeChunks.TryGetValue(chunkPosition, out GameObject chunkToDestroy))
        {
            // 네트워크 오브젝트를 파괴할 때는 Despawn을 사용해야 합니다.
            chunkToDestroy.GetComponent<NetworkObject>().Despawn();
            activeChunks.Remove(chunkPosition);
            // Destroy(chunkToDestroy); // Despawn이 오브젝트 파괴를 처리합니다.
        }
    }

    void GenerateChunk(Vector2Int chunkPosition, Tilemap groundTilemap, Tilemap objectTilemap)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkPosition.x * chunkSize + x;
                int worldY = chunkPosition.y * chunkSize + y;

                float noiseValue = Mathf.PerlinNoise(worldX * noiseScale, worldY * noiseScale);

                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(tilePosition, groundTile);

                if (noiseValue > objectThreshold)
                {
                    objectTilemap.SetTile(tilePosition, objectTile);
                }
            }
        }
    }

    Vector2Int GetChunkPositionFromWorld(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int y = Mathf.FloorToInt(worldPosition.y / chunkSize);
        return new Vector2Int(x, y);
    }
}
