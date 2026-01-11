//코드 담당자: 유호정
using UnityEngine;
using System.Collections;
using System.Linq;
using Fusion;

public class Radio : NetworkBehaviour
{
    private enum ERadioState
    {
        Initializing,
        Static,
        GhostSound
    }

    [Header("Audio Clips")]
    public AudioClip staticNoiseClip;
    public AudioClip ghostSoundClip;

    [Header("Settings")]
    public float minCheckInterval = 3.0f;
    public float maxCheckInterval = 7.0f;
    [Range(0f, 1f)] public float ghostSoundChance = 0.9f;


    private AudioSource audioSource;
    private ItemData itemData;
    private bool _canDetectCurrentGhost = false;
    private GhostSpawner _ghostSpawner;


    private ERadioState _previousState;

    [Networked]
    private ERadioState CurrentState { get; set; }

    [Networked]
    private TickTimer NextCheckTimer { get; set; }


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        itemData = GetComponent<ItemObject>()?.itemData;

        if (staticNoiseClip == null || ghostSoundClip == null || itemData == null)
        {
            Debug.LogError($"[{gameObject.name}] 필수 컴포넌트(AudioClips, ItemData)가 없습니다.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        PlayStaticNoise();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentState = ERadioState.Initializing;
        }


        _previousState = CurrentState;

        HandleStateChange(CurrentState);
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentState == ERadioState.Initializing)
        {
            if (_ghostSpawner == null)
            {
                _ghostSpawner = GhostSpawner.Instance;
                return;
            }

            GhostSpawner.EGhost currentGhostType = _ghostSpawner.mapGhostType;
            if (itemData.canDetect != null)
            {
                _canDetectCurrentGhost = itemData.canDetect.Contains(currentGhostType);
            }
            Debug.Log($"[Radio] 초기화 완료. 맵 귀신: {currentGhostType}, 감지 가능: {_canDetectCurrentGhost}");

            if (Object.HasStateAuthority)
            {
                CurrentState = ERadioState.Static;
                float initialDelay = Random.Range(minCheckInterval, maxCheckInterval);
                NextCheckTimer = TickTimer.CreateFromSeconds(Runner, initialDelay);
            }
            return;
        }


        if (!Object.HasStateAuthority || !NextCheckTimer.Expired(Runner))
        {
            return;
        }


        if (CurrentState == ERadioState.Static)
        {
            if (_canDetectCurrentGhost && Random.value < ghostSoundChance)
            {
                CurrentState = ERadioState.GhostSound;
                NextCheckTimer = TickTimer.CreateFromSeconds(Runner, ghostSoundClip.length + 0.5f);
            }
            else
            {
                float nextDelay = Random.Range(minCheckInterval, maxCheckInterval);
                NextCheckTimer = TickTimer.CreateFromSeconds(Runner, nextDelay);
            }
        }
        else if (CurrentState == ERadioState.GhostSound)
        {
            CurrentState = ERadioState.Static;
            float nextDelay = Random.Range(minCheckInterval, maxCheckInterval);
            NextCheckTimer = TickTimer.CreateFromSeconds(Runner, nextDelay);
        }
    }

    public override void Render()
    {
        if (CurrentState != _previousState)
        {
            HandleStateChange(CurrentState);
            _previousState = CurrentState;
        }
    }


    private void HandleStateChange(ERadioState state)
    {
        if (audioSource == null) return;
        
        switch (state)
        {
            case ERadioState.Initializing:
            case ERadioState.Static:
                PlayStaticNoise();
                break;
            case ERadioState.GhostSound:
                PlayGhostSound();
                break;
        }
    }


    private void PlayStaticNoise()
    {
        if (audioSource.clip != staticNoiseClip || !audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = staticNoiseClip;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("라디오에서 잡음이 들린다.");
        }
    }

    private void PlayGhostSound()
    {
        if (audioSource.clip != ghostSoundClip || !audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = ghostSoundClip;
            audioSource.loop = false;
            audioSource.Play();
            Debug.Log("라디오에서 비명소리가 들린다.");
        }
    }
}