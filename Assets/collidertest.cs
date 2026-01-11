using UnityEngine;

public class collidertest : NetworkedTriggerEventSupporter
{
    protected override void OnTargetEnter(Collider other)
    {
        Debug.Log("충돌한다!!!!!!!!!");
    }

    protected override void OnTargetExit(Collider other)
    {
        Debug.Log("충돌 나갔다!!!!!!!!!");

    }

}
