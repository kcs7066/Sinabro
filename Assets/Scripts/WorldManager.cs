using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class WorldManager : NetworkBehaviour
{
    [Header("월드 설정")]
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
    private Dictionary<ulong, Vector2Int> playerChunkPositions = new Dictionary<ulong, Vector2Int>();

    private HashSet<Vector2Int> unlockedChunks = new HashSet<Vector2Int>();

    // ## 추가: 서버가 종료 중인지 확인하는 보다 안정적인 플래그 ##
    private bool isShuttingDown = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

            // ## 추가: 서버가 멈출 때 isShuttingDown 플래그를 true로 설정합니다. ##
            NetworkManager.Singleton.OnServerStopped += (bool _) => { isShuttingDown = true; };

            unlockedChunks.Add(Vector2Int.zero);

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                OnClientConnected(client.ClientId);
            }
        }
    }

    void Update()
    {
        // ## 수정: isShuttingDown 플래그를 확인합니다. ##
        if (!IsServer || isShuttingDown) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            Vector2Int currentPlayerChunkPos = GetChunkPositionFromWorld(client.PlayerObject.transform.position);
            playerChunkPositions.TryGetValue(client.ClientId, out Vector2Int lastPlayerChunkPos);

            if (currentPlayerChunkPos != lastPlayerChunkPos)
            {
                playerChunkPositions[client.ClientId] = currentPlayerChunkPos;
                UpdateChunks();
            }
        }
    }

    public void UnlockChunkAt(Vector3 altarWorldPosition, Vector2Int offset)
    {
        if (!IsServer) return;

        Vector2Int altarChunkPos = GetChunkPositionFromWorld(altarWorldPosition);
        Vector2Int chunkToUnlockPos = altarChunkPos + offset;

        if (unlockedChunks.Add(chunkToUnlockPos))
        {
            Debug.Log($"[Server] Chunk at {chunkToUnlockPos} has been unlocked.");
            UpdateChunks();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} connected. Initializing chunk position.");
        playerChunkPositions[clientId] = new Vector2Int(int.MaxValue, int.MaxValue);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // ## 수정: isShuttingDown 플래그를 확인합니다. ##
        if (isShuttingDown) return;

        Debug.Log($"Player {clientId} disconnected. Removing from chunk tracking.");
        playerChunkPositions.Remove(clientId);
        UpdateChunks();
    }

    void UpdateChunks()
    {
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        foreach (var playerPos in playerChunkPositions.Values)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int y = -viewDistance; y <= viewDistance; y++)
                {
                    requiredChunks.Add(new Vector2Int(playerPos.x + x, playerPos.y + y));
                }
            }
        }

        foreach (var chunkPos in requiredChunks)
        {
            if (unlockedChunks.Contains(chunkPos) && !activeChunks.ContainsKey(chunkPos))
            {
                LoadChunk(chunkPos);
            }
        }

        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (!requiredChunks.Contains(chunk.Key) || !unlockedChunks.Contains(chunk.Key))
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
        newChunk.name = $"Chunk ({chunkPosition.x}, {chunkPosition.y})";

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

        NetworkObject chunkNetworkObject = newChunk.GetComponent<NetworkObject>();
        chunkNetworkObject.Spawn();

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
            if (chunkToDestroy != null && chunkToDestroy.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned)
            {
                netObj.Despawn();
            }
            else if (chunkToDestroy != null)
            {
                Destroy(chunkToDestroy);
            }
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

