using UnityEngine;

public class MarriageCostume : ExorcismItemBase
{
    protected override void OnUseSuccess()
    {
        Debug.Log("혼례복 사용");
    }
}
