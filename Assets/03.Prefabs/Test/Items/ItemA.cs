using UnityEngine;
using Fusion;

public class ItemA : NetworkBehaviour, IUsable
{

    [Header("Settings")]
    public int DamageAmount = 1;
    private PlayerCondition playerCondition;
    private InventoryManager inventoryManager;
    public void Initialize(InventoryManager invMan, ItemData data)
    {
        this.inventoryManager = invMan;
        if (invMan != null)
        {
            playerCondition = invMan.GetComponent<PlayerCondition>();
        }
    }
    public void Use()
    {
        if (inventoryManager != null && inventoryManager.HasInputAuthority)
        {
            if (playerCondition != null)
            {
                Debug.Log($"[TestDamageItem] 나 자신에게 Rpc_TakeDamage({DamageAmount}) 요청");
                playerCondition.Rpc_TakeDamage(DamageAmount);
            }
            else
            {
                Debug.LogWarning("[TestDamageItem] PlayerCondition을 찾을 수 없습니다!");
            }
        }
    }
}
