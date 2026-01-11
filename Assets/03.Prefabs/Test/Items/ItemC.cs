using UnityEngine;

public class ItemC : MonoBehaviour, IUsable
{
    public float protectionDuration = 10f;
    public void Use()
    {
        PlayerCondition playerCondition = GetComponentInParent<PlayerCondition>();
        if (playerCondition != null)
        {
            // playerCondition.ApplyTimedProtection(protectionDuration);
        }
        
        Debug.Log("ItemC가 사용되어 플레이어가 일정 시간 동안 공격을 막습니다");
    }
}
