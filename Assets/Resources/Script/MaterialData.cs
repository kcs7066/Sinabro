using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Inventory/Material")]
public class MaterialData : ItemData
{
    [Header("재료 정보")]
    public int maxStack = 999;
}