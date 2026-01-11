using UnityEngine;
using Fusion;

public class PlayerCameraManager : NetworkBehaviour
{
    public GameObject gameplayCam;
    public GameObject viewmodelCam;
    public GameObject deathCam;
    public GameObject observerCam;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            DisableAllCams();
        }
    }


    public void ActivateGameplay()
    {
        DisableAllCams();
        gameplayCam.SetActive(true);
        viewmodelCam.SetActive(true);
    }

    public void ActivateDeathCam()
    {
        DisableAllCams();
        deathCam.SetActive(true);
    }

    public void ActivateObserverCam()
    {
        DisableAllCams();
        observerCam.SetActive(true);
    }

    public void DisableAllCams()
    {
        gameplayCam.SetActive(false);
        viewmodelCam.SetActive(false);
        deathCam.SetActive(false);
        observerCam.SetActive(false);
    }
}