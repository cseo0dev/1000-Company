using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 코드 담당자: 김수아

public class GhostStatePatrol : GhostBaseMoveState
{
    private float _patrolTimer;
    private float _patrolDuration;

    public override GhostController.EGhostState State => GhostController.EGhostState.Patrol;

    public GhostStatePatrol(GhostController ghost) : base(ghost) { }

    public override void EnterState()
    {
        ghost.Agent.speed = ghost.GhostMoveSpeed;
        _patrolDuration = Random.Range(5f, 10f);
        _patrolTimer = 0f;

        bool isCrawling = (GhostSpawner.Instance.ExorcismState == GhostSpawner.EExorcismState.Failed) ? false : ghost.IsCeilingSpawn;
        ghost.Animator.SetBool(GhostAnimParams.GhostWalk, true);
        ghost.Animator.SetBool(GhostAnimParams.GhostIsCrawling, isCrawling); // 천장 스폰이면 true, 아니면 false

        ghost.FX?.ChangePos(isCrawling);
        SetRandomDestination();
    }

    public override void ExecuteState()
    {
        if (!ghost.Object.HasStateAuthority) return;

        // 플레이어를 찾으면 Chase 상태로 전환
        if (ghost.FindPlayerRegisteredPlayer(out Transform player))
        {
            ghost.TargetPlayer = player;
            ghost.ChangeState(GhostController.EGhostState.Chase);
            return;
        }

        // 순찰 타임아웃
        _patrolTimer += ghost.Runner.DeltaTime;
        var exorcismState = GhostSpawner.Instance.ExorcismState;

        if (_patrolTimer > _patrolDuration)
        {
            _patrolTimer = 0f;

            if (exorcismState == GhostSpawner.EExorcismState.Failed)
                GhostSpawner.Instance.relocateSpawner.RequestRelocate(ghost, "patrol timeout", GhostController.EGhostState.Patrol);

            else
                ghost.Disappear();

            return;
        }

        // 목적지에 도착하면 새로운 목적지 설정
        if (!ghost.Agent.pathPending && ghost.Agent.remainingDistance < ghost.Agent.stoppingDistance)
        {
            SetRandomDestination();
        }
    }

    public override void ExitState() { }
}
