using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 게임 내의 모든 ItemData를 관리하고 쉽게 찾아올 수 있게 해줍니다.
public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> allItems; // Inspector에서 모든 아이템 에셋을 연결합니다.
    private Dictionary<int, ItemData> itemLookup = new Dictionary<int, ItemData>();

    void Awake()
    {
        // 게임이 시작될 때, 모든 아이템을 Dictionary에 등록하여 빠르게 찾을 수 있도록 준비합니다.
        foreach (var item in allItems)
        {
            if (item != null && !itemLookup.ContainsKey(item.itemID))
            {
                itemLookup.Add(item.itemID, item);
            }
        }
    }

    // ID를 받아서 해당하는 ItemData를 반환하는 함수
    public ItemData GetItemById(int id)
    {
        // 만약 ID가 -1이거나 Dictionary에 없는 ID라면 null을 반환합니다.
        if (id == -1 || !itemLookup.TryGetValue(id, out ItemData item))
        {
            return null;
        }
        return item;
    }
}
