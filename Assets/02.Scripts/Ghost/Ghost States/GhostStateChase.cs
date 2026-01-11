using System.Collections;
using System.Threading;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

// 코드 담당자: 김수아
public class GhostStateChase : GhostBaseMoveState
{
    private float _loseTimer; // 플레이어를 놓친 시간
    private const float LostDelay = 1f; // 놓친 후 유예 시간
    private bool _canDisappear; // 즉시 사라지지 않도록 보호
    private TickTimer _spawnGraceTime; // 소환 직후 사라짐 방지 시간

    // 목적지 업데이트 주기 & NavMesh 샘플 반경
    private const float UpdateInterval = 0.5f;
    private float _updateTimer;

    // 제자리 감지
    private Vector3 _lastPosition;
    private float _stuckTimer = 0f;
    private const float StuckThreshold = 2f; // 이 시간 이상 같은 위치면 relocate
    private const float PositionThreshold = 0.08f; // 이 거리 이내면 같은 위치로 판단

    private Vector3 _lastDest;

    public override GhostController.EGhostState State => GhostController.EGhostState.Chase;

    public GhostStateChase(GhostController ghost) : base(ghost) { }

    public override void EnterState()
    {
        ghost.Agent.isStopped = false;
        ghost.Agent.speed = ghost.GhostMoveSpeed * 2f;

        _loseTimer = 0f;
        _stuckTimer = 0f;
        _updateTimer = 0f;
        _lastPosition = ghost.transform.position; // 현재 위치 저장

        _spawnGraceTime = TickTimer.CreateFromSeconds(ghost.Runner, 2f);
        _canDisappear = false;

        // 애니메이션, 사운드
        ghost.Animator.SetBool(GhostAnimParams.GhostIdle, false);
        bool isCrawling = ghost.Animator.GetBool(GhostAnimParams.GhostIsCrawling);
        ghost.Animator.SetBool(GhostAnimParams.GhostChase, true);
        ghost.FX?.ChangePos(isCrawling);
        ghost.Sound?.Rpc_PlayLoop(EGhostSound.ChaseLoop, 0.3f);
    }


    public override void ExecuteState()
    {
        if (!ghost.Object.HasStateAuthority) return;

        // 소환 후 일정 시간 동안 사라짐 방지
        if (_spawnGraceTime.Expired(ghost.Runner)) _canDisappear = true;

        // 최적화용 (0.5초마다 계산)
        _updateTimer += ghost.Runner.DeltaTime;
        if (_updateTimer < UpdateInterval) return;
        _updateTimer = 0f;

        // 타겟 없으면 새로 찾기
        if (ghost.TargetPlayer == null)
        {
            if (ghost.FindPlayerRegisteredPlayer(out var found))
            {
                ghost.TargetPlayer = found;
                _loseTimer = 0f;
            }
        }

        float dist = Vector3.Distance(ghost.transform.position, ghost.TargetPlayer.position);
        bool isReal = GhostSpawner.Instance.ExorcismState == GhostSpawner.EExorcismState.Failed;

        // // 최대 추격 거리 초과
        if (dist > ghost.PatrolDetectionDistance)
        {
            if (isReal)
            {
                Debug.Log("[Chase] lost target → RELOCATE (real)");
                GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "chase: target too far", GhostController.EGhostState.Patrol);
                return;
            }
            else
            {
                Debug.Log("[Chase] target too far → DISAPPEAR (black)");
                ghost.Disappear();
                return;
            }
        }

        // 장애물 뒤에 플레이어가 숨은 경우
        if (!ghost.HasLineOfSight(ghost.TargetPlayer))
        {
            if (isReal)
            {
                GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "chase: lost line of sight", GhostController.EGhostState.Patrol);
                return;
            }

            ghost.TargetPlayer = null;
        }

        if (ghost.TargetPlayer == null)
        {
            _loseTimer += UpdateInterval;

            if (_loseTimer > LostDelay && _canDisappear)
            {
                if (GhostSpawner.Instance.ExorcismState == GhostSpawner.EExorcismState.Failed)
                {
                    GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "lost target", GhostController.EGhostState.Patrol);
                    return;
                }

                ghost.Disappear();
            }

            return;
        }

        // 공격 진입
        float attackRange = 1.5f;
        if (dist <= attackRange && ghost.Agent.hasPath && ghost.Agent.remainingDistance <= attackRange)
        {
            ghost.ChangeState(GhostController.EGhostState.Attack);
            return;
        }

        // 플레이어가 NavMesh 밖에 있으면 relocate
        if (!NavMesh.SamplePosition(ghost.TargetPlayer.position, out var playerNavHit, 0.2f, NavMesh.AllAreas))
        {
            GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "player off-navmesh", GhostController.EGhostState.Patrol);
            return;
        }

        // 제자리 감지 → relocate
        float moved = Vector3.Distance(ghost.transform.position, _lastPosition);
        if (moved < PositionThreshold)
        {
            _stuckTimer += UpdateInterval;
            if (_stuckTimer >= StuckThreshold)
            {
                _stuckTimer = 0f;
                ghost.TargetPlayer = null;
                GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "chase: stuck", GhostController.EGhostState.Patrol);
                return;
            }
        }
        else
        {
            _stuckTimer = 0f;
            _lastPosition = ghost.transform.position;
        }

        // 이전 목적지와 0.5m 이상 차이날때만 SetDesination으로 목적지 갱신
        Vector3 newDest = playerNavHit.position;
        if (Vector3.Distance(newDest, _lastDest) > 0.5f)
        {
            ghost.Agent.SetDestination(newDest);
            _lastDest = newDest;
        }

        _loseTimer = 0f;
    }

    public override void ExitState()
    {
        ghost.Animator.SetBool(GhostAnimParams.GhostChase, false);
        ghost.Sound?.Rpc_StopLoop(1f);
    }
}

