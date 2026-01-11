using Fusion;
using UnityEngine;

public class Lamp : MonoBehaviour, IUsable
{
    [SerializeField] public GameObject lightObj;

    private InventoryManager _ownerManager;

    // 램프 객체의 현재 시각적 상태를 저장할 로컬 변수
    private bool _currentVisualState = false;

    Animator viewModelAnimator; //유호정 추가

    void OnEnable()
    {
        viewModelAnimator = GetComponentInParent<Animator>();
        if (viewModelAnimator != null)
        {
            viewModelAnimator.SetInteger("EquippedItemType", 2);
        }
    }
    void OnDisable()
    {
        if (viewModelAnimator != null)
        {
            viewModelAnimator.SetInteger("EquippedItemType", 1);
        }
    }

    public void Initialize(InventoryManager owner, ItemData data)
    {
        _ownerManager = owner;

        if (lightObj != null)
            lightObj.SetActive(_currentVisualState);
    }

    public void Use()
    {
        if (_ownerManager != null && _ownerManager.HasInputAuthority)
        {
            // 서버에 램프 토글 요청 RPC 보냄
            _ownerManager.RPC_RequestToggleLamp();
        }
        else
        {
            Debug.LogWarning("Lamp: OwnerManager가 없거나 권한이 없습니다!");
        }
    }

    // 서버/클라 공통 시각 업데이트용
    public void SetLightVisual(bool isOn)
    {
        // 현재 시각적 상태를 변수에 저장
        _currentVisualState = isOn;

        if (lightObj != null)
            lightObj.SetActive(isOn);
    }
}
