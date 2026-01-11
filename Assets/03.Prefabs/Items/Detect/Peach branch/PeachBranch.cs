// 코드 담당자 : 최서영
using Fusion;
using UnityEngine;

public class PeachBranch : NetworkBehaviour
{
    private ItemData itemData;
    private GhostSpawner ghostSpawner;

    [SerializeField] private Renderer[] peachBranch;
    private MaterialPropertyBlock mpb;

    private float dissolveDelay = 60f; // 대기 시간
    private float dissolveDuration = 30f; // 천천히 사라지게 큰 값
    static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
    static readonly int EdgeColorID = Shader.PropertyToID("_EdgeColor");

    [Networked] public bool IsDissolving { get; set; } // 귀신 감지용 (감지되면 dissolve 기능 true로)
    [Networked] public int DissolveStartTick { get; set; } // 네트워크 공유용

    void Awake()
    {
        itemData = GetComponent<ItemObject>()?.itemData;
        
        if (peachBranch.Length == 0)
        {
            Debug.Log("[PeachBranch] 복숭아 꽃 프리팹 연결 필요");
        }

        mpb = new MaterialPropertyBlock();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.Instance;

        if (ghostSpawner == null || itemData == null || itemData.canDetect == null)
            return;

        bool canDetectThisGhost = itemData.canDetect.Contains(ghostSpawner.mapGhostType); // 귀신 탐지

        // 테스트용
        //var ghostName = ghostSpawner.mapGhostType.ToString();
        //Debug.Log($"[Peach Branch] {ghostName} 탐지 완료 / 탐지 대상 : {canDetectThisGhost}");

        if (canDetectThisGhost && !IsDissolving)
        {
            IsDissolving = true;
            DissolveStartTick = Runner.Tick;
        }
    }

    public override void Render()
    {
        if (!IsDissolving)
            return;

        int tickElapsed = Runner.Tick - DissolveStartTick;
        float elapsedSeconds = tickElapsed * Runner.DeltaTime;

        // Delay 적용 후 0~1로 클램프
        float t = (elapsedSeconds - dissolveDelay) / Mathf.Max(0.0001f, dissolveDuration);
        t = Mathf.Clamp01(t);

        SetDissolve(t);
    }

    // Dissolve(=셰이더) 값 변경 -> 점점 사라지는  효과
    public void SetDissolve(float v)
    {
        mpb.Clear();
        mpb.SetFloat(DissolveID, v);
        mpb.SetColor(EdgeColorID, new Color(255f, 0f, 0f, 128f));

        for (int r = 0; r < peachBranch.Length; r++)
        {
            var rend = peachBranch[r];
            if (!rend) continue;

            var mats = rend.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                rend.SetPropertyBlock(mpb, i);
            }
        }
    }
}
