using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap을 제어하기 위해 추가합니다.

// 이 스크립트는 월드 전체를 관리하며, 플레이어 주변의 청크를 동적으로 로드/언로드합니다.
public class WorldManager : MonoBehaviour
{
    [Header("월드 설정")]
    public Transform player;
    public int chunkSize = 16;
    public int viewDistance = 1;

    [Header("청크 프리팹")]
    public GameObject chunkPrefab;

    [Header("타일 에셋")] // 생성할 타일들을 연결해 줄 변수들입니다.
    public TileBase groundTile;
    public WorldTile objectTile; // StoneTile 같은 WorldTile을 연결합니다.

    [Header("지형 생성 설정")]
    public float noiseScale = 0.1f; // 노이즈의 크기 (값이 작을수록 지형이 완만해집니다)
    public float objectThreshold = 0.7f; // 이 값보다 노이즈가 높으면 오브젝트(돌)가 생성됩니다.

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerChunkPosition;

    void Start()
    {
        UpdateChunks();
    }

    void Update()
    {
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
            // 현재 청크와 플레이어 청크 사이의 거리를 계산합니다.
            int distanceX = Mathf.Abs(chunk.Key.x - lastPlayerChunkPosition.x);
            int distanceY = Mathf.Abs(chunk.Key.y - lastPlayerChunkPosition.y);

            // 만약 거리가 viewDistance보다 크다면, 언로드 목록에 추가합니다.
            if (distanceX > viewDistance || distanceY > viewDistance)
            {
                chunksToUnload.Add(chunk.Key);
            }
        }

        // 언로드 목록에 있는 모든 청크를 제거합니다.
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

        Tilemap groundTilemap = newChunk.transform.Find("GroundTilemap").GetComponent<Tilemap>();
        Tilemap objectTilemap = newChunk.transform.Find("ObjectTilemap").GetComponent<Tilemap>();

        GenerateChunk(chunkPosition, groundTilemap, objectTilemap);

        activeChunks.Add(chunkPosition, newChunk);
    }

    // 특정 위치의 청크를 파괴하는 함수 (새로 추가됨)
    void UnloadChunk(Vector2Int chunkPosition)
    {
        // 만약 해당 청크가 활성화 목록에 있다면,
        if (activeChunks.TryGetValue(chunkPosition, out GameObject chunkToDestroy))
        {
            // 게임 오브젝트를 파괴합니다.
            Destroy(chunkToDestroy);
            // 활성화된 청크 목록에서 제거합니다.
            activeChunks.Remove(chunkPosition);
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
