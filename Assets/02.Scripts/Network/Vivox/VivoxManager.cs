using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

// 코드 담당자: 김수아
/// <summary>
/// Vivox 초기화 / 방 ID로 채널 관리(Join/Leave)
/// 사망시 Ghost채널로 이동해 음성 분리
/// </summary>

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance;
    public bool MainChannelConnected { get; private set; }

    // room 채널 이름들
    public string RoomId { get; private set; }

    [Header("Default Channels")]
    [Tooltip("캐싱할 채널 이름")]
    public string mainChannel;
    private string ghostChannel;

    [Header("3D Voice Settings")]
    [SerializeField] private int audioDistance = 32; // 가청거리 : 어디까지 목소리 들릴지
    [SerializeField] private int conversationalDistance = 1; // 작아지기 시작하는 거리
    [SerializeField] private float audioFadeInByDistance = 1.0f; // 값이 1.0보다 크면 대화 거리에서 멀어질수록 오디오가 더 빨리 사라짐, 값이 1.0보다 작으면 오디오가 더 느리게 사라짐. 기본값은 1.0.

    private bool isDead = false;
    private bool isMainJoined = false;
    private bool isGhostJoined = false;

    private bool initialized = false;
    private bool loggedIn = false;

    // UI를 위한 Actions
    public Action<List<VivoxParticipant>> OnParticipantChangedEvent; // UI에 인원 추가 전달용
    public Action<string, bool> OnSpeechDetectedEvent; // 로컬이 말하는 거 감지
    public Action<string, float> OnVolumeChangedEvent; // 볼륨 슬라이더 조절용
    public Action<string, bool> OnMuteChangedEvent; // 음성 뮤트용

    public List<VivoxParticipant> participantsList;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        participantsList = new List<VivoxParticipant>();
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        participantsList.Add(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);

        participant.ParticipantSpeechDetected += () =>
        {
            Debug.Log($"{participant.DisplayName} Speaking: {participant.SpeechDetected}");
            OnSpeechDetectedEvent?.Invoke(participant.DisplayName, participant.SpeechDetected);
        };
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        participantsList.Remove(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);
    }

    /// <summary>
    /// Vivox 서버 초기화 함수
    /// </summary>
    public async Task InitVivox()
    {
        if (initialized) return;
        initialized = true;

        await UnityServices.InitializeAsync(); //유니티 서비스 초기화
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //AuthenticationService를 사용하여 익명 인증
        await VivoxService.Instance.InitializeAsync(); //Vivox 초기화

        // 초기화 후 이벤트 등록
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }

    /// <summary>
    /// Vivox 로그인 -> 초기화 이후 호출
    /// </summary>
    public async Task LoginVivox()
    {
        if (loggedIn) return;
        loggedIn = true;

        //로그인 옵션 생성
        LoginOptions options = new LoginOptions();

        //디스플레이 이름 설정
        options.DisplayName = Guid.NewGuid().ToString();
        // options.DisplayName = AuthManager.Instance.userName;

        //로그인
        await VivoxService.Instance.LoginAsync(options);
        Debug.Log("Vivox 로그인");
    }

    /// <summary>
    /// Vivox 로그아웃 함수
    /// </summary>
    public async Task VivoxLogoutAsync()
    {
        if (!VivoxService.Instance.IsLoggedIn) return;

        await VivoxService.Instance.LogoutAsync();
        Debug.Log("Vivox 로그아웃");
    }

    /// <summary>
    /// 룸 입장 시 호출 (roomID에 따라 채널 자동 설정)
    /// 3D 채널을 사용해 음성 거리별 효과 줌
    /// </summary>
    public async Task JoinMainChannel(string roomId)
    {
        if (!VivoxService.Instance.IsLoggedIn) return;

        RoomId = roomId;
        mainChannel = $"Room_{RoomId}_Main";

        var props = new Channel3DProperties(audioDistance, conversationalDistance, audioFadeInByDistance, AudioFadeModel.InverseByDistance);

        await VivoxService.Instance.JoinPositionalChannelAsync(mainChannel, ChatCapability.AudioOnly, props);

        MainChannelConnected = true;
        isMainJoined = true;
        isGhostJoined = false;
        Debug.Log($"[VivoxVoice] Joined MainChannel: {mainChannel}");
    }

    public async Task JoinGhostChannel(string roomId)
    {
        ghostChannel = $"Room_{roomId}_Ghost";

        // Ghost채널은 거리별 볼륨 변화 없음
        await VivoxService.Instance.JoinGroupChannelAsync(ghostChannel, ChatCapability.AudioOnly);
        Debug.Log($"[Vivox] Joined Ghost 2D Channel: {ghostChannel}");
    }

    public async Task LeaveMainChannel(string channelName)
    {
        await VivoxService.Instance.LeaveChannelAsync(channelName);
        MainChannelConnected = false;
        Debug.Log($"Main 채널 떠남: {channelName}");
    }

    public async Task LeaveGhostChannel(string channelName)
    {
        await VivoxService.Instance.LeaveChannelAsync(channelName);
        Debug.Log($"Ghost 채널 떠남: {channelName}");
    }

    /// <summary>
    /// 모든 채널 떠나기
    /// </summary>
    public async Task LeaveAllChannels()
    {
        if (isMainJoined) await VivoxService.Instance.LeaveChannelAsync(mainChannel);
        if (isGhostJoined) await VivoxService.Instance.LeaveChannelAsync(ghostChannel);
        isMainJoined = isGhostJoined = false;
    }

    /// <summary>
    /// 플레이어 생존/사망 상태에 따라 채널 변경
    /// </summary>
    public async void UpdatePlayerState(bool dead)
    {
        if (isDead == dead) return;
        isDead = dead;

        if (dead)
        {
            if (isMainJoined)
            {
                await LeaveMainChannel(mainChannel);
                isMainJoined = false;
            }

            if (!isGhostJoined)
            {
                await JoinGhostChannel(RoomId);
                isGhostJoined = true;
            }

            Debug.Log("[VivoxVoice] → GhostChannel 이동");
        }
        else
        {
            if (isGhostJoined)
            {
                await LeaveGhostChannel(ghostChannel);
                isGhostJoined = false;
            }

            if (!isMainJoined)
            {
                await JoinMainChannel(RoomId);
                isMainJoined = true;
            }

            Debug.Log("[VivoxVoice] → MainChannel 복귀");
        }
    }

    /// <summary>
    /// 플레이어별 음량 조절(슬라이더)
    /// </summary>
    public void SetPlayerVolume(string displayName, float volume)
    {
        var participant = participantsList.Find(p => p.DisplayName == displayName);
        if (participant == null) return;

        int volumeValue = Mathf.RoundToInt(Mathf.Lerp(-50f, 50f, Mathf.Clamp01(volume)));
        participant.SetLocalVolume(volumeValue);
        OnVolumeChangedEvent?.Invoke(displayName, volume);
    }

    // 토글로 뮤트/언뮤트 (로컬)
    private Dictionary<string, bool> localMuteStates = new Dictionary<string, bool>();

    public void TogglePlayerMute(string displayName)
    {
        var participant = participantsList.Find(p => p.DisplayName == displayName);
        if (participant == null) return;

        bool currentlyMuted = false;
        localMuteStates.TryGetValue(displayName, out currentlyMuted);

        if (!currentlyMuted)
        {
            participant.MutePlayerLocally();

            localMuteStates[displayName] = true;
            OnMuteChangedEvent?.Invoke(displayName, true);
        }
        else
        {
            participant.UnmutePlayerLocally();

            localMuteStates[displayName] = false;
            OnMuteChangedEvent?.Invoke(displayName, false);
        }
    }
}