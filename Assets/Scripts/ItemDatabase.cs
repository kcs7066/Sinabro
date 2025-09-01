using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 게임 내의 모든 ItemData를 관리하고 쉽게 찾아올 수 있게 해줍니다.
public class ItemDatabase : MonoBehaviour
{
    // --- Singleton Pattern Start ---
    // 이제 어디서든 ItemDatabase.Instance 로 이 스크립트에 접근할 수 있습니다.
    public static ItemDatabase Instance { get; private set; }

    void Awake()
    {
        // 만약 이미 Instance가 존재하는데, 그게 내가 아니라면 스스로를 파괴합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 아니라면, 자기 자신을 Instance로 등록합니다.
            Instance = this;
        }
        // --- Singleton Pattern End ---

        // 게임이 시작될 때, 모든 아이템을 Dictionary에 등록하여 빠르게 찾을 수 있도록 준비합니다.
        foreach (var item in allItems)
        {
            if (item != null && !itemLookup.ContainsKey(item.itemID))
            {
                itemLookup.Add(item.itemID, item);
            }
        }
    }

    public List<ItemData> allItems;
    private Dictionary<int, ItemData> itemLookup = new Dictionary<int, ItemData>();

    public ItemData GetItemById(int id)
    {
        if (id == -1 || !itemLookup.TryGetValue(id, out ItemData item))
        {
            return null;
        }
        return item;
    }
}
