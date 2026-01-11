// 코드 담당자 : 최서영
using UnityEngine;

public class GuideBook : MonoBehaviour, IUsable
{
    static Book _cachedUIBook;
    bool _isOpen;
    Animator viewModelAnimator; //유호정 추가

    void OnEnable()
    {
        viewModelAnimator = GetComponentInParent<Animator>(); //유호정 추가
        if (viewModelAnimator != null)
        {
            viewModelAnimator.SetInteger("EquippedItemType", 1);
        }

        ForceClose();
    }
    void OnDisable()
    {
        if (viewModelAnimator != null) //유호정 추가
        {
            viewModelAnimator.SetInteger("EquippedItemType", 1);
        }

        ForceClose();
    }

    // 로컬(HasInputAuthority) 인벤토리 찾기
    InventoryManager FindLocalInventory()
    {
        InventoryManager target = null;
        var invs = FindObjectsByType<InventoryManager>(FindObjectsSortMode.None);

        foreach (var inv in invs)
        {
            if (inv != null && inv.HasInputAuthority)
            {
                target = inv;
                break;
            }
        }

        return target;
    }

    Book ResolveUIBook()
    {
        if (_cachedUIBook && _cachedUIBook.gameObject)
            return _cachedUIBook;

        // 태그 추적
        //var tagged = GameObject.FindGameObjectWithTag("UI_GuideBook");
        //if (tagged)
        //{
        //    var b = tagged.GetComponentInChildren<Book>(true);
        //    if (b && b.GetComponentInParent<Canvas>(true))
        //        return _cachedUIBook = b;
        //}

        // Canvas의 자식인 Book 오브젝트 가져오기 (book.cs가 붙은 오브젝트)
        // book.cs => 책 넘기는 애니메이션 스크립트 (에셋임)
        var books = FindObjectsByType<Book>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in books)
        {
            if (b && b.GetComponentInParent<Canvas>(true))
                return _cachedUIBook = b;
        }

        return null;
    }
    
    public void Use()
    {
        var book = ResolveUIBook();
        if (!book)
            return;

        // 책이 열려있으면 닫고, 닫혀있으면 열도록
        _isOpen = !book.gameObject.activeSelf;
        book.gameObject.SetActive(_isOpen);

        if (_isOpen)
        {
            // 0페이지 부터 시작
            book.currentPage = 0;
            book.UpdateSprites();

            if (viewModelAnimator != null) viewModelAnimator.SetInteger("EquippedItemType", 3); //유호정 추가
        }
        else
        {
            if (viewModelAnimator != null) viewModelAnimator.SetInteger("EquippedItemType", 1); //유호정 추가
        }

        // 인벤토리 휠 스크롤 잠금/해제 (로컬 인벤토리만)
        var localInv = FindLocalInventory();
        if (localInv != null)
        {
            // 책이 열려 있을 때만 휠 잠금, 닫히면 바로 복구
            localInv.SetScrollPaused(_isOpen);
            // Debug.Log($"[GuideBook] 로컬 인벤토리 움직임 여부 : {_isOpen}");
        }
    }

    // 책/휠 상태 강제 초기화 (드랍/해제 시)
    // TODO : 나중에 시간되면 use에 구현한거랑 중복된 부분 많으니 함수로 만들기
    void ForceClose()
    {
        var book = ResolveUIBook();
        if (book)
        {
            if (book.gameObject.activeSelf || book.currentPage != 0)
            {
                book.currentPage = 0;
                book.UpdateSprites();
                book.gameObject.SetActive(false);
            }
        }

        _isOpen = false;

        // 로컬 인벤토리만 휠 해제 (다른 플레이어 영향 X)
        var localInv = FindLocalInventory();
        if (localInv != null)
        {
            localInv.SetScrollPaused(false);
        }
    }
}
