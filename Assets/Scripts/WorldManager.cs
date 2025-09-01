using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : NetworkBehaviour
{
    [Header("월드 설정")]
    private Transform localPlayerTransform;
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

    void Update()
    {
        if (!IsServer) return;

        if (localPlayerTransform == null)
        {
            if (PlayerController.LocalInstance != null)
            {
                localPlayerTransform = PlayerController.LocalInstance.transform;
                UpdateChunks();
            }
            return;
        }

        Vector2Int currentPlayerChunkPosition = GetChunkPositionFromWorld(localPlayerTransform.position);
        if (currentPlayerChunkPosition != lastPlayerChunkPosition)
        {
            UpdateChunks();
        }
    }

    void UpdateChunks()
    {
        if (localPlayerTransform == null) return;

        lastPlayerChunkPosition = GetChunkPositionFromWorld(localPlayerTransform.position);

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
        // 1. 청크 오브젝트를 인스턴스화합니다.
        Vector3 worldPosition = new Vector3(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize, 0);
        GameObject newChunk = Instantiate(chunkPrefab, worldPosition, Quaternion.identity, this.transform);
        newChunk.name = $"Chunk ({chunkPosition.x}, {chunkPosition.y})";

        // 2. 서버에서만 지형 데이터를 생성합니다.
        List<Vector3Int> groundPositions = new List<Vector3Int>();
        List<Vector3Int> objectPositions = new List<Vector3Int>();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkPosition.x * chunkSize + x;
                int worldY = chunkPosition.y * chunkSize + y;
                float noiseValue = Mathf.PerlinNoise(worldX * noiseScale, worldY * noiseScale);
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                groundPositions.Add(tilePosition);
                if (noiseValue > objectThreshold)
                {
                    objectPositions.Add(tilePosition);
                }
            }
        }

        // 3. 청크를 네트워크에 스폰합니다.
        NetworkObject chunkNetworkObject = newChunk.GetComponent<NetworkObject>();
        chunkNetworkObject.Spawn();

        // 4. 스폰된 청크의 ChunkData 컴포넌트에 지형 데이터를 설정합니다.
        // 이 데이터는 NetworkVariable을 통해 클라이언트로 자동 동기화됩니다.
        ChunkData chunkData = newChunk.GetComponent<ChunkData>();
        if (chunkData != null)
        {
            chunkData.SetTileData(groundPositions, objectPositions);
        }
        else
        {
            Debug.LogError($"Chunk prefab is missing the ChunkData component!");
        }

        activeChunks.Add(chunkPosition, newChunk);
    }

    void UnloadChunk(Vector2Int chunkPosition)
    {
        if (activeChunks.TryGetValue(chunkPosition, out GameObject chunkToDestroy))
        {
            chunkToDestroy.GetComponent<NetworkObject>().Despawn(); // Despawn()이 Destroy()를 처리합니다.
            activeChunks.Remove(chunkPosition);
        }
    }

    Vector2Int GetChunkPositionFromWorld(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int y = Mathf.FloorToInt(worldPosition.y / chunkSize);
        return new Vector2Int(x, y);
    }
}

