//코드 담당자: 유호정
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // 추가: 김수아
    // 일반, 감지용, 제령 아이템 구분
    public enum EItemType { General, Detect, Exorcism }

    [Header("Identification")]
    public int itemID; // 멀티/서버를 위한 아이템 고유 식별 ID 추가

    [Header("Data")]
    public EItemType itemType = EItemType.General;
    public string itemName;
    public int maxUseCount;
    public Sprite icon;             // 추가 : 정하윤
    public string description;      // 추가 : 정하윤
    public bool canUse;             // 추가 : 정하윤
    public int animationID;

    [Header("Detection")]
    public List<GhostSpawner.EGhost> canDetect;
    public bool wasUse = false; // 설치형 아이템 한번 반응했으면 재사용 안 되도록 하는 변수: 김수아

    [Header("Prefab")]
    public GameObject itemPrefab;
    public NetworkPrefabRef dropPrefab;     // GameObject -> NetworkPrefabRef 형태로 변경 : 정하윤

    [Header("Installable")]
    public bool isInstallable = false;
    public NetworkPrefabRef installPrefab; // GameObject -> NetworkPrefabRef 형태로 변경 : 정하윤

    [Header("Placement")]
    public GameObject previewPrefab;
    public LayerMask placementLayerMask;
    public float placementOffset = 0.1f; //설치 시 바닥에서 offset 높이만큼 띄웁니다.
    public float placementCheckRadius = 1.0f; //설치 시 장애물 체크 반경
}

// itemPrefab: 인벤토리에 들어갔을 때 손에 들리는 오브젝트, IUsable 상속 필요
// dropPrefab: 바닥에 떨어졌을 때의 오브젝트, IInteractable 상속 필요, ItemObject 컴포넌트 필요, rigidbody, collider 필요
// 사용횟수에 제한이 없는 아이템은 maxUseCount를 99로 설정해 주세요.