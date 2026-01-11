using UnityEngine;
using Fusion;
using System.Collections;

public class DropPrefabDebug : NetworkBehaviour
{
    private Rigidbody rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        

        Debug.LogWarning($"[DropPrefabDebug] {name} Spawned. Pos: {transform.position}, " +
                         $"HasStateAuth: {Object.HasStateAuthority}, " +
                         $"IsClient: {Runner.IsClient}, IsServer: {Runner.IsServer}");

        if (rb != null)
        {

            if (!Object.HasStateAuthority) 
            {
                // rb.isKinematic = true;
            }
        }
        

        if (Runner.IsClient) 
        {
            StartCoroutine(CheckPositionAfterDelay());
        }
    }

    private IEnumerator CheckPositionAfterDelay()
    {

        yield return new WaitForSeconds(0.5f);
        Debug.LogWarning($"[DropPrefabDebug] Client position after 0.5s: {transform.position}");
        

        yield return new WaitForSeconds(1.5f);
        Debug.LogWarning($"[DropPrefabDebug] Client position after 2.0s: {transform.position}");
    }
}