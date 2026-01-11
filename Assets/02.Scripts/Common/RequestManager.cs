using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameModeState //게임 모드
{
    Single,
    Multi
}
public enum ReadyState //플레이어 레디상태 관리 
{
    None,
    AllReady
}

public enum RequestState //플레이어 의뢰 상태 관리
{
    NoneReq,
    IngReq,
    EndReq,
    StartReq
}

public class RequestManager : SingleTon<RequestManager>
{
    //코드 담당자: 최은주 
    //의뢰 상태를 관리하고 새 의뢰를 생성함

    [SerializeField] GameObject requestUiprefab;
    [SerializeField] GameObject requestDetailPrefab;
    [SerializeField] Transform uiParent;
    [SerializeField] Transform questParent;
    public GameModeState gameMode { get; set; } = GameModeState.Single;

    [Networked]
    public ReadyState readyState { get; set; } = ReadyState.None;
    public RequestState nowState { get; set; } = RequestState.StartReq;

    //bool isQuestEnd = false; //게임 매니저에서 성과 보고서 나오고 확인 누르면 의뢰 완료 true로
    public int currQuestIndex = 0;

    RequestInformation requestInfo;
    List<DetailInfo> requestDatas;

    public List<RequestDetailInfo> detailDatas = new List<RequestDetailInfo>();
    public List<RequestUI> reqDatas = new List<RequestUI>();


    public void Start()
    {

        requestInfo = Resources.Load<RequestInformation>("ScriptableObject/RequestInformation");

        if (requestInfo == null)
        {
            Debug.LogError("RequestInformation SO를 찾을 수 없습니다.");
            return;
        }

        requestDatas = new List<DetailInfo>(requestInfo.detailInfo); //배열을 리스트로 변환
        InstanceRequest();
    }

    public void SetRequestState(RequestState state)
    {
        if (nowState == state) return;

        nowState = state;
    }

    public void SetReadyState(ReadyState state)
    {
        if (readyState == state) return;

        readyState = state;
    }


    public void SetGameMode(GameModeState mode)
    {
        if (gameMode == mode) return;

        gameMode = mode;
    }

    public void InstanceRequest() //의뢰 리스트 생성
    {
        if (nowState == RequestState.EndReq || nowState == RequestState.StartReq)
        {
            SetRequestState(RequestState.NoneReq); //상태 None으로 바꿔주고

            GameObject ui = Instantiate(requestUiprefab, questParent);
            RequestUI requestUI = ui.GetComponent<RequestUI>();
            reqDatas.Add(requestUI);
            requestUI.SetInfo(requestDatas[currQuestIndex]);

            InstanceReqDetail();
            currQuestIndex++;
        }
    }

    public void InstanceReqDetail() //하위 프리팹 고정 위치에 생성
    {
        GameObject reqUi = Instantiate(requestDetailPrefab, uiParent);
        RequestDetailInfo detailInfoUi = reqUi.GetComponent<RequestDetailInfo>();
        detailInfoUi.SetDetail(requestDatas[currQuestIndex]);
        detailInfoUi.gameObject.SetActive(false);
        detailDatas.Add(detailInfoUi);
    }
}