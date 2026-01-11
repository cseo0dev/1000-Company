using System.Collections;
using Fusion;
using UnityEngine;

// 코드 담당자: 김수아

public class Bujeok : ExorcismItemBase
{
    [Header("부적 시각효과")]
    [SerializeField] private float burnDelay = 2f; // 부적 타는 연출 시간
    [SerializeField] private GameObject effectObj;

    [SerializeField] protected Renderer[] bujeokRenderers;
    private MaterialPropertyBlock mpb;
    static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
    static readonly int EdgeColorID = Shader.PropertyToID("_EdgeColor");

    protected override void Start()
    {
        mpb = new MaterialPropertyBlock();
        SetDissolve(0f);
    }

    protected override void OnUseSuccess()
    {
        Rpc_PlayBurnFX();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_PlayBurnFX()
    {
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        effectObj.SetActive(true);

        float elapsed = 0f;

        while (elapsed < burnDelay)
        {
            float t = Mathf.Clamp01(elapsed / burnDelay);
            SetDissolve(t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (Object && Object.IsValid)
            Runner.Despawn(Object);
        else
            Destroy(gameObject);
    }

    private void SetDissolve(float v)
    {
        mpb.Clear();
        mpb.SetFloat(DissolveID, v);
        mpb.SetColor(EdgeColorID, new Color(255f, 0f, 0f, 128f));

        for (int r = 0; r < bujeokRenderers.Length; r++)
        {
            var rend = bujeokRenderers[r];
            if (!rend) continue;

            // 서브메시 전체에 PropertyBlock 적용
            var mats = rend.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                rend.SetPropertyBlock(mpb, i);
        }
    }
}
