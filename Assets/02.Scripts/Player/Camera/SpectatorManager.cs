using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class SpectatorManager : NetworkBehaviour
{
    public TextMeshProUGUI spectatorNameUI;

    private List<NetworkObject> _spectatablePlayers = new List<NetworkObject>();
    private int _currentTargetIndex = -1;
    
    private PlayerCameraManager _localCameraManager;
    private PlayerCameraManager _currentTargetCamManager;
    private ObserverOrbitControl _currentOrbitControl;
    private bool _isSpectateModeActive = false;

    void Awake()
    {
        _localCameraManager = GetComponent<PlayerCameraManager>();
    }

    void OnEnable()
    {
        if (spectatorNameUI != null)
            spectatorNameUI.gameObject.SetActive(true);
            UpdatePlayerList();

        _currentTargetIndex = _spectatablePlayers.IndexOf(this.Object);
        if (_currentTargetIndex == -1) _currentTargetIndex = 0;
        _currentTargetCamManager = _localCameraManager;
        UpdateSpectatorUI();
    }

    void OnDisable()
    {
        if (spectatorNameUI != null)
            spectatorNameUI.gameObject.SetActive(false);

        DisableCurrentSpectatorCam();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) CycleTarget(1);
        else if (scroll < -0f) CycleTarget(-1);
    }

    private void UpdatePlayerList()
    {
        _spectatablePlayers.Clear();

        if (Runner == null || Runner.ActivePlayers == null) return;

        foreach (PlayerRef playerRef in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
            {
                if (playerObject != null)
                {
                    _spectatablePlayers.Add(playerObject);
                }
            }
        }
    }

    private void CycleTarget(int direction)
    {
        UpdatePlayerList();
        if (_spectatablePlayers.Count == 0) return;
        int newIndex = (_currentTargetIndex + direction + _spectatablePlayers.Count) % _spectatablePlayers.Count;
        SwitchToTarget(newIndex);
    }

    private void SwitchToTarget(int index)
    {
        UpdatePlayerList();
        if (index < 0 || index >= _spectatablePlayers.Count) return;

        if (index == _currentTargetIndex && _currentTargetCamManager != null)
        {
             if (_spectatablePlayers.Count > 1) return;
        }

        DisableCurrentSpectatorCam();

        _currentTargetIndex = index;
        NetworkObject targetObject = _spectatablePlayers[_currentTargetIndex];

        var targetCondition = targetObject.GetComponent<PlayerCondition>();
        var targetCamManager = targetObject.GetComponent<PlayerCameraManager>();

        if (targetCondition == null || targetCamManager == null) return;
        _currentTargetCamManager = targetCamManager;
        if (targetCondition.IsDead)
        {
            targetCamManager.ActivateDeathCam();
        }
        else
        {
            targetCamManager.ActivateObserverCam();
            if (targetCamManager.observerCam != null) 
            {
                _currentOrbitControl = targetCamManager.observerCam.GetComponent<ObserverOrbitControl>();
                if (_currentOrbitControl != null)
                {
                    _currentOrbitControl.enabled = true;
                }
            }
        }
        
        // UI 텍스트 업데이트
        UpdateSpectatorUI();
    }
    private void DisableCurrentSpectatorCam()
    {
        if (_currentOrbitControl != null)
        {
            _currentOrbitControl.enabled = false;
            _currentOrbitControl = null;
        }
        if (_currentTargetCamManager != null)
        {
            _currentTargetCamManager.DisableAllCams();
        }
    }

    private void UpdateSpectatorUI()
    {
        if (spectatorNameUI == null) return;
        if (_currentTargetCamManager == null) return;

        var targetCondition = _currentTargetCamManager.GetComponent<PlayerCondition>();
        if (targetCondition == null) return;

        string status = targetCondition.IsDead ? "(DEAD)" : "";
        spectatorNameUI.text = $"관전 중: {targetCondition.Nickname} {status}";
    }
}