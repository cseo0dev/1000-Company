// 코드 담당자 : 최우석
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PaperData
{
    public string id;
    public Sprite sprite;
}

public class DocumentUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject dimBackground;
    public Image paperViewImage;

    [Header("문서 데이터베이스")]
    public List<PaperData> paperDatabase = new List<PaperData>();

    private Dictionary<string, Sprite> paperLookup = new Dictionary<string, Sprite>();
    private bool isVisible = false;

    private void Awake()
    {
        foreach (var paper in paperDatabase)
        {
            if (paper.sprite != null && !string.IsNullOrEmpty(paper.id))
            {
                paperLookup[paper.id] = paper.sprite;
            }
        }
        HidePaperView(false);
    }

    public void ShowPaperView(string paperID)
    {
        if (paperLookup.TryGetValue(paperID, out Sprite paperToShow))
        {
            paperViewImage.sprite = paperToShow;
            dimBackground.SetActive(true);
            paperViewImage.gameObject.SetActive(true);
            isVisible = true;

            if (PlayerController.Local != null)
            {
                // PlayerController.cs에 이미 있는 SetPaused 함수를 호출합니다.
                PlayerController.Local.SetPaused(true);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogWarning($"Paper ID '{paperID}'를 찾을 수 없습니다.");
        }
    }

    public void HidePaperView(bool resumePlayer = true)
    {
        dimBackground.SetActive(false);
        paperViewImage.gameObject.SetActive(false);
        isVisible = false;

        if (resumePlayer)
        {
            if (PlayerController.Local != null)
            {
                // PlayerController.cs에 이미 있는 SetPaused 함수를 호출합니다.
                PlayerController.Local.SetPaused(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnCloseButtonClicked()
    {
        HidePaperView(true);
    }
}