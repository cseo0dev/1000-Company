using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

// 코드 담당자: 김수아

public class GhostSpawner : NetworkBehaviour
{
    public static GhostSpawner Instance { get; private set; }

    public enum EGhost { Jibakreong, Dokkaebi, Changgui, Agui }
    public enum EExorcismState { None, InProgress, Success, Failed }

    [Networked] public EExorcismState ExorcismState { get; set; } = EExorcismState.None;
    [Networked] public NetworkBool IsRetry { get; private set; } // 제령 재시도 플래그
    [Networked] public EGhost mapGhostType { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _minSpawnDelay = 10f;
    [SerializeField] private float _maxSpawnDelay = 30f;
    // 정신력 30 이하 이벤트용
    [SerializeField] private float _lowSanMinDelay = 40f;
    [SerializeField] private float _lowSanMaxDelay = 60f;
    private List<PlayerCondition> _lowSanityPlayers = new List<PlayerCondition>(); // 정신력 30이하 플레이어

    [Header("Ghost Database")]
    public MapGhostDatabase _mapGhostDatabase;
    public GhostRelocationSpawner relocateSpawner;

    private NetworkObject _currentGhost;
    private GhostController _ghostController;
    public GhostController GhostController { get; private set; }
    private bool _spawnLoopRunning = false;
    private Coroutine _spawnRoutine;


    #region  유니티 이벤트

    void OnEnable()
    {
        PlayerCondition.OnLowSanityStatus += OnPlayerSanStatusChanged;
    }

    void OnDisable()
    {
        PlayerCondition.OnLowSanityStatus -= OnPlayerSanStatusChanged;
    }

    public override void Spawned()
    {
        Instance = this; // 싱글턴
        if (!Object.HasStateAuthority) return;

        /* 현재 활성 씬 이름으로 mapGhost 소환
        var sceneName = $"Sua_Network"; // 수정 필요
        var mg = _mapGhostDatabase.GetGhostForScene(sceneName);
        if (mg != null && mg.ghostData != null)
        {
            mapGhostType = mg.ghostData.ghostType;
            Debug.Log($"[GhostSpawner] Scene '{sceneName}' → mapGhostType = {mapGhostType}");
        }
        else
        {
            // 없으면 기본값 혹은 에러
            Debug.LogError($"[GhostSpawner] 씬 '{sceneName}'에 매핑된 Ghost가 없습니다. MapGhostDatabase 설정을 확인하세요.");
        } */
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;

        StopSpawnLoop();
    }

    public override void FixedUpdateNetwork()
    {
        // Host만 관리
        if (!Object.HasStateAuthority) return;
        if (ExorcismState == EExorcismState.Success)
        {
            StopSpawnLoop();
            return;
        }

        var missionManager = NetworkMissionManager.Instance;
        if (missionManager == null || missionManager.Object == null || !missionManager.Object.IsValid) return;

        // 미션 시작(플레이어 집 진입)후부터 스폰 루프 실행
        if (NetworkMissionManager.Instance.IsMissionStarted && ExorcismState == EExorcismState.None && !_spawnLoopRunning)
        {
            Debug.Log("[GhostSpawner] Start Spawn Loop");
            StartSpawnLoop();
        }
    }
    #endregion

    // // BasicSpawner에서 맵 귀신 타입 받아감
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // public void RPC_SetMapGhostType(EGhost type)
    // {
    //     mapGhostType = type;
    // }

    private void StartSpawnLoop()
    {
        if (_spawnLoopRunning) return;

        _spawnLoopRunning = true;
        _spawnRoutine = StartCoroutine(SpawnManagerRoutine());
    }

    private void StopSpawnLoop()
    {
        if (!_spawnLoopRunning) return;
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        _spawnLoopRunning = false;
    }

    // ========저정신력 플레이어 쫓기========
    // 정신력 30이하를 타겟으로 업데이트
    private void OnPlayerSanStatusChanged(PlayerCondition player, bool isLowSanity)
    {
        if (Object == null) return;
        if (!Object.HasStateAuthority) return;
        if (player == null) return;

        // Low Sanity 플레이어가 진입하면 타겟으로 설정
        if (isLowSanity)
        {
            if (!_lowSanityPlayers.Contains(player))
            {
                _lowSanityPlayers.Add(player);
                Debug.Log($"Low Sanity 플레이어 목록 추가: {player.name}. 현재 {_lowSanityPlayers.Count}명.");
            }
        }
        // Low Sanity 플레이어가 해제되면 타겟 해제
        else
        {
            if (_lowSanityPlayers.Contains(player))
            {
                _lowSanityPlayers.Remove(player);
                Debug.Log($"Low Sanity 플레이어 목록 제거: {player.name}. 현재 {_lowSanityPlayers.Count}명.");
            }
        }
    }

    // ========스폰 루프========
    //일반 스폰(10~30초)과 정신력 낮을 때 발생하는 스폰(40~60초)를 관리하는 루틴
    private IEnumerator SpawnManagerRoutine()
    {
        if (ExorcismState == EExorcismState.Success)
        {
            Debug.Log("제령 성공으로 스폰 루틴 중단");
            StopSpawnLoop();
            yield break;
        }

        while (true)
        {
            // 제령 중이면 대기
            while (ExorcismState == EExorcismState.InProgress)
                yield return null;

            // 귀신이 있으면 파괴될 때까지 대기
            while (_ghostController != null || (_currentGhost != null && _currentGhost.IsValid))
                yield return null;

            //  씬 전환, 파괴 대비
            if (_spawnPoints == null || _spawnPoints.Length == 0 || _mapGhostDatabase == null)
            {
                StopSpawnLoop();
                yield break;
            }

            // 특수, 일반 스폰 분기            
            GhostController.EGhostState nextState;
            Vector3 spawnPosition = Vector3.zero; // 일반 스폰용
            Transform target = null; // 특수 스폰용
            PlayerCondition targetPC = null; // 특수 스폰용

            bool lowSanPlayerExit = _lowSanityPlayers.Count > 0;
            bool useSpecialSpawn = lowSanPlayerExit && Random.value < 0.5f;

            float delay;
            bool isCeiling = false; // 천장 여부 확인
            string debugMessage;

            // 2. 정신력 30 이하 플레이어 여부에 따른 스폰
            if (useSpecialSpawn)
            {
                // 특수 스폰 (40~60초)
                delay = Random.Range(_lowSanMinDelay, _lowSanMaxDelay);
                nextState = GhostController.EGhostState.Chase;

                // Low San 플레이어 중 한 명 랜덤
                int ranIndex = Random.Range(0, _lowSanityPlayers.Count);
                targetPC = _lowSanityPlayers[ranIndex];
                target = targetPC.transform;
                debugMessage = $"[LowSanity Spawn] {target.name} 근처에 {delay:F1}초 후 스폰 예정";
            }
            else
            {
                // 일반 스폰 (10~30초)
                delay = Random.Range(_minSpawnDelay, _maxSpawnDelay);

                // 스폰 위치 및 상태 결정
                spawnPosition = GetRandomSpawnPoint(out isCeiling);
                nextState = isCeiling
                    ? GhostController.EGhostState.Patrol
                    : (Random.value < 0.5f ? GhostController.EGhostState.Idle : GhostController.EGhostState.Patrol);

                if (lowSanPlayerExit)
                    debugMessage = $"정신력 저하 플레이어가 있지만, 일반 스폰 ({delay:F1}초 후) 실행";
                else
                    debugMessage = $"랜덤 위치에 {delay:F1}초 후 스폰 예정";
            }

            if (ExorcismState == EExorcismState.Success)
            {
                StopSpawnLoop();
                yield break;
            }
            Debug.Log(debugMessage);
            yield return new WaitForSeconds(delay);

            if (_currentGhost != null) continue; // 대기 중 다른 곳에서 귀신이 생성된 경우 무시

            if (useSpecialSpawn)
            {
                // 특수 스폰 위치 계산 (대기 시간 후에도 타겟 플레이어의 상태가 유효한지 확인)
                if (targetPC != null && targetPC.CurrentSanity <= 30f)
                {
                    Vector3 randomSpherePos = target.position + Random.insideUnitSphere * 8f;
                    randomSpherePos.y = target.position.y;

                    if (NavMesh.SamplePosition(randomSpherePos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                        spawnPosition = hit.position;
                    else
                    {
                        if (targetPC != null) _lowSanityPlayers.Remove(targetPC);
                        continue;
                    }
                }
                else
                {
                    // 대기 시간 동안 회복되거나 죽은 경우
                    if (targetPC != null) _lowSanityPlayers.Remove(targetPC);
                    continue;
                }
            }

            GhostData data = _mapGhostDatabase.GetGhost(mapGhostType);
            if (data != null)
                SpawnGhost(mapGhostType, data, spawnPosition, nextState, target, isCeiling);
        }
    }

    // 프리팹 결정 로직
    private bool GetPrefab(GhostData data, out NetworkPrefabRef prefab, bool? forceReal = false)
    {
        prefab = default;
        if (data == null) return false;

        if (forceReal.HasValue)
        {
            prefab = forceReal.Value ? data.realGhostPrefab : data.blackGhostPrefab;
        }
        else
        {
            prefab = ExorcismState switch
            {
                EExorcismState.None => data.blackGhostPrefab,
                EExorcismState.InProgress => data.blackGhostPrefab,
                EExorcismState.Success => data.realGhostPrefab,
                EExorcismState.Failed => data.realGhostPrefab, // 재시도 가능
                _ => data.blackGhostPrefab // 그 외의 경우
            };
        }

        if (prefab.Equals(default(NetworkPrefabRef)))
            return false;

        return true;
    }

    /// <summary>
    /// 귀신 생성 로직
    /// </summary>
    public void SpawnGhost(EGhost ghostType, GhostData data, Vector3 position, GhostController.EGhostState initState, Transform target, bool isCeiling = false, bool? forceReal = null)
    {
        if (!Object.HasStateAuthority || data == null) return;

        // 기존 귀신이 있으면 제거
        if (_currentGhost != null && _currentGhost.IsValid)
        {
            Runner.Despawn(_currentGhost);
            _currentGhost = null;
            _ghostController = null;
            GhostController = _ghostController;
        }

        if (!GetPrefab(data, out var prefabRef, forceReal)) return;

        _currentGhost = Runner.Spawn(prefabRef, position, Quaternion.identity, null, (runner, obj) =>
        {
            var gc = obj.GetComponent<GhostController>();
            bool isBlack = prefabRef.Equals(data.blackGhostPrefab);

            gc.Init(ghostType, data, isBlack);

            // Spawned() 이후에 상태 바꾸기 위함
            gc.PrevStartState = initState;
            gc.PrevTarget = target;

            gc.TargetPlayer = target;
            gc.IsCeilingSpawn = isCeiling;

            _ghostController = gc;
            GhostController = gc;
        });
    }

    // 제령 결과 설정
    public void SetExorcismResult(bool success)
    {
        if (!Object.HasStateAuthority) return;

        ExorcismState = success ? EExorcismState.Success : EExorcismState.Failed;
        IsRetry = !success;

        if (success)
        {
            StopSpawnLoop();
        }

        Debug.Log($"EExorcismState 상태 변경 : {ExorcismState}, IsRetry={IsRetry}");
    }

    /// <summary>
    /// 랜덤 스폰 포인트 반환
    /// </summary>
    public Vector3 GetRandomSpawnPoint(out bool isCeiling)
    {
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        isCeiling = spawnPoint.CompareTag("Ceiling");
        return spawnPoint.position;
    }

    public void DespawnCurrentGhost()
    {
        if (_currentGhost != null && _currentGhost.IsValid)
            Runner.Despawn(_currentGhost);

        _currentGhost = null;
        _ghostController = null;
    }
}
