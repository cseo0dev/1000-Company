//코드 담당자: 유호정
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using Fusion; 

public class Thermometer : MonoBehaviour, IUsable
{
    [Header("Temperature Settings")]
    public TextMeshProUGUI temperatureText; 
    public float normalTempMin = 15.0f;     
    public float normalTempMax = 25.0f;     
    public float coldTempMin = -10.0f;      
    public float coldTempMax = 5.0f;        
    public float updateInterval = 0.5f;     

    [Header("Detection Logic")]
    public float minCheckInterval = 5.0f;   
    public float maxCheckInterval = 10.0f;  
    [Range(0f, 1f)] public float ghostColdChance = 0.9f; //테스트를 위해 확률을 90%로 설정했습니다.
    public AudioClip temperatureDropSound;

    [Header("Animation")]
    public string useAnimationBool = "IsViewingThermometer";
    private Animator viewModelAnimator;
    // private Animator playerAnimator;

    private AudioSource audioSource;
    private ItemData itemData;
    private InventoryManager inventoryManager;

    private GhostSpawner.EGhost currentGhostType;
    private bool canDetectCurrentGhost = false;
    private bool isCurrentlyCold = false;     
    private float currentDisplayTemp;       

    private Coroutine temperatureCheckCoroutine;
    private Coroutine updateDisplayCoroutine;
    GhostSpawner ghostSpawner;

    private bool isInitialized = false; 

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    private void Start()
    {
        TryInitialize();
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            TryInitialize();
            if (!TryInitialize())
            {
                return;
            }
        }
        StartCoroutines();
    }

    private bool TryInitialize()
    {
        if (isInitialized) return true;

        if (inventoryManager == null)
        {
            enabled = false;
            return false;
        }
        if (inventoryManager.viewModelHandsObject != null)
        {
            viewModelAnimator = inventoryManager.viewModelHandsObject.GetComponentInChildren<Animator>();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] InventoryManager에 viewModelHandsObject가 연결되지 않았습니다.");
        }

        int currentIndex = inventoryManager.CurrentSlotIndex;
        if (currentIndex < 0 || currentIndex >= inventoryManager.SyncedSlots.Length)
        {
            return false; 
        }

        NetworkInventorySlot currentSlot = inventoryManager.SyncedSlots[currentIndex];
        if (currentSlot.IsEmpty())
        {
            return false;
        }

        itemData = ItemDatabase.GetItemDataFromID(currentSlot.ItemID);
        if (itemData == null)
        {
            Debug.LogError($"[{gameObject.name}] ItemID {currentSlot.ItemID}에 해당하는 ItemData를 찾을 수 없습니다", this);
            enabled = false;
            return false;
        }
        if (ghostSpawner == null)
        {
            InitializeThermometer();//
        }
        if (ghostSpawner != null)
        {
            isInitialized = true;//
            return true;//
        }
        return false;
    }

    private void StartCoroutines()
    {
        if (updateDisplayCoroutine == null)
        {
            updateDisplayCoroutine = StartCoroutine(UpdateDisplayRoutine());
        }

        if (canDetectCurrentGhost && temperatureCheckCoroutine == null)
        {
            temperatureCheckCoroutine = StartCoroutine(TemperatureCheckRoutine());
        }
    }


    private void OnDisable()
    {
        if (temperatureCheckCoroutine != null)
        {
            StopCoroutine(temperatureCheckCoroutine);
            temperatureCheckCoroutine = null;
        }
        if (updateDisplayCoroutine != null)
        {
            StopCoroutine(updateDisplayCoroutine);
            updateDisplayCoroutine = null;
        }
        if (isInitialized)
        {
            SetAnimBool(false);
        }
    }

    private void InitializeThermometer()
    {
        ghostSpawner = GhostSpawner.Instance;
        if (ghostSpawner != null)
        {
            currentGhostType = ghostSpawner.mapGhostType;
        }
        // else
        // {
        //     currentGhostType = GhostSpawner.EGhost.Jibakreong; //기본값: 지박령
        // }

        canDetectCurrentGhost = itemData.canDetect.Contains(currentGhostType);
        Debug.Log($"[Thermometer] 현재 귀신: {currentGhostType}, 감지 가능: {canDetectCurrentGhost}");

        isCurrentlyCold = false;
        currentDisplayTemp = Random.Range(normalTempMin, normalTempMax);
        UpdateTemperatureDisplay();
    }


    private IEnumerator TemperatureCheckRoutine()
    {
        yield return new WaitForSeconds(0.5f); 

        while (true)
        {
            float waitTime = Random.Range(minCheckInterval, maxCheckInterval);
            yield return new WaitForSeconds(waitTime);

            if (Random.value < ghostColdChance)
            {
                if (!isCurrentlyCold)
                {
                    isCurrentlyCold = true;
                    Debug.Log($"[{itemData.itemName}] 온도가 떨어졌습니다.");

                    if (audioSource != null && temperatureDropSound != null)
                    {
                        audioSource.PlayOneShot(temperatureDropSound);
                    }
                }
            }
            else
            {
                if (isCurrentlyCold)
                {
                    isCurrentlyCold = false;
                    Debug.Log($"[{itemData.itemName}] 온도가 정상으로 돌아왔습니다.");
                }
            }
        }
    }

    private IEnumerator UpdateDisplayRoutine()
    {
        while (true)
        {
            if (canDetectCurrentGhost && isCurrentlyCold)
            {
                currentDisplayTemp = Random.Range(coldTempMin, coldTempMax);
            }
            else
            {
                currentDisplayTemp = Random.Range(normalTempMin, normalTempMax);
            }

            UpdateTemperatureDisplay();

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateTemperatureDisplay()
    {
        if (temperatureText != null)
        {
            temperatureText.text = $"{currentDisplayTemp:F1}C";
        }
    }

    public void Use()
    {
        if (!isInitialized) TryInitialize();
        Debug.Log("온도계를 자세히 봅니다.");
        bool newState = true;


        if (viewModelAnimator != null)
        {
            newState = !viewModelAnimator.GetBool(useAnimationBool);
            Debug.Log("온도계를 자세히 보는 애니메이션");
        }
        


        SetAnimBool(newState);
    }
    
    private void SetAnimBool(bool state)
    {
        if (viewModelAnimator != null)
        {
            viewModelAnimator.SetBool(useAnimationBool, state);
        }
    }
}

