using UnityEngine;

public class Burner : ExorcismItemBase
{
    protected override void OnUseSuccess()
    {
        Debug.Log("향로 사용");
    }
}
