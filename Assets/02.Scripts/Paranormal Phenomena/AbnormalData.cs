// 코드 담당자 : 최서영
using UnityEngine;

[CreateAssetMenu(menuName = "Abnormal/AbnormalData")]
public class AbnormalData : ScriptableObject
{
    public EAbnormal type;
    public GameObject prefab;
    public bool spawnWall = false; // true면 벽 스폰
}
