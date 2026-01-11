using Fusion;
using UnityEngine;

public class test : MonoBehaviour
{
    public int damageAmount = 3;

    private void OnTriggerEnter(Collider other)
    {



        if (other.TryGetComponent<PlayerCondition>(out var playerCondition))
        {
            if (playerCondition.IsDead) return;
            Debug.Log($"[서버] {other.name}이(가) 데미지 트리거에 닿음! 데미지 {damageAmount} 적용.");
            playerCondition.Rpc_TakeDamage(damageAmount);
        }
    }
}
