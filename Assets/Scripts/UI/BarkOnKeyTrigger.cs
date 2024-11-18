using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem.Wrappers;
using Unity.Netcode;

public class BarkOnKeyTrigger : MonoBehaviour
{
    [SerializeField] private KeyCode m_keyCode = KeyCode.F;

    private StandardBarkUI m_standardBarkUI;

    private bool m_isVisible = false;

    private void Awake()
    {
    }

    private void Update()
    {
        if (m_standardBarkUI == null)
        {
            m_standardBarkUI = GetComponentInChildren<StandardBarkUI>();

        }

        if (Input.GetKeyDown(m_keyCode))
        {
            if (m_isVisible)
            {
                m_standardBarkUI.waitForContinueButton = true;
            }
            else
            {
                m_standardBarkUI.Hide();
            }

            //GetComponentInChildren<StandardBarkUI>().Hide();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 
        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null) return;

        if (playerNetworkObject.IsLocalPlayer)
        {
            m_isVisible = true;
        }

        

        //// Check if the colliding object has the "CameraFollower" tag
        //if (collider.gameObject.CompareTag("CameraFollower"))
        //{
        //    // If the tag matches, enable the waitForContinueButton property
        //    GetComponentInChildren<StandardBarkUI>().waitForContinueButton = true;
        //}
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null) return;

        if (playerNetworkObject.IsLocalPlayer)
        {
            m_isVisible = false;
        }


        // Check if the colliding object has the "CameraFollower" tag before hiding the UI
        //if (collision.gameObject.CompareTag("CameraFollower"))
        //{
        //    GetComponentInChildren<StandardBarkUI>().Hide();
        //}
    }
}
