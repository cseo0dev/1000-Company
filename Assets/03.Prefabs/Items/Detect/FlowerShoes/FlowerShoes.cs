// 코드 담당자 : 최서영
using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // 꽃신은 돌아다녀야 해서 NavMesh 필요
public class FlowerShoes : NetworkBehaviour
{
    private ItemData itemData;
    private GhostSpawner ghostSpawner;
    private NavMeshAgent agent;
    private Animator animator;

    // Move 관련 변수
    private float moveRadius = 5f; // 목적지 반경
    private float minWait = 5f; // 최소 대기 시간
    private float maxWait = 15f; // 최대 대기 시간

    private int maxDestinationTries = 10; // 유효 목적지 탐색 재시도 횟수
    private float sampleMaxDistance = 1.0f; // NavMesh.SamplePosition 반경

    private float moveTimeoutSeconds = 3f; // 목적지 향해 걷는 최대 시간
    private float moveElapsed = 0f;   // 현재 목적지를 향해 이동한 누적 시간

    private Vector3 dest; // 목적지 좌표
    float waitRemain = 0f; // 대기 시간

    enum Phase { Idle, Waiting, Moving }
    Phase phase = Phase.Idle;

    private readonly int FlowerShoesAnimMove = Animator.StringToHash("Move");
    [Networked] public float FlowerShoesAnim { get; set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        itemData = GetComponent<ItemObject>()?.itemData;
    }

    void OnEnable()
    {
        phase = Phase.Idle;
        waitRemain = 0f;
        moveElapsed = 0f;
        AgentStop();
    }

    public override void Spawned()
    {
        // Awake 전에 호출되는 상황 대비용
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
        if (!animator)
            animator = GetComponent<Animator>();
        if (!itemData)
            itemData = GetComponent<ItemObject>()?.itemData;

        // 호스트만
        if (!Object || !Object.HasStateAuthority || !agent)
            return;

        agent.enabled = false; // 에이전트가 임의로 움직이기 전에 잠깐 꺼두기

        Vector3 desired = transform.position; // 층간 스폰용

        if (NavMesh.SamplePosition(desired, out var hit, 0.5f, NavMesh.AllAreas))
        {
            desired = hit.position;
        }

        transform.position = desired; // 트랜스폼 먼저 맞추기

        // 에이전트 활성화 + NavMesh 상의 위치 강제로 확정
        agent.enabled = true;
        agent.Warp(desired);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object)
            return;

        // NavMeshAgent 비활성
        if (!Object.HasStateAuthority)
        {
            if (agent)
            {
                if (agent.enabled)
                {
                    agent.isStopped = true;
                    if (agent.hasPath)
                        agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    agent.enabled = false; // 완전히 꺼버리기
                }
            }
            return;
        }

        if (agent && !agent.enabled)
            agent.enabled = true;

        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.Instance;

        if (ghostSpawner == null || itemData == null || itemData.canDetect == null)
        {
            AgentStop();
            return;
        }

        bool canDetectThisGhost = itemData.canDetect.Contains(ghostSpawner.mapGhostType);

        if (!canDetectThisGhost)
        {
            phase = Phase.Idle;
            moveElapsed = 0f;
            AgentStop();
            return;
        }

        bool isMoving =
            agent &&
            !agent.isStopped &&
            !agent.pathPending &&
            agent.remainingDistance > agent.stoppingDistance &&
            agent.velocity.sqrMagnitude > 0.01f;

        FlowerShoesAnim = isMoving ? 1f : 0f;
        FlowerShoesMove(Runner.DeltaTime);
    }

    public override void Render()
    {
        if (!animator || !agent) return;

        animator.SetFloat(FlowerShoesAnimMove, FlowerShoesAnim);
    }

    void FlowerShoesMove(float dt)
    {
        switch (phase)
        {
            case Phase.Idle:
                // 첫 진입 시 대기부터 시작
                waitRemain = RandomWait();
                moveElapsed = 0f;
                AgentStop();
                phase = Phase.Waiting;
                break;

            case Phase.Waiting:
                if (waitRemain > 0f)
                {
                    waitRemain -= dt;
                    break;
                }
                // 목적지 선정
                if (!TargetDestination(transform.position, out dest))
                {
                    // 샘플 실패 시 짧게 재시도 대기
                    waitRemain = RandomWait();
                    break;
                }

                // 이동 시작
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.isStopped = false;
                agent.SetDestination(dest);

                moveElapsed = 0f;
                phase = Phase.Moving;
                break;

            case Phase.Moving:
                // 아직 경로 계산 중이면 대기
                if (agent.pathPending)
                    break;

                // 경로가 유효하지 않으면 다시 후보를 찾도록 대기 상태로 복귀
                if (!agent.hasPath ||
                    agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                    agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    AgentStop();
                    waitRemain = RandomWait(); // 바로 다시 굴리기보단 살짝 쉬고 재탐색
                    moveElapsed = 0f;
                    phase = Phase.Waiting;
                    break;
                }

                moveElapsed += dt;

                // 목적지까지 일정 시간 이상 걸리면 이 목적지는 포기하고 새 목적지 탐색
                if (moveElapsed >= moveTimeoutSeconds)
                {
                    AgentStop();
                    waitRemain = RandomWait();
                    moveElapsed = 0f;
                    phase = Phase.Waiting;
                    break;
                }

                // 정상 경로일 때 도착 판정
                if (Arrived(dest))
                {
                    AgentStop();
                    waitRemain = RandomWait();
                    moveElapsed = 0f;
                    phase = Phase.Waiting;
                }
                break;
        }
    }

    bool Arrived(Vector3 target)
    {
        if (agent.pathPending) return false;
        float stop = Mathf.Max(0.2f, agent.stoppingDistance);
        return Vector3.Distance(transform.position, target) <= stop;
    }

    /// <summary>
    /// 이동 / 애니메이션
    /// </summary>
    private void AgentStop()
    {
        if (!agent || !agent.enabled) return;

        agent.isStopped = true;
        if (agent.hasPath)
            agent.ResetPath();
        agent.velocity = Vector3.zero; // 잔여 속도 제거

        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    float RandomWait()
    {
        return (maxWait > minWait) ? Random.Range(minWait, maxWait) : minWait;
    }

    /// <summary>
    /// 현재 위치를 기준으로 moveRadius내에서 랜덤 목적지 찾는 함수
    /// </summary>
    private bool TargetDestination(Vector3 origin, out Vector3 result)
    {
        // NavMesh 상의 랜덤 점 + PathComplete 조건을 만족하는 목적지만 인정
        for (int i = 0; i < Mathf.Max(1, maxDestinationTries); i++)
        {
            Vector2 v = Random.insideUnitCircle * moveRadius;
            Vector3 cand = origin + new Vector3(v.x, 0f, v.y);

            // NavMesh 위의 포인트 샘플
            if (!NavMesh.SamplePosition(cand, out var hit, sampleMaxDistance, NavMesh.AllAreas))
                continue;

            // 현재 위치에서 해당 포인트까지의 경로가 완전한지 확인
            if (!HasCompletePath(hit.position))
                continue;

            result = hit.position;
            return true;
        }

        result = origin;
        return false; // 유효 지점 실패
    }

    /// <summary>
    /// 현재 에이전트 위치에서 destination까지 PathComplete인지 확인
    /// </summary>
    private bool HasCompletePath(Vector3 destination)
    {
        if (!agent || !agent.isOnNavMesh)
            return false;

        var path = new NavMeshPath();
        if (!agent.CalculatePath(destination, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }
}
