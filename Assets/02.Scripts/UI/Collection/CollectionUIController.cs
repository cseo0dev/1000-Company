using System.Collections.Generic;
using UnityEngine;

// 작성자 : 정하윤
public class CollectionUIController : MonoBehaviour
{
    [SerializeField] private List<CollectionCell> cells;

    // 보기 버튼 클릭 시 나오는 설명창
    [SerializeField] private ItemExplainPopup explainPrefab;
    [SerializeField] private WarningPopup dropWarningPrefab;
    [SerializeField] private WarningPopup useWarningPrefab;

    // 수집한 아이템 리스트
    private List<ItemData> collectedItems = new List<ItemData>();

    private void Start()
    {
        // 테스트용 아이템 3개 생성 후 추가
        for (int i = 0; i < 5; i++)
        {
            ItemData item = new ItemData
            {
                itemName = $"TestItem{i + 1}",
                icon = null,
                description = $"이것은 TestItem{i + 1}의 설명입니다.",
                canUse = true
            };

            AddItem(item);
        }
    }

    // 아이템 추가
    public void AddItem(ItemData item)
    {
        // 최대 9칸 제한
        if (collectedItems.Count >= 9) 
            return; 

        collectedItems.Add(item);

        UpdateCells();
    }

    // Cell UI 갱신
    private void UpdateCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (i < collectedItems.Count)
            {
                // 버튼 이벤트 연결
                cells[i].Initialize
                    (
                        collectedItems[i],
                        onViewClicked: ShowExplain,
                        onUseClicked: ShowUseWarning,
                        onDropClicked: ShowDropWarning
                    );
            }
            else
            {
                // 아이템 데이터가 없으면 Cell 비활성화
                cells[i].Clear();
            }
        }
    }

    // View 버튼 클릭 시 나올 설명창 표시
    private void ShowExplain(ItemData item)
    {
        explainPrefab.Show(item);
    }

    // 버리기 버튼 클릭 시 나올 경고창 표시
    private void ShowDropWarning(ItemData item)
    {
        dropWarningPrefab.Show(item);
        dropWarningPrefab.Open(() =>
        {
            DropItem(item); 
        });
    }

    // 사용 버튼 클릭 시 나올 경고창 표시
    private void ShowUseWarning(ItemData item)
    {
        useWarningPrefab.Show(item);
        useWarningPrefab.Open(() =>
        {
            UseItem(item);
        });
    }

    // Use 버튼 클릭 시 아이템 사용
    private void UseItem(ItemData item)
    {
        if(!item.canUse)
        {
            Debug.Log("사용 아이템이 아닙니다");
            return;
        }

        Debug.Log($"Use {item.itemName}");

        // 실제 아이템 사용 로직 추가
        collectedItems.Remove(item);
        UpdateCells();
    }

    // Drop 버튼 클릭 시 아이템 제거
    private void DropItem(ItemData item)
    {
        Debug.Log($"{item.itemName}을 버렸습니다");

        collectedItems.Remove(item);
        UpdateCells();
    }

    // 제령 아이템만 수집품 목록에서 제거
    public void RemoveExorcismItems()
    {
        collectedItems.RemoveAll(item => item.itemType == ItemData.EItemType.Exorcism);
        UpdateCells();
    }
}
