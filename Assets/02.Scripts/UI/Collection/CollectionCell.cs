using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 작성자 : 정하윤
public class CollectionCell : Cell
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private Image icon;

    [SerializeField] private Button viewButton;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;

    // 현재 Cell에 할당된 아이템
    private ItemData currentItem;

    // 아이템 데이터 세팅과 버튼 이벤트 연결
    public void Initialize(ItemData item, Action<ItemData> onViewClicked, Action<ItemData> onUseClicked, Action<ItemData> onDropClicked)
    {
        currentItem = item;

        // 아이템이 있으면 활성화
        bool hasItem = item != null;

        icon.gameObject.SetActive(hasItem);
        itemName.gameObject.SetActive(hasItem);
        dropButton.gameObject.SetActive(hasItem);
        viewButton.gameObject.SetActive(hasItem);
        useButton.gameObject.SetActive(hasItem && item.canUse);

        if (hasItem)
        {
            // UI 내용 갱신
            icon.sprite = item.icon;
            itemName.text = item.itemName;

            // 버튼 이벤트 초기화 후 연결
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(() => onDropClicked?.Invoke(currentItem));

            viewButton.onClick.RemoveAllListeners();
            viewButton.onClick.AddListener(() => onViewClicked?.Invoke(currentItem));

            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => onUseClicked?.Invoke(currentItem));
        }
    }

    public void SetItem(ItemData item, int index)
    {
        currentItem = item;
        itemName.text = currentItem.itemName;
        Index = index;
    }

    public void Clear()
    {
        currentItem = null;
        icon.gameObject.SetActive(false);
        itemName.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
        viewButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
    }
}
