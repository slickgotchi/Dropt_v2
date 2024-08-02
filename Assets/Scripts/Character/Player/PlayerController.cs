using CarlosLab.UtilityIntelligence;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// PlayerController handles:
// - camera/virtual camera
// - audio listener
// - level spawning

public class PlayerController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] AudioListener playerAudioListener;

    [SerializeField] TextMeshProUGUI m_positionText;

    private bool m_isPlayerHUDInitialized = false;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer) {
            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;
        } else
        {
            playerAudioListener.enabled = false;
            playerCamera.Priority = 0;
        }

        if (IsServer && !IsHost)
        {
            //PingServerRpc(Time.time);
        }

    }


    public void StartTransition()
    {
        if (!IsServer) return;
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            if (m_positionText != null)
            {
                // update position text
                var pos = transform.position;
                m_positionText.text = $"({pos.x.ToString("F2")}, {pos.y.ToString("F2")})";
            }

            HandleLevelManagerState();

            // setup player hud
            if (!m_isPlayerHUDInitialized && GetComponent<NetworkCharacter>() != null)
            {
                PlayerHUDCanvas.Singleton.SetLocalPlayerCharacter(GetComponent<NetworkCharacter>());
            }
        }
    }

    private LevelManager.TransitionState m_localTransition = LevelManager.TransitionState.Null;

    void HandleLevelManagerState()
    {
        if (!IsLocalPlayer) return;

        LevelManager.TransitionState state = LevelManager.Instance.State.Value;

        if (state == LevelManager.TransitionState.Start ||
            state == LevelManager.TransitionState.ClientHeadsUp || 
            state == LevelManager.TransitionState.GoToNext)
        {
            if (m_localTransition != LevelManager.TransitionState.ClientHeadsUp)
            {
                m_localTransition = LevelManager.TransitionState.ClientHeadsUp;
                LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeIn");
            }
        }

        if (state == LevelManager.TransitionState.ClientHeadsDown ||
            state == LevelManager.TransitionState.End ||
            state == LevelManager.TransitionState.Null)
        {
            if (m_localTransition == LevelManager.TransitionState.ClientHeadsUp)
            {
                m_localTransition = LevelManager.TransitionState.Null;
                LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeOut");
                REKTCanvas.Instance.Container.SetActive(false);
            }
        }
    }
}
