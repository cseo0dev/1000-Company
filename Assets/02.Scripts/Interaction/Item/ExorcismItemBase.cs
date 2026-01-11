using System.Collections;
using Fusion;
using UnityEngine;

// 코드 담당자: 김수아

/// <summary>
/// 제령 아이템들의 부모 클래스
/// 공통적으로 귀신의 방 체크, 마법진 생성, 사용 성공/실패 처리를 담당합니다.
/// </summary>
public abstract class ExorcismItemBase : NetworkBehaviour, IUsable
{
    [Header("Exorcism Settings")]
    [SerializeField] protected NetworkPrefabRef magicCirclePrefab;
    [SerializeField] protected ItemData itemData;
    [SerializeField] protected float offsetY = 0.02f;

    protected Vector3 startPosition;

    protected virtual void Start()
    {
        startPosition = transform.position;
        transform.position = new Vector3(startPosition.x, startPosition.y + offsetY, startPosition.z);
    }

    public void Use()
    {
        Debug.Log("[ExorcismItemBase] Use() 호출 - 설치 아이템은 RPC_PlaceItem에서 처리됨");
    }

    // 플레이어 상호작용 시 호출 (클라에서 호출)
    public void TryUseFromPlayer(Vector3 usePos)
    {
        Debug.Log($"[TryUseFromPlayer] itemID={itemData.itemID}, pos={usePos}");
        // 클라/호스트 모두 해당 함수 사용 → 서버가 처리
        Rpc_RequestUse(usePos, itemData.itemID);
    }

    // 클라 → 서버
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RequestUse(Vector3 usePos, int itemId)
    {
        Debug.Log("[Bujeok] Rpc_RequestUse 수신됨 (서버 권한:" + Object.HasStateAuthority + ")");
        if (!Object.HasStateAuthority) return;
        Server_ProcessUse(usePos, itemId);
    }

    /// <summary>
    /// 서버 전용: 아이템 검증, 귀신 스폰, 마법진 시작
    /// InventoryManager.RPC_PlaceItem에서 호출
    /// </summary>
    public void Server_ProcessUse(Vector3 pos, int itemId)
    {
        if (!Object.HasStateAuthority) return;

        if (magicCirclePrefab.Equals(default(NetworkPrefabRef)))
        {
            OnUseFailed();
            return;
        }

        // 귀신의 방 확인
        var ghostRoom = FindFirstObjectByType<GhostRoom>();
        if (ghostRoom == null || !ghostRoom.IsInGhostRoom(transform.position))
        {
            // Rpc_PlayUseResult(false); // 연출용
            OnUseFailed();
            return;
        }

        // 이미 활성된 마법진 있으면 거기에 아이템 추가
        if (ghostRoom.TryGetActiveCircle(out var exist))
        {
            Debug.Log("[Exorcism] 기존 마법진에 추가");
            exist.Server_AddExorcismItem(itemId);
            // Rpc_PlayUseResult(true); // 연출용
            OnUseSuccess();
            return;
        }

        Debug.Log("[Exorcism] 기존 마법진 없음 → 새로 생성");

        // 마법진 동시 생성 방지
        if (ghostRoom.IsCircleSpawning)
        {
            Runner.StartCoroutine(WaitAndAddToActiveCircle(ghostRoom, itemId));
            // Rpc_PlayUseResult(false);
            OnUseFailed();
            return;
        }

        ghostRoom.IsCircleSpawning = true;

        // 새 마법진 생성(서버)
        Runner.Spawn(magicCirclePrefab, transform.position, Quaternion.identity, null, (runner, obj) =>
        {
            var magicCircle = obj.GetComponent<MagicCircleCore>();
            var room = FindFirstObjectByType<GhostRoom>();

            magicCircle.Server_SetOwnerRoom(ghostRoom);
            magicCircle.Server_Begin();
            magicCircle.Server_AddExorcismItem(itemId);
        });

        // Rpc_PlayUseResult(true); // 연출용
        OnUseSuccess();
    }

    private IEnumerator WaitAndAddToActiveCircle(GhostRoom room, int itemId)
    {
        float timeout = 1f;
        float t = 0f;
        while (t < timeout)
        {
            if (room.TryGetActiveCircle(out var circle)) // 활성화된 마법진 있으면 아이템 추가하고 종료
            {
                circle.Server_AddExorcismItem(itemId);
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        /* 등록이 실패했으면 마지막 방어로 새로 생성
        if (!room.IsCircleSpawning)
        {
            room.IsCircleSpawning = true;
            Runner.Spawn(magicCirclePrefab, transform.position, Quaternion.identity, null, (runner, obj) =>
            {
                var circle = obj.GetComponent<MagicCircle>();
                circle.Server_SetOwnerRoom(room);
                circle.Server_BeginExorcism();
                circle.Server_AddExorcismItem(itemId);
            });
        } */
    }

    // 서버 → 전체에 연출 전달
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void Rpc_PlayUseResult(bool ok)
    // {
    //     // 사운드/이펙트 등 클라 연출
    // }

    protected abstract void OnUseSuccess();

    // 기본적으로 성공과 동일하게 처리하고 자식 클래스에서 상황에 따라 수정
    protected virtual void OnUseFailed()
    {
        OnUseSuccess();
    }
}
