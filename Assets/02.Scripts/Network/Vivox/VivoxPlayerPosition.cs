using System.Collections;
using Unity.Services.Vivox;
using UnityEngine;
using Fusion;

// 코드 담당자: 김수아
/// <summary>
/// 플레이어 프리팹에 넣어서 사용
/// 플레이어의 위치를 3D 채널에 업데이트 → 거리에 따른 감쇄
/// </summary>
public class VivoxPlayerPosition : NetworkBehaviour
{
    [SerializeField] Transform cam; //플레이어 카메라
    [SerializeField] private WaitForSeconds interval = new WaitForSeconds(0.1f);

    public override void Spawned()
    {
        if ((Object.HasInputAuthority))
        {
            StartCoroutine(WaitStartVoicePos());
        }
    }

    private IEnumerator WaitStartVoicePos()
    {
        // Vivox 초기화/로그인/채널 Join이 끝날 때까지 대기
        while (!VivoxManager.Instance.MainChannelConnected)
            yield return null;

        // 채널 연결 확실히 끝 -> 음성 위치 업데이트 시작
        StartCoroutine(UpdateVoicePos());
    }

    IEnumerator UpdateVoicePos()
    {
        while (true)
        {
            // MainChannel에 있을 때만 3D 위치 업데이트
            if (VivoxManager.Instance.MainChannelConnected)
            {
                VivoxService.Instance.Set3DPosition(transform.position, cam.position, cam.forward, cam.up, VivoxManager.Instance.mainChannel);
            }

            yield return interval;
        }
    }
}
