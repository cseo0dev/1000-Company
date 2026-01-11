using UnityEngine;

[CreateAssetMenu(fileName = "RequestInformation", menuName = "Scriptable Objects/RequestInformation")]
public class RequestInformation : ScriptableObject
{
    //코드담당자 : 최은주 
    //의뢰 정보 담아놓는 SO. 유니티 에디터에서 의뢰 정보 추가하면 됨

    public DetailInfo[] detailInfo;
}

[System.Serializable]
public class DetailInfo
{
    public int requestOrder;
    public string requestName;
    public int dangerness;

    public int weirdPhenomenon;
    public string requestContent;
    public string address;

    public Sprite mapImage;
}