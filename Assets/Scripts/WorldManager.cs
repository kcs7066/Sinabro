using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Linq;

public class WorldManager : NetworkBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("맵 설정")]
    public List<LandData> allLandData;
    public List<LandGroupZone> landGroupZones;

    [System.Serializable]
    public class LandGroupZone
    {
        public string groupId;
        public List<Vector2Int> possiblePositions;
    }

    [Header("월드 설정")]
    public int chunkSize = 16;
    public TileBase wallTile; // 벽 타일

    [Header("청크 프리팹")]
    public GameObject chunkPrefab;

    [Header("타일 에셋")]
    public TileBase groundTile;
    public WorldTile objectTile;

    [Header("지형 생성 설정")]
    public float noiseScale = 0.1f;
    public float objectThreshold = 0.7f;

    // 서버 전용 데이터
    private Dictionary<Vector2Int, string> chunkToLandIdMap = new Dictionary<Vector2Int, string>();
    private HashSet<string> serverUnlockedLandIds = new HashSet<string>();
    private Dictionary<Vector2Int, ulong> activeChunkNetworkIds = new Dictionary<Vector2Int, ulong>();
    private Dictionary<ulong, Vector2Int> playerChunkPositions = new Dictionary<ulong, Vector2Int>();

    // 클라이언트 전용 데이터 (로컬에서 관리)
    private Dictionary<Vector2Int, GameObject> localWallChunks = new Dictionary<Vector2Int, GameObject>();
    private HashSet<string> clientUnlockedLandIds = new HashSet<string>();


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }


    private void HandleServerStarted()
    {
        Debug.Log("[Server] Server Started. Generating map layout...");
        GenerateMapLayout(); // 맵 레이아웃 생성 시도
    }

    private void GenerateMapLayout()
    {
        chunkToLandIdMap.Clear();
        serverUnlockedLandIds.Clear();
        Debug.Log($"[Server:GenerateMapLayout] Starting layout generation. Found {allLandData?.Count ?? 0} total LandData assets."); // 로그 추가

        // 1. 그룹이 없는 고정 랜드 (Start, 마을 등)를 먼저 배치합니다.
        var fixedLandsToPlace = allLandData?.Where(l => string.IsNullOrEmpty(l.groupId)).ToList() ?? new List<LandData>();
        Debug.Log($"[Server:GenerateMapLayout] Found {fixedLandsToPlace.Count} fixed lands.");
        foreach (var land in fixedLandsToPlace)
        {
            Debug.Log($"[Server:GenerateMapLayout] Placing fixed land: {land.landId} ({land.chunkOffsets?.Count ?? 0} chunks)");
            if (land.chunkOffsets == null) continue;
            foreach (var offset in land.chunkOffsets)
            {
                chunkToLandIdMap[offset] = land.landId;
                Debug.Log($"  - Chunk at {offset} assigned to land {land.landId}");
            }
        }

        // 2. 그룹별로 랜덤 배치를 진행합니다.
        Debug.Log($"[Server:GenerateMapLayout] Starting random group placement. Found {landGroupZones?.Count ?? 0} zones defined.");
        if (landGroupZones == null)
        {
            Debug.LogWarning("[Server:GenerateMapLayout] landGroupZones list is null. Skipping random group placement.");
        }
        else
        {
            foreach (var zone in landGroupZones)
            {
                if (zone == null || string.IsNullOrEmpty(zone.groupId) || zone.possiblePositions == null || zone.possiblePositions.Count == 0)
                {
                    Debug.LogWarning($"[Server:GenerateMapLayout] Skipping invalid zone definition (ID: {zone?.groupId ?? "null"})");
                    continue;
                }

                Debug.Log($"[Server:GenerateMapLayout] Processing zone for group: {zone.groupId}");
                var landsInGroup = allLandData?.Where(l => l.groupId == zone.groupId).ToList() ?? new List<LandData>();
                Debug.Log($"  - Found {landsInGroup.Count} lands for group {zone.groupId}.");
                var shuffledPositions = zone.possiblePositions.OrderBy(p => Random.value).ToList();
                Debug.Log($"  - Found {shuffledPositions.Count} possible positions for this group.");


                for (int i = 0; i < landsInGroup.Count && i < shuffledPositions.Count; i++)
                {
                    var land = landsInGroup[i];
                    var basePosition = shuffledPositions[i];
                    Debug.Log($"  - Placing land {land.landId} at zone base {basePosition} ({land.chunkOffsets?.Count ?? 0} chunks)");
                    if (land.chunkOffsets == null) continue;
                    foreach (var offset in land.chunkOffsets)
                    {
                        Vector2Int finalPos = basePosition + offset;
                        chunkToLandIdMap[finalPos] = land.landId;
                        Debug.Log($"    - Chunk at {finalPos} assigned to land {land.landId}");
                    }
                }
            }
        }

        serverUnlockedLandIds.Add("Start"); // 시작 랜드 해금
        Debug.Log($"[Server:GenerateMapLayout] Map layout generated. Total chunks defined: {chunkToLandIdMap.Count}. Start land unlocked.");
        // 서버 시작 시에는 아직 클라이언트 연결 콜백 전이므로 여기서 맵 상태 전송 X
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"[Server] Client {clientId} connected. Initializing position tracker and sending map state.");
        playerChunkPositions[clientId] = new Vector2Int(int.MaxValue, int.MaxValue); // 초기 위치값 설정
        UpdateClientMapState(clientId); // 연결된 클라이언트에게 현재 맵 상태 전송
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"[Server] Client {clientId} disconnected. Removing from chunk tracking.");
        playerChunkPositions.Remove(clientId);
        UpdateChunks(); // 연결 끊긴 클라이언트 주변 청크 언로드
    }

    void Update()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        bool needsUpdate = false;
        var connectedClients = NetworkManager.Singleton.ConnectedClients;
        if (connectedClients == null) return;


        foreach (var clientPair in connectedClients)
        {
            ulong clientId = clientPair.Key;
            if (clientPair.Value == null) continue;
            NetworkObject playerObject = clientPair.Value.PlayerObject;

            if (playerObject == null)
            {
                if (!playerChunkPositions.ContainsKey(clientId))
                    playerChunkPositions[clientId] = new Vector2Int(int.MaxValue, int.MaxValue);
                continue;
            }

            Vector2Int currentPlayerChunkPos = GetChunkPositionFromWorld(playerObject.transform.position);

            // 딕셔너리에 키가 없거나, 이전 위치가 초기값일 때 최초 위치 설정 및 업데이트 강제
            if (!playerChunkPositions.ContainsKey(clientId) || playerChunkPositions[clientId].x == int.MaxValue)
            {
                Debug.Log($"[Server:Update] Player {clientId} object is ready. Setting initial position: {currentPlayerChunkPos}");
                playerChunkPositions[clientId] = currentPlayerChunkPos;
                needsUpdate = true; // 초기 위치 설정 후 청크 업데이트 필요
            }
            // 위치가 변경되었을 때 업데이트 트리거
            else if (playerChunkPositions[clientId] != currentPlayerChunkPos)
            {
                Debug.Log($"[Server:Update] Player {clientId} moved to chunk {currentPlayerChunkPos}");
                playerChunkPositions[clientId] = currentPlayerChunkPos;
                needsUpdate = true; // 위치가 변경되었으므로 청크 업데이트 필요
            }
        }

        if (needsUpdate)
        {
            Debug.Log("[Server:Update] Player position changed or initialized, triggering UpdateChunks.");
            UpdateChunks();
        }
    }

    // 서버 전용: 특정 랜드를 해금 목록에 추가하고 클라이언트에게 알림
    public void UnlockLand(string landId)
    {
        if (!IsServer) return;
        if (serverUnlockedLandIds.Add(landId))
        {
            Debug.Log($"[Server] Land '{landId}' unlocked.");
            UpdateAllClientsMapState(); // 변경사항 전파
        }
    }

    // 서버 전용: 특정 클라이언트에게 현재 맵 상태 전송
    private void UpdateClientMapState(ulong targetClientId)
    {
        string packedMapLayout = PackMapLayout();
        string packedUnlockedIds = string.Join(",", serverUnlockedLandIds);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetClientId } }
        };
        Debug.Log($"[Server] Sending map state to client {targetClientId}. Layout size: {packedMapLayout.Length}, Unlocked: {packedUnlockedIds}");
        UpdateMapClientRpc(packedMapLayout, packedUnlockedIds, clientRpcParams);
    }

    // 서버 전용: 모든 클라이언트에게 현재 맵 상태 전송
    private void UpdateAllClientsMapState()
    {
        if (!IsServer) return;
        string packedMapLayout = PackMapLayout();
        string packedUnlockedIds = string.Join(",", serverUnlockedLandIds);
        Debug.Log($"[Server] Sending map state to all clients. Layout size: {packedMapLayout.Length}, Unlocked: {packedUnlockedIds}");
        UpdateMapClientRpc(packedMapLayout, packedUnlockedIds);
    }

    // 클라이언트가 맵 구조와 해금 상태를 받아 처리 (벽 생성/제거만 담당)
    [ClientRpc]
    private void UpdateMapClientRpc(string packedMapLayout, string packedUnlockedIds, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("[Client] Received map update from server.");
        UnpackMapLayout(packedMapLayout); // 맵 구조 복원

        clientUnlockedLandIds = new HashSet<string>(packedUnlockedIds.Split(','));
        Debug.Log($"[Client] Unlocked lands updated: {string.Join(", ", clientUnlockedLandIds)}");

        // 기존 로컬 벽 청크 모두 제거 (새 상태에 맞춰 다시 그림)
        List<Vector2Int> wallsToDestroy = localWallChunks.Keys.ToList();
        foreach (var pos in wallsToDestroy)
        {
            if (localWallChunks.TryGetValue(pos, out var wallChunk))
            {
                Destroy(wallChunk);
                localWallChunks.Remove(pos);
            }
        }

        // 필요한 벽 청크 다시 그리기
        int wallCount = 0;
        // 맵 전체 (-4,-4) 부터 (4,4) 까지 순회 (하드코딩된 범위, 추후 수정 필요)
        // ## 디버그: 실제로 정의된 청크만 순회하도록 변경 ##
        Debug.Log($"[Client] Checking for walls based on {chunkToLandIdMap.Count} defined chunks.");
        foreach (var pair in chunkToLandIdMap)
        {
            Vector2Int chunkPos = pair.Key;
            string landId = pair.Value;
            bool isUnlocked = clientUnlockedLandIds.Contains(landId);

            // 해금되지 않았으면 벽 로드
            if (!isUnlocked)
            {
                // 네트워크 청크가 있는지 확인 (타이밍 이슈 방지)
                bool networkChunkExists = false;
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null && NetworkManager.Singleton.SpawnManager.SpawnedObjects != null)
                {
                    networkChunkExists = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values.Any(no => no != null && GetChunkPositionFromWorld(no.transform.position) == chunkPos);
                }

                if (!networkChunkExists && !localWallChunks.ContainsKey(chunkPos))
                {
                    LoadWallChunk(chunkPos);
                    wallCount++;
                }
            }
        }
        Debug.Log($"[Client] Finished updating wall chunks. Created {wallCount} new walls.");
    }

    // 서버 전용: 플레이어 위치 기반 청크 로딩/언로딩 관리
    void UpdateChunks()
    {
        if (!IsServer) return;
        Debug.Log("[Server] Updating chunks based on player positions...");

        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        foreach (var playerPos in playerChunkPositions.Values)
        {
            if (playerPos.x == int.MaxValue) continue;
            int currentViewDistance = 1;
            for (int x = -currentViewDistance; x <= currentViewDistance; x++)
            {
                for (int y = -currentViewDistance; y <= currentViewDistance; y++)
                {
                    requiredChunks.Add(new Vector2Int(playerPos.x + x, playerPos.y + y));
                }
            }
        }
        Debug.Log($"[Server] Total required chunks around players: {requiredChunks.Count}");

        // 로드할 청크 (필요하고, 해금되었고, 현재 활성화되지 않음)
        int loadedCount = 0;
        foreach (var chunkPos in requiredChunks)
        {
            // ## 디버그: 청크 로드 조건 확인 로그 ##
            bool existsInLayout = chunkToLandIdMap.TryGetValue(chunkPos, out string landId);
            bool isUnlocked = existsInLayout && serverUnlockedLandIds.Contains(landId);
            bool isActive = activeChunkNetworkIds.ContainsKey(chunkPos);
            // Debug.Log($"[Server:UpdateChunks] Checking {chunkPos}: Exists={existsInLayout}, Unlocked={isUnlocked}, Active={isActive}");

            if (existsInLayout && isUnlocked && !isActive)
            {
                Debug.Log($"[Server] --> Loading chunk at {chunkPos} (Land: {landId})");
                LoadChunk(chunkPos);
                loadedCount++;
            }
        }
        Debug.Log($"[Server] Attempted to load {loadedCount} new chunks.");

        // 언로드할 청크 (현재 활성화되어 있지만 더 이상 필요하지 않음)
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var chunkPair in activeChunkNetworkIds)
        {
            bool shouldUnload = !requiredChunks.Contains(chunkPair.Key);
            if (shouldUnload)
            {
                // ## 디버그: 언로드 조건 확인 로그 ##
                // Debug.Log($"[Server:UpdateChunks] Marking chunk {chunkPair.Key} for unload (Reason: Out of view range)");
                chunksToUnload.Add(chunkPair.Key);
            }
            // ## 추가: 청크가 맵 정의에 없거나 잠겼을 경우에도 언로드 (이전 로직 제거됨 - 불필요) ##
        }

        int unloadedCount = 0;
        foreach (var chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
            unloadedCount++;
        }
        Debug.Log($"[Server] Unloaded {unloadedCount} chunks.");
        Debug.Log("[Server] Finished updating chunks.");
    }

    // 서버 전용: 실제 청크 생성 및 스폰
    void LoadChunk(Vector2Int chunkPosition)
    {
        if (!IsServer) return;
        if (activeChunkNetworkIds.ContainsKey(chunkPosition)) return;

        Vector3 worldPosition = new Vector3(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize, 0);
        GameObject newChunk = Instantiate(chunkPrefab, worldPosition, Quaternion.identity);
        newChunk.name = $"Chunk ({chunkPosition.x}, {chunkPosition.y})";

        NetworkObject chunkNetworkObject = newChunk.GetComponent<NetworkObject>();
        if (chunkNetworkObject == null)
        {
            Debug.LogError($"[Server:LoadChunk] Chunk Prefab is missing NetworkObject component! Destroying instance at {chunkPosition}");
            Destroy(newChunk);
            return;
        }

        ChunkData chunkData = newChunk.GetComponent<ChunkData>();
        if (chunkData != null)
        {
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
            chunkData.SetTileData(groundPositions, objectPositions);
            Debug.Log($"[Server:LoadChunk] Set initial data for chunk {chunkPosition}");
        }
        else
        {
            Debug.LogError($"[Server:LoadChunk] Chunk Prefab at {chunkPosition} is missing ChunkData component!");
        }

        chunkNetworkObject.Spawn(); // 클라이언트에도 생성 명령
        activeChunkNetworkIds.Add(chunkPosition, chunkNetworkObject.NetworkObjectId); // 네트워크 ID로 관리
        Debug.Log($"[Server:LoadChunk] Spawned chunk at {chunkPosition} with NetworkID {chunkNetworkObject.NetworkObjectId}");
    }

    // 클라이언트 전용: 벽 청크 생성 (네트워크 X)
    void LoadWallChunk(Vector2Int chunkPosition)
    {
        if (localWallChunks.ContainsKey(chunkPosition)) return;

        Vector3 worldPosition = new Vector3(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize, 0);
        GameObject newChunk = Instantiate(chunkPrefab, worldPosition, Quaternion.identity, this.transform);
        newChunk.name = $"WallChunk ({chunkPosition.x}, {chunkPosition.y})";

        if (newChunk.TryGetComponent<NetworkObject>(out var netObj)) netObj.enabled = false;
        if (newChunk.TryGetComponent<ChunkData>(out var chunkData)) chunkData.enabled = false;

        Tilemap groundTilemap = newChunk.transform.Find("GroundTilemap")?.GetComponent<Tilemap>();
        Tilemap objectTilemap = newChunk.transform.Find("ObjectTilemap")?.GetComponent<Tilemap>();
        if (objectTilemap != null) objectTilemap.ClearAllTiles();

        if (groundTilemap != null && wallTile != null)
        {
            TileBase[] wallTiles = new TileBase[chunkSize * chunkSize];
            Vector3Int[] wallPositions = new Vector3Int[chunkSize * chunkSize];
            int index = 0;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    wallPositions[index] = new Vector3Int(x, y, 0);
                    wallTiles[index] = wallTile;
                    index++;
                }
            }
            groundTilemap.SetTiles(wallPositions, wallTiles);
        }

        localWallChunks.Add(chunkPosition, newChunk);
        Debug.Log($"[Client:LoadWallChunk] Loaded wall chunk at {chunkPosition}");
    }

    // 서버 전용: 청크 언로드
    void UnloadChunk(Vector2Int chunkPosition)
    {
        if (!IsServer) return;

        if (activeChunkNetworkIds.TryGetValue(chunkPosition, out ulong networkId))
        {
            // NetworkManager나 SpawnManager가 null일 수 있음 (종료 시)
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
            {
                if (netObj != null)
                {
                    netObj.Despawn(); // Despawn 호출
                    Debug.Log($"[Server:UnloadChunk] Despawning chunk at {chunkPosition} (NetworkID: {networkId})");
                }
            }
            activeChunkNetworkIds.Remove(chunkPosition);
            // Debug.Log($"[Server] Unloaded chunk tracking for {chunkPosition}"); // 로그 레벨 조정
        }
        else
        {
            Debug.LogWarning($"[Server:UnloadChunk] Tried to unload chunk at {chunkPosition} but it was not in active list.");
        }
    }

    // 공개 함수: 청크 좌표로 랜드 ID 반환
    public string GetLandIdAt(Vector2Int chunkPosition)
    {
        chunkToLandIdMap.TryGetValue(chunkPosition, out string landId);
        return landId;
    }

    // 공개 함수: 특정 랜드 ID가 해금되었는지 확인 (클라이언트에서 사용)
    public bool IsLandUnlocked(string landId)
    {
        if (string.IsNullOrEmpty(landId)) return false; // ID가 없는 청크는 잠긴 것으로 간주
        return clientUnlockedLandIds.Contains(landId);
    }

    // 유틸리티 함수: 월드 좌표를 청크 좌표로 변환
    public Vector2Int GetChunkPositionFromWorld(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int y = Mathf.FloorToInt(worldPosition.y / chunkSize);
        return new Vector2Int(x, y);
    }

    // 유틸리티 함수: 맵 레이아웃 압축
    private string PackMapLayout()
    {
        if (chunkToLandIdMap == null || chunkToLandIdMap.Count == 0) return "";
        return string.Join(";", chunkToLandIdMap.Select(kvp => $"{kvp.Key.x},{kvp.Key.y},{kvp.Value}"));
    }

    // 유틸리티 함수: 맵 레이아웃 압축 해제
    private void UnpackMapLayout(string packedData)
    {
        chunkToLandIdMap.Clear();
        if (string.IsNullOrEmpty(packedData)) return;

        string[] entries = packedData.Split(';');
        foreach (var entry in entries)
        {
            string[] parts = entry.Split(',');
            if (parts.Length == 3 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                string landId = parts[2];
                chunkToLandIdMap[new Vector2Int(x, y)] = landId;
            }
        }
        Debug.Log($"[Client] Map layout unpacked. Total chunks defined: {chunkToLandIdMap.Count}");
    }
}

