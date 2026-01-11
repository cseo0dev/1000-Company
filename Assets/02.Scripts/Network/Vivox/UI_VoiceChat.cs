using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.Services.Vivox;
using Fusion;

// 코드 담당자: 김수아
// Fusion 전용 UI 매니저
public class UI_VoiceChat : NetworkBehaviour
{
    [SerializeField] private GameObject chatPrefab;
    [SerializeField] private Transform chatParent;

    private Dictionary<string, Chat> chatDict = new Dictionary<string, Chat>();

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            VivoxManager.Instance.OnParticipantChangedEvent += OnParticipantChanged;
        }

        VivoxManager.Instance.OnSpeechDetectedEvent += OnSpeechDetected;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasInputAuthority)
        {
            VivoxManager.Instance.OnParticipantChangedEvent -= OnParticipantChanged;
        }

        VivoxManager.Instance.OnSpeechDetectedEvent -= OnSpeechDetected;
    }

    public void OnParticipantChanged(List<VivoxParticipant> participants)
    {
        foreach (Transform child in chatParent)
            Destroy(child.gameObject);

        chatDict.Clear();

        foreach (var p in participants)
        {
            var chatObj = Instantiate(chatPrefab, chatParent);
            var chat = chatObj.GetComponent<Chat>();

            chat.Setup(p.DisplayName); // PlayerName 기본 표시
            chatDict[p.DisplayName] = chat;
        }
    }
    
    private void OnSpeechDetected(string playerName, bool speaking)
    {
        if (chatDict.TryGetValue(playerName, out var chat))
            chat.SetMicActive(speaking); // MicIcon 표시/숨김
    }
}