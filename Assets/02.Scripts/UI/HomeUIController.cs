using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 작성자 : 정하윤
public class HomeUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickNameText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI aspirationText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI mileageText;
    [SerializeField] private Image expFillImage;
    [SerializeField] private TextMeshProUGUI remainPerformance;

    private int currentPerformance= 0;
    private int maxPerformance = 100;

    private string nickname = "플레이어";
    private string grade = "팀장";
    private string aspiration = "화이팅";
    private int coin = 30000;
    private int mileage = 100;
    private void Start()
    {
        UpdateUserUI();
        UpdateUI();
    }

    // 최대 경험치 변경
    public void SetMaxPerformance(int max)
    {
        maxPerformance = Mathf.Max(1, max);
        UpdateUI();
    }

    public void SetPerformance(int value)
    {
        currentPerformance = Mathf.Clamp(value, 0, maxPerformance);
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 성괄 Bar UI 갱신
        float fillAmount = (float)currentPerformance / maxPerformance;
        expFillImage.fillAmount = fillAmount;

        // 승진까지 남은 성과 계산
        int remaining = maxPerformance - currentPerformance;

        // 승진까지 남은 성과 UI 갱신
        if (remainPerformance != null)
            remainPerformance.text = $"{remaining}";
    }

    // 경험치 추가
    public void AddPerformance(int amount)
    {
        SetPerformance(currentPerformance + amount);
    }

    public void SetNickname(string name)
    {
        nickname = name;
        UpdateUserUI();
    }
    public void SetAspiration(string comment)
    {
        aspiration = comment;
        UpdateUserUI();
    }

    public void SetGrade(string newGrade)
    {
        grade = newGrade;
        UpdateUserUI();
    }

    public void SetCoin(int amount)
    {
        coin = amount;
        UpdateUserUI();
    }
    public void SetMileage(int amount)
    {
        mileage = amount;
        UpdateUserUI();
    }

    private void UpdateUserUI()
    {
        if (nickNameText != null)
            nickNameText.text = nickname;

        if (gradeText != null)
            gradeText.text = grade;

        if (coinText != null)
            coinText.text = $"{coin} 원";

        if (aspirationText != null)
            aspirationText.text = aspiration;

        if (mileageText != null)
            mileageText.text = $"{mileage} 개";
    }
}
