using Fusion;
using UnityEngine;

// 코드 담당자 : 김수아

public class Candlestick : NetworkBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private GameObject lightObj;
    [SerializeField] private Renderer lantern;
    [SerializeField] private int uvCN = 0;

    [Header("Ghost Detection")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask ghostLayerMask;
    private SphereCollider triggerCollider;
    private GhostSpawner.EGhost currentGhostType;

    [Header("VFX")]
    [SerializeField] private GameObject smokePrefab;

    // 데이터/상태
    private ItemData itemData;

    // 네트워크
    [Networked] private NetworkBool IsLightActive { get; set; }
    [Networked] private NetworkBool GhostInteractiveLight { get; set; }


    void Awake()
    {
        itemData = GetComponent<ItemObject>()?.itemData;

        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;
    }

    public override void Spawned()
    {
        // 처음 스폰하는 경우만 초기화
        if (Object.HasStateAuthority && Runner.IsServer)
        {
            if (itemData.wasUse)
            {
                IsLightActive = false;
                GhostInteractiveLight = true;
            }
            else // 처음 설치됨
                IsLightActive = true;
        }

        ApplyLightVisual();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        if (GhostInteractiveLight)
            return;

        var ghostSpawner = GhostSpawner.Instance;
        currentGhostType = ghostSpawner ? ghostSpawner.mapGhostType : GhostSpawner.EGhost.Jibakreong;

        if (!itemData.canDetect.Contains(currentGhostType))
            return;

        // 거리 내 귀신 감지
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, ghostLayerMask);
        if (hits.Length > 0)
        {
            GhostInteractiveLight = true;
            IsLightActive = false;

            itemData.wasUse = true;
        }
    }

    private void ApplyLightVisual()
    {
        if (lightObj) lightObj.SetActive(IsLightActive);

        // Material 발광 처리
        if (lantern && uvCN < lantern.materials.Length)
        {
            lantern.materials[uvCN].SetColor("_EmissionColor", IsLightActive ? Color.white : Color.black);
        }
    }

    public override void Render()
    {
        ApplyLightVisual();
    }
}


