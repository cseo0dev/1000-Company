using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 작성자 : 정하윤
// Explain 프리팹에 할당되는 스크립트
public class ItemExplainPopup : ItemPopupBase
{
    [SerializeField] protected TextMeshProUGUI itemNameText;
    [SerializeField] protected Image icon;
    [SerializeField] protected TextMeshProUGUI descriptionText;

    public override void Initialize()
    {
        icon.sprite = itemData.icon;
        itemNameText.text = itemData.itemName;
        descriptionText.text = itemData.description;
    }
}
