// 코드 담당자 : 최서영
using System.Collections;
using UnityEngine;

public class MudangBell : MonoBehaviour, IUsable
{
    private string targetTag = "GhostItem"; // 탐지할 아이템의 태그
    private float detectRadius = 10f; // 탐지 범위

    private float shakeValue = 12f; // 무당방울 흔들리는 정도
    private float shakeSpeed = 8f; // 흔들리는 속도
    private float shakeDuration = 1f; // 흔드는 시간
    private Vector3 baseEuler = Vector3.zero; // 씬에 배치한 프리팹 Rotation에 영향을 받길래 zero값 추가

    private AudioSource audioSource;
    [Range(0f, 1f)] private float maxVolume = 1f; // 탐지 대상이 가까울 때 최대 볼륨
    [Range(0f, 1f)] private float minVolume = 0f; // 탐지 대상이 멀 때 최소 볼륨
    private float minDistanceRatio = 0.01f; // 이 거리 내에서는 maxVoluem 유지
    private float nearBoostExponent = 6f; // 근거리 부스트 지수

    private InventoryManager _inventoryManager;

    // 내부 상태
    private Quaternion _baseLocalRot;
    private bool _isShaking;

    void Awake()
    {
        _inventoryManager = GetComponentInParent<InventoryManager>();

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 3D Sound Setting 기본값 설정 (오디오소스 컴포넌트 내 옵션입니다!)
        if (audioSource)
        {
            audioSource.spatialBlend = 1f; // 3D Sound 사용하겠다 (true)
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // 3D Sound 모드 중 Logarithmic 선택
            audioSource.minDistance = detectRadius * minDistanceRatio; // 가까울수록 크게
            audioSource.maxDistance = detectRadius; // 이 거리쯤에서는 음이 거의 사라짐
            audioSource.loop = false; // 루프 X
            audioSource.dopplerLevel = 0f;
        }

        _baseLocalRot = transform.localRotation;
    }

    public void Use()
    {
        PlayBell();
    }

    public void PlayBell()
    {
        if (_isShaking) return; // 이미 흔들리고 있으면 또 흔들지 않음

        StartCoroutine(bellShake());

        float vol = DistanceCalcul();
        if (audioSource && audioSource.clip)
            audioSource.PlayOneShot(audioSource.clip, vol);
    }

    /// <summary>
    /// GhostItem 위치 기반으로 음량 크기 조절
    /// </summary>    
    float DistanceCalcul()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);
        float nearest = float.MaxValue;

        foreach (var h in hits)
        {
            if (h && h.gameObject.activeInHierarchy && h.CompareTag(targetTag))
            {
                float d = Vector3.Distance(transform.position, h.ClosestPoint(transform.position));
                if (d < nearest)
                    nearest = d;
            }
        }

        if (nearest == float.MaxValue)
            return minVolume; // 감지 대상이 없으면 최소 볼륨

        // 거리기반 사운드 볼륨 조절
        float t = Mathf.InverseLerp(detectRadius, 0f, nearest); // 0 = 멀다, 1 = 가깝다

        // 근거리 강조: t^γ
        float boosted = Mathf.Pow(t, nearBoostExponent);

        // 최종 볼륨
        return Mathf.Lerp(minVolume, maxVolume, boosted);
    }

    /// <summary>
    /// 흔들리는 애니메이션용 코루틴
    /// </summary>
    IEnumerator bellShake()
    {
        _isShaking = true;
        float elapsed = 0f;
        _baseLocalRot = transform.localRotation * Quaternion.Euler(baseEuler);

        // 흔들리는 애니메이션
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float life = 1f - Mathf.Clamp01(elapsed / shakeDuration);
            float angle = Mathf.Sin(elapsed * shakeSpeed * Mathf.PI * 2f) * shakeValue * life;
            transform.localRotation = _baseLocalRot * Quaternion.Euler(0f, 0f, -angle);
            yield return null;
        }

        // 원 위치 복귀
        transform.localRotation = _baseLocalRot;
        _isShaking = false;
    }

    // 탐지 반경 확인용
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
