// 코드 담당자 : 최서영
using Fusion;
using UnityEngine;

public class FlowerShoesSound : NetworkBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 애니메이션에 맞게 사운드 이벤트 호출
    /// </summary>
    public void PlayFlowerShoesSfx()
    {
        if (Object && Object.HasStateAuthority)
            RpcPlaySfx();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlaySfx()
    {
        if (audioSource) audioSource.Play();
    }
}
