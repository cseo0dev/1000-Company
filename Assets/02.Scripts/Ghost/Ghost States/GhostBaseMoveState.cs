using UnityEngine;
using UnityEngine.AI;

// 코드 담당자: 김수아

/// <summary>
/// Patrol, Chase, Hunting시 사용할 기본 움직임에 관한 클래스
/// </summary>
public abstract class GhostBaseMoveState : GhostBaseState
{
    public override GhostController.EGhostState State { get; } // 하위 클래스에서 상태 구현

    public GhostBaseMoveState(GhostController ghost) : base(ghost) { }

    public override void EnterState() { }

    public override void ExecuteState() { }

    public override void ExitState() { }

    protected void SetRandomDestination()
    {
        if (ghost.Agent && ghost.Agent.enabled && ghost.Agent.isOnNavMesh)
        {
            Vector3 randomDir = Random.insideUnitSphere * ghost.WanderRadius + ghost.transform.position;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, ghost.WanderRadius, NavMesh.AllAreas))
            {
                ghost.Agent.SetDestination(hit.position);
            }
        }
    }
}
