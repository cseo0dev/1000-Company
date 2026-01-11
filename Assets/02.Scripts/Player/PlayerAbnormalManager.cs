using UnityEngine;
using Fusion;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 비정상 상태(빙의, 환각, 자해)를 트리거하고 AI 로직을 실행합니다.
/// 이 스크립트는 서버(StateAuthority)에서만 핵심 로직을 실행합니다.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(PlayerCondition), typeof(PlayerInteraction))]
public class PlayerAbnormalManager : NetworkBehaviour
{
    // 1. 상태 정의
    public enum AbnormalStateType
    {
        None,
        Possession,  // 빙의
        Hallucination, // 환각
        SelfHarm     // 자해
    }

    [Networked]
    public AbnormalStateType CurrentAbnormalState { get; private set; } = AbnormalStateType.None;

    [Networked]
    private TickTimer _abnormalStateTimer { get; set; }

    [Networked]
    public TickTimer AbnormalCooldown { get; private set; }

    // 2. 컴포넌트 참조
    private PlayerController _controller;
    private PlayerCondition _condition;
    private PlayerInteraction _interaction;
    private Animator _animator;
    private NetworkCharacterController _ncc;

    // 3. AI 설정값
    [Header("AI Settings")]
    [SerializeField] private float aiRunSpeed = 4.0f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float selfHarmDamageInterval = 1.5f;
    [SerializeField] private float stateDuration = 10.0f;

    [Networked] private TickTimer _aiActionTimer { get; set; }

    public override void Spawned()
    {
        _controller = GetComponent<PlayerController>();
        _condition = GetComponent<PlayerCondition>();
        _interaction = GetComponent<PlayerInteraction>();
        _animator = GetComponent<Animator>();
        _ncc = GetComponent<NetworkCharacterController>();

        if (Runner.IsServer)
        {
            AbnormalCooldown = TickTimer.CreateFromSeconds(Runner, 0);
            CurrentAbnormalState = AbnormalStateType.None;
        }
    }

    public bool IsControlling()
    {
        return CurrentAbnormalState == AbnormalStateType.Possession ||
               CurrentAbnormalState == AbnormalStateType.SelfHarm;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        CheckAndTriggerAbnormalState();

        if (CurrentAbnormalState != AbnormalStateType.None)
        {
            if (_abnormalStateTimer.Expired(Runner))
            {
                Rpc_StopAbnormalState();
            }
            else
            {
                switch (CurrentAbnormalState)
                {
                    case AbnormalStateType.Possession:
                        RunPossessionAI();
                        break;
                    case AbnormalStateType.SelfHarm:
                        RunSelfHarmAI();
                        break;
                }
            }
        }
    }

    /// <summary>
    /// [서버 전용] 정신력을 체크하여 상태이상을 발동시킵니다.
    /// </summary>
    private void CheckAndTriggerAbnormalState()
    {
        float sanityThreshold = _condition.MaxSanity * 0.15f;

        if (CurrentAbnormalState == AbnormalStateType.None &&
            AbnormalCooldown.Expired(Runner) &&
            _condition.CurrentSanity <= sanityThreshold)
        {
            // Unity Random 사용 (서버에서만 실행되므로 동기화 문제 없음)
            if (Random.value < 0.20f)
            {
                int stateTypeIndex = Random.Range(1, 4); // 1, 2, 3 중 하나
                AbnormalStateType chosenState = (AbnormalStateType)stateTypeIndex;

                Rpc_StartAbnormalState(chosenState, stateDuration);
            }
            else
            {
                AbnormalCooldown = TickTimer.CreateFromSeconds(Runner, 1.0f);
            }
        }
    }

    // --- C. AI 실행 로직 (서버 전용) ---

