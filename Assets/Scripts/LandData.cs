using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Land Data", menuName = "World/Land Data")]
public class LandData : ScriptableObject
{
    public string landId; // 예: "A1", "T", "Z1"

    // ## 추가: 랜드 그룹핑을 위한 변수 ##
    public string groupId; // 예: "A", "B", "F" (그룹이 없으면 비워둡니다)

    public List<Vector2Int> chunkOffsets; // 이 랜드를 구성하는 청크들의 상대적 위치
}

