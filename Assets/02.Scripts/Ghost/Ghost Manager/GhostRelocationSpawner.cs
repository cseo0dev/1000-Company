using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

// 코드 담당자: 김수아

/// <summary>
/// 귀신이 방에 갇히거나 끼어있을 때 재스폰시키는 용도
/// </summary>
public class GhostRelocationSpawner : NetworkBehaviour
{
    [Header("Refs")]
    private GhostSpawner spawner;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Relocate")]
    [SerializeField] private float checkInterval = 0.5f; // 위치 감시 주기
    [SerializeField] private float stuckDuration = 2f; // 정지 판단 누적 시간
    [SerializeField] private float positionThreshold = 0.5f;
    [SerializeField] private float safeDistance = 3f; // 플레이어와 거리
    [SerializeField] private bool verboseLog = true;

    private GhostSpawner _spawner;
    private Coroutine _watchRoutine;


    public override void Spawned()
    {
        _spawner = GhostSpawner.Instance;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || _spawner == null) return;

        var ghost = _spawner.GhostController;
        bool hasGhost = ghost != null && ghost.Object && ghost.Object.IsValid;

        if (hasGhost && _watchRoutine == null)
            _watchRoutine = StartCoroutine(WatchRoutine(ghost));
        else if (!hasGhost && _watchRoutine != null)
        {
            StopCoroutine(_watchRoutine);
            _watchRoutine = null;
        }
    }

    private IEnumerator WatchRoutine(GhostController gc)
    {
        Vector3 lastPos = gc.transform.position;
        float stuckTimer = 0f;

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (gc == null || gc.Agent == null || !gc.Agent.enabled)
                continue;

            if (gc.CurrentState == null)
                continue;

            var stateEnum = gc.CurrentState.State;

            if (stateEnum != GhostController.EGhostState.Chase && stateEnum != GhostController.EGhostState.Patrol)
                continue;

            // 1.플레이어가 Navmesh 밖인지 감시
            if (!gc.Agent.enabled)
            {
                gc.Agent.enabled = true;
                gc.Agent.Warp(gc.transform.position);
            }

            // NavMesh 밖 감지 → relocate
            if (!gc.Agent.isOnNavMesh)
            {
                if (verboseLog)
                    Debug.Log("[Relocate] Agent off NavMesh!");
                TryForceRelocate(gc);
                continue;
            }

            // 타깃이 NavMesh 밖에 있으면 relocate
            if (gc.TargetPlayer != null &&
                !NavMesh.SamplePosition(gc.TargetPlayer.position, out _, 0.2f, NavMesh.AllAreas))
            {
                if (verboseLog)
                    Debug.Log("[Relocate] Target player off NavMesh!");
                TryForceRelocate(gc);
                continue;
            }

            // 2.일정 시간 동안 멈춰있음
            float movedDist = Vector3.Distance(gc.transform.position, lastPos);

            if (movedDist < positionThreshold)
            {
                stuckTimer += checkInterval;
                if (stuckTimer >= stuckDuration)
                {
                    if (verboseLog)
                        Debug.Log($"[Relocate] Ghost stuck for {stuckDuration}s");
                    TryForceRelocate(gc);
                    stuckTimer = 0f;
                }
            }
            else
                stuckTimer = 0f;

            lastPos = gc.transform.position;
        }
    }

    private void TryForceRelocate(GhostController gc)
    {
        if (!TryRelocate(gc, out Vector3 pos)) return;

        SoftRelocate(gc, pos);
        gc.ChangeState(GhostController.EGhostState.Patrol);

        if (verboseLog)
            Debug.Log($"[Relocate] Ghost relocated to {pos}");
    }

    private bool TryRelocate(GhostController gc, out Vector3 chosen)
    {
        chosen = default;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        foreach (var sp in spawnPoints.OrderBy(_ => Random.value))
        {
            if (!NavMesh.SamplePosition(sp.position, out var hit, 0.2f, NavMesh.AllAreas))
                continue;

            Vector3 candidate = hit.position;

            // 플레이어 거리 확인
            if (!IsSafeFromPlayers(candidate, safeDistance))
                continue;

            chosen = candidate;
            return true;
        }

        return false;
    }

    private bool IsSafeFromPlayers(Vector3 pos, float minDist)
    {
        foreach (var pc in ServerPlayerRegistry.Players)
        {
            if (pc == null || pc.IsDead) continue;
            if (Vector3.Distance(pc.transform.position, pos) < minDist)
                return false;
        }
        return true;
    }

    private void SoftRelocate(GhostController gc, Vector3 pos)
    {
        if (!Object.HasStateAuthority) return;

        gc.Agent.enabled = false;
        gc.transform.position = pos;
        gc.Agent.Warp(pos);
        gc.Agent.enabled = true;
        gc.Agent.isStopped = false;

        gc.ResetAllAnimation();
        gc.Agent.ResetPath();
    }

    // 외부에서 호출해 사용
    public void RequestRelocate(GhostController gc, string reason, GhostController.EGhostState nextState = GhostController.EGhostState.Patrol)
    {
        if (!Object.HasStateAuthority) return;

        if (!TryRelocate(gc, out Vector3 pos))
        {
            if (verboseLog)
                Debug.LogWarning($"[Relocate] Failed to find new spawn for {reason}");
            return;
        }

        SoftRelocate(gc, pos);
        gc.ChangeState(nextState);

        if (verboseLog)
            Debug.Log($"[Relocate] {reason} → moved to {pos}");
    }
}

