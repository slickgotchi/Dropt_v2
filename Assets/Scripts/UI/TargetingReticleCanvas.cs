using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TargetingReticleCanvas : MonoBehaviour
{
    public static TargetingReticleCanvas Instance { get; private set; }
    public GameObject Reticle;

    private void Awake()
    {
        Instance = this;
    }

    private PlayerController m_playerController;

    private void Update()
    {
        GetPlayerController();


    }

    void GetPlayerController()
    {
        if (m_playerController == null)
        {
            var playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var playerController in playerControllers)
            {
                var networkObject = playerController.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    if (networkObject.IsLocalPlayer)
                    {
                        m_playerController = playerController;
                    }
                }
            }
        }
    }
}
