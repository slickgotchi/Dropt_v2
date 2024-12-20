using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class PlayerConnectionWarning : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas connectionLostCanvas;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI countdownText;

    private float disconnectTimeout; // The timeout duration set in UnityTransport
    private float countdownTimer;
    private bool isConnectionLost = false;

    private void Start()
    {
        // Initialize UI and timeout settings
        if (!IsLocalPlayer) return;

        connectionLostCanvas.gameObject.SetActive(false);

        // Retrieve the timeout value from UnityTransport
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport != null)
        {
            disconnectTimeout = transport.DisconnectTimeoutMS / 1000f; // Convert ms to seconds
        }
        else
        {
            Debug.LogWarning("UnityTransport not found. Using default timeout of 30 seconds.");
            disconnectTimeout = 30f; // Fallback default
        }

        countdownTimer = disconnectTimeout;
    }

    private void Update()
    {
        if (!IsLocalPlayer || !isConnectionLost) return;

        HandleCountdown();
        UpdateUI();
    }

    public void NotifyConnectionLost()
    {
        if (!IsLocalPlayer) return;

        isConnectionLost = true;
        countdownTimer = disconnectTimeout;
        connectionLostCanvas.gameObject.SetActive(true);
    }

    public void NotifyConnectionRestored()
    {
        if (!IsLocalPlayer) return;

        isConnectionLost = false;
        connectionLostCanvas.gameObject.SetActive(false);
        ResetCountdown();
    }

    private void ResetCountdown()
    {
        countdownTimer = disconnectTimeout;
    }

    private void HandleCountdown()
    {
        if (countdownTimer <= 0f)
        {
            DisconnectPlayer();
            return;
        }

        countdownTimer -= Time.deltaTime;
    }

    private void UpdateUI()
    {
        messageText.text = "Connection Lost!";
        countdownText.text = $"Disconnecting in: {Mathf.CeilToInt(countdownTimer)} seconds...";
    }

    private void DisconnectPlayer()
    {
        Debug.Log("Player has been disconnected due to timeout.");

        if (IsServer)
        {
            NetworkManager.Singleton.DisconnectClient(OwnerClientId);
        }
        else
        {
            Debug.Log("Shutdown NetworkManager");
            NetworkManager.Singleton.Shutdown();
        }
    }
}
