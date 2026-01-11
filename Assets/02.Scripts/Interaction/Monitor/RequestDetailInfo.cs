using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequestDetailInfo : NetworkBehaviour
{
    //코드 담당자 : 최은주 
    //SO에서 정보를 받아 자체적으로 세팅

    GameObject tv;
    TVscreen tvscreen;

    public TextMeshProUGUI detailInfo; //상세 내용
    public Image mapimage; //맵 이미지 
    int dangerLevelCount; //위험도
    int weridPMCount; //이상 현상 개수

    public TextMeshProUGUI acceptButtonText;
    public TextMeshProUGUI reqStatus;

    public TextMeshProUGUI dangerLevel;
    public TextMeshProUGUI weridPM;
    public TextMeshProUGUI title; //의뢰 이름
    public Button acceptButton;

    public AbnormalSpawner abnormalSpawner; // 추가 : 최서영

    DetailInfo request;
    public int uiIndex; //순서

    private void Start()
    {
        if(!Runner.IsServer)
        {
            acceptButton.gameObject.SetActive(false);
        }

        acceptButtonText.text = "의뢰 수락";
        reqStatus.text = "미해결";
        acceptButton.onClick.AddListener(RequestAccept);
        tv = GameObject.FindWithTag("TV");
        tvscreen = tv.GetComponent<TVscreen>();        
    }

    public void SetDetail(DetailInfo infoIndex) //SO 정보 받음
    {
        request = infoIndex;
        uiIndex = infoIndex.requestOrder;

        mapimage.sprite = infoIndex.mapImage;
        title.text = infoIndex.requestName;
        dangerLevelCount = infoIndex.dangerness;
        weridPMCount = infoIndex.weirdPhenomenon;
        dangerLevel.text = "의뢰 난이도 : " + dangerLevelCount;
        weridPM.text = "이상 현상 :" + weridPMCount + "개";
        detailInfo.text = infoIndex.requestContent;
    }

    public void RequestAccept() //의뢰 수락
    {
        Debug.Log($"[RequestAccept] called on {Object.InputAuthority}, HasStateAuthority={Object.HasStateAuthority}");
        RPC_NotifyRequest();      
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyRequest()
    {
        RequestManager.Instance.SetRequestState(RequestState.IngReq); //의뢰 받음으로 전환
        Debug.Log("의뢰중 상태로 전환");

        acceptButtonText.text = "수락 완료";
        acceptButton.interactable = false;
        reqStatus.text = "해결 중";

        foreach (var ui in RequestManager.Instance.reqDatas)
        {
            if (ui.index == uiIndex)
            {
                ui.SetStatus("해결 중");
            }
        }
        tvscreen.RPC_ShowNowRequest(request.requestName, request.dangerness, request.weirdPhenomenon, request.requestContent);

        // 추가 : 최서영
        if (abnormalSpawner != null)
            abnormalSpawner.SpawnAnomalies(weridPMCount);
        else
            Debug.LogError("AbnormalSpawner가 할당되지 않았습니다.");
    }


}
