using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 작성자 : 정하윤
public enum EGrade
{
    인턴,
    사원,
    대리,
    과장,
    팀장
}

[System.Serializable]
public class TeamMemberData
{
    public string nickName;
    public EGrade grade;
    public int CurrentMental = 100;
}
public class VitalCellUI : MonoBehaviour
{
    public int maxMental = 100;

    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI mentalStateText;
    [SerializeField] private TextMeshProUGUI mentalPercentText;
    [SerializeField] private Image BarImage;

    private TeamMemberData memberData;

    public void SetData(TeamMemberData data)
    {
        memberData = data;
        RefreshUI();
    }

    public void SetMental(int value)
    {
        memberData.CurrentMental = Mathf.Clamp(value, 0, maxMental);
        RefreshUI();
    }

    public void Upgrade(EGrade value)
    {
        memberData.grade = value;
        RefreshUI();
    }

    public void UpdateMental(int value)
    {
        memberData.CurrentMental = Mathf.Clamp(value, 0, maxMental);
        RefreshUI();
    }

    // UI 갱신
    public void RefreshUI()
    {
        nicknameText.text = memberData.nickName;
        gradeText.text = memberData.grade.ToString();
        mentalStateText.text = $"{memberData.CurrentMental} / {maxMental}";
        BarImage.fillAmount = maxMental > 0 ? (float)memberData.CurrentMental / maxMental : 0f;

        UpdateMentalStatus();
        UpdateMentalPercent();
    }

    // 정신력 상태 정보 갱신
    private void UpdateMentalStatus()
    {
        if (maxMental <= 0)
        {
            mentalStateText.text = "정보 없음";
            return;
        }

        float percent = (float)memberData.CurrentMental / maxMental;

        if (percent <= 0.25f)
            mentalStateText.text = "나쁨";
        else if (percent >= 0.75f)
            mentalStateText.text = "정상";
        else
            mentalStateText.text = "양호";
    }

    // 정신력 퍼센트 정보 갱신
    private void UpdateMentalPercent()
    {
        float value = (float)memberData.CurrentMental / maxMental;
        int percent = Mathf.RoundToInt(value * 100);
        mentalPercentText.text = $"{percent}%";
    }
}
