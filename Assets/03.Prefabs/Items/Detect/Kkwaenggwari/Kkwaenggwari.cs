//코드 담당자: 유호정
using UnityEngine;
using System.Collections.Generic;

public class Kkwaenggwari : MonoBehaviour
{
    public float detectionRadius = 2.5f;
    public LayerMask ghostLayerMask;
    public string animationTriggerName = "DetectGhost";
    private AudioSource audioSource;
    private Animator animator;
    private SphereCollider triggerCollider;
    private ItemData itemData;
    private GhostSpawner.EGhost currentGhostType;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<SphereCollider>();
        itemData = GetComponent<ItemObject>()?.itemData;


        GhostSpawner ghostSpawner = FindFirstObjectByType<GhostSpawner>();
        if (ghostSpawner != null)
        {
            currentGhostType = ghostSpawner.mapGhostType;
        }
        else
        {
            currentGhostType = GhostSpawner.EGhost.Jibakreong;
        }
        


        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((ghostLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {

            if (itemData.canDetect.Contains(currentGhostType))
            {
                if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
                {
                    animator.SetTrigger(animationTriggerName);
                }
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }
            }
            else
            {
                Debug.Log("꽹과리로 감지 불가능한 적입니다.");
            }
        }
    }
        

    // private void OnTriggerExit(Collider other)
    // {
    //     if ((ghostLayerMask.value & (1 << other.gameObject.layer)) > 0)
    //     {
           
    //     }
    // }

    
}