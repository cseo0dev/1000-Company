//코드 담당자: 유호정
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabase : MonoBehaviour //resources/ItemData 폴더의 모든 ItemData를 불러와 ID로 조회할 수 있음
{
    public static ItemDatabase Instance { get; private set; }
    public List<ItemData> AllItems; 

    private Dictionary<int, ItemData> _itemDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        

        var itemsFromResources = Resources.LoadAll<ItemData>("ItemData");
        AllItems = new List<ItemData>(itemsFromResources);
        _itemDictionary = new Dictionary<int, ItemData>();
        

        foreach (var itemData in itemsFromResources)
        {
            if (itemData != null && !_itemDictionary.ContainsKey(itemData.itemID))
            {
                _itemDictionary.Add(itemData.itemID, itemData);
            }
            else
            {
                Debug.LogError($"ItemDatabase: 중복된 ID거나 ItemData가 null입니다: ID {itemData?.itemID}");
            }
        }

    }

    public static ItemData GetItemDataFromID(int id)
    {
        if (Instance == null)
        {
            Debug.LogError("ItemDatabase 인스턴스가 씬에 없습니다.");
            return null;
        }

        if (Instance._itemDictionary.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }
        
        Debug.LogWarning($"ItemDatabase: ID {id}에 해당하는 ItemData를 찾을 수 없습니다.");
        return null;
    }
}