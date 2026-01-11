//코드 담당자: 유호정

using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera playerCamera;
    public float interactionDistance = 5f;
    public LayerMask interactionLayer;

    public IInteractable CurrentInteractable { get; set; }
    // 현재 레이캐스트가 유효한 표면에 닿았는지 여부
    public bool HasValidHit { get; private set; }
    // 현재 레이캐스트가 닿은 표면의 상세 정보 (위치, 법선 등)
    public RaycastHit HitInfo { get; private set; }
    private IInteractable lastInteractable;
    private PlayerController _playerController;


    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_playerController == null || !_playerController.HasInputAuthority)
        {
            lastInteractable?.DisableOutline();
            lastInteractable = null;
            CurrentInteractable = null;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        HasValidHit = Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer);

        // 코드수정: 김수아
        IInteractable newInteractable = null;
        if (HasValidHit)
        {
            HitInfo = hit;

            newInteractable = hit.collider.GetComponentInParent<IInteractable>();
        }

        // 코드추가: 김수아
        // Raycast가 아무것도 안 맞았을 때 KioskTrigger에서 지정해둔 CurrentInteractable을 그대로 유지합니다.
        if (newInteractable != null)
        {
            CurrentInteractable = newInteractable;
        }

        if (lastInteractable != CurrentInteractable)
        {
            lastInteractable?.DisableOutline();
            CurrentInteractable?.EnableOutline();
            lastInteractable = CurrentInteractable;
        }
    }


    public void OnInteract(InputValue value)
    {
        if (_playerController == null || !_playerController.HasInputAuthority || CurrentInteractable == null)
        {
            return;
        }
        NetworkBehaviour interactableNB = CurrentInteractable as NetworkBehaviour;


        // 추가: 김수아
        bool canUseNetworkPath = interactableNB != null && interactableNB.Object != null && _playerController.Runner && _playerController.Runner.IsRunning;

        // 키오스크는 로컬UI
        if (CurrentInteractable is KioskTrigger || CurrentInteractable is ReturnTrigger)
        {
            CurrentInteractable.Interact(gameObject);
        }
        else if (canUseNetworkPath) // 기존 네트워크 상호작용
        {
            _playerController.RequestInteract(interactableNB.Object.Id);
        }
    }
}