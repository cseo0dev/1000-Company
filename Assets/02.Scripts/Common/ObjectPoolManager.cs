// 코드 담당자 : 최서영
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// 단일 매니저에서 여러 프리팹의 오브젝트 풀을 관리하는 스크립트
/// 
/// 규칙:
/// - 등록(Register) : 맵에 생성될 이상현상만 등록
/// - 꺼낼 때(Get):  프리팹 키로만 요청한다. (반환값은 "비활성" 상태 / 스포너가 초기화 후 활성화)
/// - 반납(Return):  인스턴스만 넘긴다. (프리팹 키 불필요, 내부 매핑으로 복귀)
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    // 프리팹 대기열
    readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

    // 프리팹 역매핑 (반납시 어느 큐에 넣을지)
    readonly Dictionary<GameObject, GameObject> _instanceKey = new();

    /// <summary>
    /// 맵에 생성할 prefab과 수량을 등록하는 함수
    /// 등록 시점에 size 만큼 즉시 생성하여 비활성 상태로 큐에 적재
    /// </summary>
    public void Register(GameObject prefab, int size)
    {
        if (prefab == null)
        {
            Debug.Log("Register 실패 : 프리팹이 없습니다.");
            return;
        }

        if (_pools.ContainsKey(prefab))
        {
            Debug.Log("Register 실패 : 이미 등록한 프리팹입니다.");
            return;
        }

        var q = new Queue<GameObject>(size);
        _pools.Add(prefab, q );

        for (int i = 0; i < size; i++)
        {
            var inst = Instantiate(prefab, transform); // 매니저 하위에 생성 (불편하시면 각자 프리팹 이름으로 생성되게 바꾸겠습니다!)
            inst.SetActive(false);

            q.Enqueue(inst);
            _instanceKey[inst] = prefab; // 인스턴스(inst)와 원본 프리팹(prefab)을 1:1로 역매핑 (반납 시 소속 풀을 찾기 위함)
        }
    }

    /// <summary>
    /// 지정된 프리팹의 비활성 인스턴스를 꺼내는 함수
    /// 반환값은 항상 비활성 상태다(초기화/활성은 호출자가 수행)
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.Log("Get 실패 : 프리팹이 없습니다.");
            return null;
        }

        if (!_pools.TryGetValue(prefab, out var q))
        {
            Debug.Log($"Get 실패 : Register 먼저 호출하세요. ({prefab.name})");
            return null;
        }

        // 정해진 수량을 넘어서는 사용 막기
        if (q.Count == 0)
        {
            Debug.LogWarning($"풀 소진: {prefab.name} 재고 0개");
            return null;
        }

        var inst = q.Dequeue();
        if (inst.activeSelf) inst.SetActive(false);
        return inst;
    }

    /// <summary>
    /// 사용이 끝난 인스턴스를 풀로 반환
    /// </summary>
    public void Return(GameObject instance)
    {
        if (instance == null) return;

        if (!_instanceKey.TryGetValue(instance, out var prefab))
        {
            Debug.Log("Return 실패 : 풀에서 관리하는 인스턴스가 아님");
            return;
        }

        // 상태 초기화
        instance.SetActive(false);
        _pools[prefab].Enqueue(instance);
    }
}

/// <summary>
/// Fusion 2용 프로바이더
/// 기본 프로바이더를 상속하고 풀의 Get/Return 메서드 연결
/// </summary>
public sealed class FusionPoolProvider : NetworkObjectProviderDefault
{
    private readonly ObjectPoolManager _pool;

    public FusionPoolProvider(ObjectPoolManager pool)
    {
        this._pool = pool;
    }

    protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
    {
        if (!prefab) return null;

        var pooled = _pool.Get(prefab.gameObject);

        if (pooled)
        {
            // 위치/회전/활성화는 Runner.Spawn 경로에서 처리할것
            return pooled.GetComponent<NetworkObject>();
        }

        // 가드용
        return base.InstantiatePrefab(runner, prefab);
    }

    protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
    {
        if (instance)
            _pool.Return(instance.gameObject);

        // 가드용
        base.DestroyPrefabInstance(runner, prefabId, instance);
    }
}