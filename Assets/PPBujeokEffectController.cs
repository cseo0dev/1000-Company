using System;
using System.Collections;
using Fusion;
using UnityEngine;

// 작성자 : 정하윤
public class PPBujeokEffectController : NetworkBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip preEffectSoundClip;
    [SerializeField] private AudioClip effectSoundClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float mainSoundDelay = 3f;

    [SerializeField] private GameObject effectObj;
    private ParticleSystem particle;

    [Networked]
    [OnChangedRender(nameof(OnEffectStateChanged))]
    private NetworkBool PlayEffect { get; set; } = false;

    private Action _onCompleteCallback;

    // 이펙트가 중복 재생되는 것을 막기 위한 로컬 변수
    private bool _effectPlayed = false;

    public override void Spawned()
    {
        if (particle == null)
            particle = GetComponentInChildren<ParticleSystem>(true);

        // Spawned 시점에 PlayEffect가 이미 true인지 확인
        if (PlayEffect)
        {
            // 이미 true라면 OnChangedRender가 호출되지 않으므로 수동으로 이펙트를 재생
            PlayEffectVisuals();
        }
    }

    public void TriggerEffect(Action onComplete)
    {
        if (!Object.HasStateAuthority)
            return;

        _onCompleteCallback = onComplete;
        PlayEffect = true;

        StartCoroutine(CallCallbackAfterDuration(onComplete));
    }

    protected void OnEffectStateChanged()
    {
        // PlayEffect가 true로 바뀌는 순간 모든 클라이언트가 각자 이펙트를 재생
        if (PlayEffect)
        {
            PlayEffectVisuals();
        }
    }

    /// <summary>
    /// 실제 파티클을 켜는 시각 로직
    /// </summary>
    private void PlayEffectVisuals()
    {
        if (_effectPlayed) 
            return;

        _effectPlayed = true;
        effectObj.SetActive(true);

        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play();
        }

        if (audioSource != null && effectSoundClip != null)
        {
            StartCoroutine(PlaySequentialSounds());
        }
        else if (audioSource == null)
        {
            Debug.Log("PPBujeokEffectController: AudioSource가 할당되지 않음");
        }
        else if (effectSoundClip == null)
        {
            Debug.Log("PPBujeokEffectController: effectSoundClip이 할당되지 않음");
        }
    }
    private IEnumerator PlaySequentialSounds()
    {
        // Pre-Effect 사운드 재생
        if (preEffectSoundClip != null)
            audioSource.PlayOneShot(preEffectSoundClip);

        yield return new WaitForSeconds(mainSoundDelay);

        // Main-Effect 사운드 재생
        if (effectSoundClip != null)
            audioSource.PlayOneShot(effectSoundClip);
    }

    private IEnumerator CallCallbackAfterDuration(Action onComplete)
    {
        // HasStateAuthority 체크는 TriggerEffect에서 이미 했지만 안전을 위해 유지
        if (!Object.HasStateAuthority) 
            yield break;

        yield return new WaitForSeconds(particle != null ? particle.main.duration : 3f);

        // 호스트에서만 콜백(ApplyTalismanToTargets)을 실행
        _onCompleteCallback?.Invoke();

        // 호스트에서만 오브젝트를 Despawn
        Runner.Despawn(Object);
    }
}
