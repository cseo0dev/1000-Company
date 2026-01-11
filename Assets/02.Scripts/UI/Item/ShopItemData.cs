using System;
using UnityEngine;

// 작성자 : 정하윤
[Serializable]
[CreateAssetMenu(fileName = "ShopItemData", menuName = "Inventory/ShopItemData")]
public class ShopItemData : ScriptableObject
{ 
    public ItemData data;

    public int requiredMileage;     // 구매 기회 소진 후 재구매를 위한 필요 마일리지
    public bool freeAvailable = true;
    public bool isSoldOut = false;
    public int itemPrice; // 추가: 김수아
}