// 코드 담당자 : 최우석
using UnityEngine;
using Fusion;

public class DocumentInteraction : NetworkBehaviour, IInteractable
{
    [Header("UI 설정")]
    [Tooltip("UI 매니저가 식별할 고유 ID (예: 'diary_page_1')")]
    public string paperViewID;

    private DocumentUI uiManager;

    // --- IInteractable 인터페이스 구현 ---

    public void Interact(GameObject interactor)
    {
        NetworkObject playerNetworkObject = interactor.GetComponent<NetworkObject>();
        if (playerNetworkObject == null) return;

        PlayerRef interactorPlayerRef = playerNetworkObject.InputAuthority;

        // [에러 1 수정] 'IsValid' 대신 'PlayerRef.None'과 비교
        if (interactorPlayerRef == PlayerRef.None) return;

        Rpc_ShowPaperView(interactorPlayerRef, paperViewID);
    }

    // [에러 2 수정] 'RpcTargets.Player' 대신 'RpcTargets.All' + 내부 필터링
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ShowPaperView(PlayerRef targetPlayer, string paperID)
    {
        // 상호작용한 플레이어 본인만 UI를 띄움
        if (targetPlayer != Runner.LocalPlayer) return;

        if (uiManager == null)
        {
            // [이번 에러 수정]
            // 'Object.FindFirstObjectByType'이 아닌, 'FindFirstObjectByType'으로 바로 호출합니다.
            uiManager = FindFirstObjectByType<DocumentUI>();
        }

        if (uiManager != null)
        {
            uiManager.ShowPaperView(paperID);
        }
    }

    // --- 아웃라인 함수 ---

    public void EnableOutline()
    {
        // 예: GetComponentInChildren<Outline>(true).enabled = true;
    }

    public void DisableOutline()
    {
        // 예: GetComponentInChildren<Outline>(true).enabled = false;
    }
}