    private void RunPossessionAI()
    {
        PlayerController nearestAlly = FindNearestAlly();
        if (nearestAlly == null)
        {
            _ncc.Move(Vector3.zero);
            return;
        }

        Vector3 directionToAlly = (nearestAlly.transform.position - transform.position);
        directionToAlly.y = 0;
        float distance = directionToAlly.magnitude;

        if (distance > attackDistance)
        {
            _ncc.maxSpeed = aiRunSpeed;
            _ncc.Move(directionToAlly.normalized);
        }
        else
        {
            _ncc.Move(Vector3.zero);

            if (_aiActionTimer.ExpiredOrNotRunning(Runner))
            {
                Rpc_TriggerAttackAnim();

                if (nearestAlly.TryGetComponent<PlayerCondition>(out var allyCondition))
                {
                    allyCondition.Rpc_TakeDamage(1);
                }

                _aiActionTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
            }
        }
    }

    private void RunSelfHarmAI()
    {
        if (_interaction.HasValidHit)
        {
            float distanceToWall = _interaction.HitInfo.distance;
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0;

            if (distanceToWall > 1.0f)
            {
                _ncc.maxSpeed = aiRunSpeed;
                _ncc.Move(forwardDirection);
            }
            else
            {
                _ncc.Move(Vector3.zero);

                if (_aiActionTimer.ExpiredOrNotRunning(Runner))
                {
                    Rpc_TriggerSelfHarmAnim();
                    _condition.Rpc_TakeDamage(1);
                    _aiActionTimer = TickTimer.CreateFromSeconds(Runner, selfHarmDamageInterval);
                }
            }
        }
        else
        {
            _ncc.maxSpeed = aiRunSpeed;
            _ncc.Move(transform.forward);
        }
    }

    private PlayerController FindNearestAlly()
    {
        var allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController nearest = null;
        float minDistance = float.MaxValue;

        foreach (var player in allPlayers)
        {
            if (player.Object.Id == this.Object.Id) continue;
            if (player.TryGetComponent<PlayerCondition>(out var cond) && cond.IsDead) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player;
            }
        }
        return nearest;
    }

    // --- D. RPCs (상태 동기화) ---

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_StartAbnormalState(AbnormalStateType stateType, float duration)
    {
        if (CurrentAbnormalState != AbnormalStateType.None) return;

        CurrentAbnormalState = stateType;
        _abnormalStateTimer = TickTimer.CreateFromSeconds(Runner, duration);
        AbnormalCooldown = TickTimer.CreateFromSeconds(Runner, duration + 5.0f);

        if (stateType == AbnormalStateType.Possession || stateType == AbnormalStateType.SelfHarm)
        {
            _aiActionTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }

        Rpc_TriggerAbnormalAnim(stateType, true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_StopAbnormalState()
    {
        if (CurrentAbnormalState == AbnormalStateType.None) return;

        Rpc_TriggerAbnormalAnim(CurrentAbnormalState, false);
        CurrentAbnormalState = AbnormalStateType.None;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_TriggerAbnormalAnim(AbnormalStateType stateType, bool start)
    {
        if (_animator == null) return;

        string animBool = "";
        switch (stateType)
        {
            case AbnormalStateType.Possession:
                animBool = "IsPossessed";
                break;
            case AbnormalStateType.SelfHarm:
                animBool = "IsSelfHarming";
                break;
            case AbnormalStateType.Hallucination:
                animBool = "IsHallucinating";
                break;
            default:
                return;
        }

        if (start)
        {
            _animator.SetBool(animBool, true);
        }
        else
        {
            _animator.SetBool("IsPossessed", false);
            _animator.SetBool("IsSelfHarming", false);
            _animator.SetBool("IsHallucinating", false);
        }
    }

    // --- E. 개별 액션 RPCs (애니메이션 동기화) ---

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_TriggerAttackAnim()
    {
        _animator.SetTrigger("Attack");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_TriggerSelfHarmAnim()
    {
        _animator.SetTrigger("Headbang");
    }

    // --- F. Render (환각 효과) ---

    public override void Render()
    {
        if (Object.HasInputAuthority)
        {
            if (CurrentAbnormalState == AbnormalStateType.Hallucination)
            {
                // TODO: 여기에 환각 UI/화면 왜곡 효과 코드를 넣습니다.
                // 예: UIManager.Instance.SetHallucinationEffect(true);
            }
            else
            {
                // TODO: 환각 효과를 끕니다.
                // 예: UIManager.Instance.SetHallucinationEffect(false);
            }
        }
    }
}