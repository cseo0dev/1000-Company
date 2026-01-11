using UnityEngine;

public class ItemB : MonoBehaviour, IUsable
{

    public float sanityDamage = 15f;
    public void Use()
    {

        // PlayerCondition playerCondition = GetComponentInParent<PlayerCondition>();
        // if (playerCondition != null)
        // {
        //     playerCondition.DecreaseSanity(sanityDamage);
        // }
        // Debug.Log("ItemB가 사용되어 플레이어의 정신력이 감소했습니다.");
    }
}
