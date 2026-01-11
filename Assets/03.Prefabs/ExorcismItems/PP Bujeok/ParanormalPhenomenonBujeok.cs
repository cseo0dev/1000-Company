using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Audio;

// 작성자 : 정하윤
// 이상현상용 부적 스크립트
public class ParanormalPhenomenonBujeok : NetworkBehaviour, IUsable
{
    [Header("부적 시각효과")]
    [SerializeField] private float burnDelay = 2f;      // 부적 타는 연출 시간
    [SerializeField] private GameObject effectObj;
    [SerializeField] protected Material[] bujeokMats;
    [SerializeField] protected ItemData itemData;
    [SerializeField] protected GameObject magicCirclePrefab;

    [Header("부적 사운드")]
    [SerializeField] private AudioSource burnAudioSource;
    [SerializeField] private AudioClip burnSoundClip;

    // 부적으로부터 얼마나 앞에 생성할지 거리
    [SerializeField] private float spawnDistance = 0.5f;
    protected Vector3 startPosition;

    private bool isUsed = false;

    private NetworkObject magicCircleObj;
    [SerializeField] private AudioSource audioSource;

    private NetworkBool PlayEffect { get; set; } = false;

    public override void Spawned()
    {
        // Material 설정
        if (bujeokMats == null || bujeokMats.Length == 0)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            bujeokMats = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                bujeokMats[i] = renderers[i].material;
                bujeokMats[i].SetFloat("_Dissolve", 0f);
            }
        }

        Use();
    }

    public void Use()
    {
        if (isUsed) 
            return;

        // 마법진 생성 및 효과 재생
        if (Object.HasStateAuthority && magicCirclePrefab != null)
        {
            // 플레이어가 바라보고 있는 앞 방향에 마법진이 생기도록 플레이어를 받아옴
            PlayerRef playerRef = Object.InputAuthority;

            if (playerRef == PlayerRef.None)
                return;

            NetworkObject playerObj = Runner.GetPlayerObject(playerRef);

            if (playerObj == null)
                return;

            Transform playerTransform = playerObj.transform;

            // 플레이어의 수평(XZ) 정면 방향을 구함
            Vector3 spawnDir = playerTransform.forward;
            spawnDir.y = 0; 

            // 플레이어가 위/아래를 볼 경우 Y축 회전값을 기반으로 수평 방향을 다시 계산
            if (spawnDir.sqrMagnitude < 0.0001f)
                spawnDir = Quaternion.Euler(0f, playerTransform.rotation.eulerAngles.y, 0f) * Vector3.forward;
            else
                // 유효한 방향이 있으면 정규화
                spawnDir.Normalize(); 

            // 마법진 생성의 기준 위치를 부적의 위치로 변경
            Vector3 spawnBasePos = transform.position;

            // 부적의 위치에서 + 플레이어가 바라보는 방향으로 spawnDistance만큼 떨어진 곳
            Vector3 spawnPos = spawnBasePos + (spawnDir * spawnDistance);

            // 마법진의 회전은 플레이어의 수평 회전을 따름
            Quaternion verticalRotation = Quaternion.Euler(0f, playerTransform.rotation.eulerAngles.y, 0f);

            magicCircleObj = Runner.Spawn
            (
                magicCirclePrefab,
                spawnPos,
                verticalRotation,
                Object.InputAuthority
            );

            var effectController = magicCircleObj.GetComponent<PPBujeokEffectController>();

            // Action 직접 전달
            if (effectController != null)
                effectController.TriggerEffect(OnEffectComplete);
        }
    }

    // 콜백 : 파티클 재생 끝나면 실행
    private void OnEffectComplete()
    {
        if (Object.HasStateAuthority)
        {
            ApplyTalismanToTargets();
            RPC_StartBurn();
        }
    }

    private void ApplyTalismanToTargets()
    {
        var colliders = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Ghost"));

        foreach (var col in colliders)
        {
            var target = col.GetComponent<IExorcisableByTalisman>();

            if (target != null)
            {
                target.ApplyTalisman();
            }
        }
    }

    /// <summary>
    /// 모든 클라이언트가 BurnRoutine을 실행하도록 RPC 함수 추가
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartBurn()
    {
        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        // 타는 사운드 재생
        if (burnAudioSource != null && burnSoundClip != null)
        {
            burnAudioSource.clip = burnSoundClip;
            burnAudioSource.loop = true;
            burnAudioSource.Play();
        }

        effectObj.SetActive(true);

        float elapsed = 0f;

        while (elapsed < burnDelay)
        {
            float t = Mathf.Clamp01(elapsed / burnDelay);

            // t가 0에서 1로 증가하며 Dissolve 효과 적용
            foreach (var mat in bujeokMats)
                mat.SetFloat("_Dissolve", t); 

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (burnAudioSource != null)
        {
            burnAudioSource.Stop();
        }

        // 호스트만 Despawn 호출
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
