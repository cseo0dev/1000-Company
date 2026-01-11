//코드 담당자 : 최우석
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct PlayerReportData
{
    public string PlayerName;
    public bool IsSurvived;
    // public string PlayerRank; // "직급" - 추후 여기에 추가
}

public class GameResultData : MonoBehaviour
{
    public static GameResultData Instance { get; private set; }

    // --- 결과 씬으로 전달할 데이터 ---

    // 1. 게임오버 여부
    public bool IsGameOver { get; set; } = false;

    // 2. 집계 데이터
    public int TotalPlayerCount { get; set; }
    public int SurvivorCount { get; set; }
    public int GuessFailCount { get; set; } // 최대 3
    public int AnomaliesRemovedCount { get; set; } // 최대 3

    // 3. 플레이어 개별 데이터
    public List<PlayerReportData> PlayerReports { get; private set; } = new List<PlayerReportData>();

    // ------------------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 새 씬으로 가기 직전, 데이터를 초기화하고 설정하는 함수
    public void PrepareData(bool isGameOver)
    {
        this.IsGameOver = isGameOver;

        // 이전 데이터 초기화
        PlayerReports.Clear();
        TotalPlayerCount = 0;
        SurvivorCount = 0;
        GuessFailCount = 0;
        AnomaliesRemovedCount = 0;
    }

    // 플레이어 데이터 추가
    public void AddPlayerReport(PlayerReportData data)
    {
        PlayerReports.Add(data);
    }
}