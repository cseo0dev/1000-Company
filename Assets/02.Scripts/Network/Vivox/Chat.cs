using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 코드 담당자: 김수아
// UI 마이크 아이콘 활성화
public class Chat : MonoBehaviour
{
    [SerializeField] private Image micIcon;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button muteButton;
    [SerializeField] private TextMeshProUGUI nameText;

    private string displayName;
    private bool isMuted;

    public void Setup(string name)
    {
        displayName = name;
        nameText.text = name;

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        muteButton.onClick.AddListener(OnMuteClicked);
    }

    public void SetMicActive(bool active)
    {
        micIcon.enabled = active;
    }

    private void OnVolumeChanged(float value)
    {
        VivoxManager.Instance.SetPlayerVolume(displayName, value);
    }

    private void OnMuteClicked()
    {
        VivoxManager.Instance.TogglePlayerMute(displayName);
        isMuted = !isMuted;
        muteButton.GetComponentInChildren<TextMeshProUGUI>().text = isMuted ? "Unmute" : "Mute";
    }
}
