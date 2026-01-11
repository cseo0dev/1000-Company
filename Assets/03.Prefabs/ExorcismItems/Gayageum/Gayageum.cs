using UnityEngine;

public class Gayageum : ExorcismItemBase
{
    protected override void OnUseSuccess()
    {
        Debug.Log("가야금 사용");
    }
}
