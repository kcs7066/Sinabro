using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Land Data", menuName = "World/Land Data")]
public class LandData : ScriptableObject
{
    public string landId; // ��: "A1", "T", "Z1"

    // ## �߰�: ���� �׷����� ���� ���� ##
    public string groupId; // ��: "A", "B", "F" (�׷��� ������ ����Ӵϴ�)

    public List<Vector2Int> chunkOffsets; // �� ���带 �����ϴ� ûũ���� ����� ��ġ
}

