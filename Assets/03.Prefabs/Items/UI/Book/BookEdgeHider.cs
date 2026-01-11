// 코드 담당자 : 최서영
// 버그 해결해서 이 스크립트는 사용 안함!!
// 근데 혹시 또 버그 생길까봐 냅두겠습니다.
using UnityEngine;
using UnityEngine.UI;

public class BookEdgeHider : MonoBehaviour
{
    [SerializeField] Book book;
    [SerializeField] Graphic leftPage;
    [SerializeField] Graphic rightPage;

    // 총 페이지 수 -> 증가되면 수정 필요
    [SerializeField] int totalPages = 14;

    void LateUpdate()
    {
        if (!book) return;

        bool isFirst = book.currentPage <= 0;
        bool isLast = book.currentPage >= totalPages - 1;

        // 왼쪽 숨김(처음 페이지)
        SetSideVisibleLeft(!isFirst);

        // 오른쪽 숨김(마지막 페이지)
        SetSideVisibleRight(!isLast);
    }

    void SetSideVisibleLeft(bool show)
    {
        if (leftPage) leftPage.enabled = show;
    }

    void SetSideVisibleRight(bool show)
    {
        if (rightPage) rightPage.enabled = show;
    }
}
