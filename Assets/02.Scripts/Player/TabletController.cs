//코드 담당자: 유호정
using UnityEngine;
using UnityEngine.InputSystem;

public class TabletController : MonoBehaviour
{
    public GameObject tabletCanvas;
    public GameObject settingCanvas;
    public GameObject monitorCanvas;
    public MonitorInteract monitor;
    private SettingController setting;

    // private Animator playerAnimator;

    [Header("Component Deactivation")]
    private PlayerController playerController;
    private PlayerInteraction playerInteraction;
    private InventoryManager inventoryManager;


    private bool isTabletOpen = false;

    bool findMonitor = false;

    private void Awake()
    {
        // if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerInteraction == null) playerInteraction = GetComponent<PlayerInteraction>();
        if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
        if (settingCanvas != null) setting = settingCanvas.GetComponent<SettingController>();
    }

    // 함수 추가 : 정하윤
    private void Start()
    {
        if (tabletCanvas != null)
        {
            tabletCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (!findMonitor)
        {
            FindMonitor();
        }
    }

    public void OnTablet(InputValue value)
    {
        ToggleTablet();
    }


    // 함수 수정 : 정하윤
    public void ToggleTablet()
    {
        // if (playerAnimator != null)
        //     playerAnimator.SetBool("IsViewingTablet", isTabletOpen);


        isTabletOpen = !isTabletOpen;
        if (tabletCanvas != null)
            tabletCanvas.SetActive(isTabletOpen);

        playerController.SetPaused(isTabletOpen);
        playerInteraction.enabled = !isTabletOpen;
        inventoryManager.SetPaused(isTabletOpen);

        Cursor.lockState = isTabletOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isTabletOpen;
    }

    //함수 추가: 최은주
    public void OnSetting(InputValue value)
    {
        if (playerController == null) return;

        // 추가: 김수아
        if (KioskTrigger.CloseCurrentKiosk())
            return;

        if (monitor != null && monitor.isMonitor)
        {
            //if (monitor.Runner && monitor.isMonitor) //CurrentUser == playerController.Runner.LocalPlayer 제외
            Debug.Log("모니터 나가기 요청 (OnSetting)");
            monitor.StopInteraction();
            return;
        }

        if (!setting.isTurnon)
        {
            setting.TurnOnUI();
            Debug.Log("ESC누름");
        }
    }

    void FindMonitor()
    {
        if (monitorCanvas != null)
        {
            monitorCanvas = GameObject.FindWithTag("Monitor");
            //monitor = GameObject.FindWithTag("Computer").GetComponent<MonitorInteract>();
            findMonitor = true;
        }
    }
}