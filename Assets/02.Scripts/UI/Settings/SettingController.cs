using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class SettingController : NetworkBehaviour
{
    public GameObject pauseUi;
    public GameObject SettingUI;
    public Button resumeBtn;
    public Button settingBtn;
    public Button outBtn;

    public bool isTurnon = false;


    public PlayerController playerController;
    bool isPlayer = false;

    private void Start()
    {
        resumeBtn.onClick.AddListener(TurnOFFUI);
        settingBtn.onClick.AddListener(TrunOnSetting);
        outBtn.onClick.AddListener(OnApplicationQuit);
        //playerController = GetComponentInParent<PlayerController>();
    }
 

    public void TurnOnUI()
    {
        if(!isPlayer)
        {
            playerController = GetComponentInParent<PlayerController>();
            isPlayer = true;
        }
       
        isTurnon = true;
        pauseUi.SetActive(true);    
        playerController.isInputLocked = true;      
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;  
    }

    void TurnOFFUI() //돌아가기
    {
        playerController = GetComponentInParent<PlayerController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        
        playerController.isInputLocked = false;     
        Debug.Log("세팅창 꺼지기");
        isTurnon = false;
        pauseUi.SetActive(false);
    }
    void TrunOnSetting()
    {
        SettingUI.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        
    }
}
