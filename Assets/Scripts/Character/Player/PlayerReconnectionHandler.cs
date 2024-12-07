using System;
using UnityEngine;
using Unity.Netcode;
using Dropt;
public class PlayerReconnectionHandler : NetworkBehaviour
{
    //private PlayerController m_playerController;
    //private ulong m_clientId;

    //private string m_playerId;

    //private SessionPlayerData m_sessionData;

    //private void Awake()
    //{
    //    m_playerController = GetComponent<PlayerController>();
    //}

    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();

    //    if (IsLocalPlayer)
    //    {
    //        // Fetch unique player ID and client ID
    //        m_clientId = NetworkManager.Singleton.LocalClientId;

    //        // Fetch Player ID (e.g., from PlayerPrefs or other persistent storage)
    //        if (PlayerPrefs.HasKey("PlayerId"))
    //        {
    //            m_playerId = PlayerPrefs.GetString("PlayerId");
    //        }
    //        else
    //        {
    //            // Generate and save a new Player ID
    //            m_playerId = Guid.NewGuid().ToString();
    //            PlayerPrefs.SetString("PlayerId", m_playerId);
    //        }

    //        // Check if this is a reconnection or a first-time connection
    //        var sessionManager = SessionManager<SessionPlayerData>.Instance;

    //        if (sessionManager.GetPlayerData(m_playerId).HasValue)
    //        {
    //            // Player ID exists in the SessionManager
    //            var sessionData = sessionManager.GetPlayerData(m_playerId).Value;

    //            if (!sessionData.IsConnected)
    //            {
    //                // Reconnection: Restore the player session
    //                Debug.Log("Reconnecting player...");
    //                RestorePlayerSession();
    //            }
    //            else
    //            {
    //                Debug.LogWarning("Duplicate connection detected. This should not happen under normal conditions.");
    //            }
    //        }
    //        else
    //        {
    //            // First-time connection: Set up new session data
    //            Debug.Log("First-time connection. Setting up session...");
    //            var sessionPlayerData = CreateInitialPlayerData();
    //            sessionManager.SetupConnectingPlayerSessionData(m_clientId, m_playerId, sessionPlayerData);
    //        }
    //    }
    //}

    //private void OnApplicationQuit()
    //{
    //    HandlePlayerExit();
    //}

    //private void OnDisable()
    //{
    //    if (Application.isPlaying && !IsNetworkInterruption())
    //    {
    //        HandlePlayerExit(); // Only clear player data for intentional exits
    //    }
    //}

    //private void HandlePlayerExit()
    //{
    //    if (IsLocalPlayer && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
    //    {
    //        NotifyServerPlayerExitServerRpc(m_playerId, m_clientId);
    //        PlayerPrefs.DeleteKey("PlayerId");
    //        PlayerPrefs.Save();
    //    }
    //}

    //private bool IsNetworkInterruption()
    //{
    //    return !IsServer && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsConnectedClient;
    //}


    //[ServerRpc]
    //private void NotifyServerPlayerExitServerRpc(string playerId, ulong clientId)
    //{
    //    // Clean up session data on the server
    //    var sessionManager = SessionManager<SessionPlayerData>.Instance;

    //    if (sessionManager.GetPlayerData(playerId).HasValue)
    //    {
    //        sessionManager.DisconnectClient(clientId);
    //        Debug.Log($"Player ID {playerId} cleaned up on the server.");
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Attempted to clean up player ID {playerId}, but no session data was found.");
    //    }
    //}

    //private SessionPlayerData CreateInitialPlayerData()
    //{
    //    return new SessionPlayerData
    //    {
    //        IsConnected = true,
    //        ClientID = m_clientId,
    //        SelectedGotchiId = m_playerController.NetworkGotchiId.Value,
    //        TotalKilledEnemies = m_playerController.GetTotalKilledEnemies(),
    //        TotalDestroyedDestructibles = m_playerController.GetTotalDestroyedDestructibles()
    //    };
    //}

    //private void RestorePlayerSession()
    //{
    //    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(m_playerId);

    //    if (sessionData.HasValue)
    //    {
    //        m_sessionData = sessionData.Value;

    //        // Restore player state
    //        Debug.Log("Restoring player session...");
    //        m_playerController.NetworkGotchiId.Value = m_sessionData.SelectedGotchiId;
    //        m_playerController.DestroyDestructible();
    //        RestorePlayerStats();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No session data found for player. Starting fresh.");
    //    }
    //}

    //private void RestorePlayerStats()
    //{
    //    m_playerController.NetworkGotchiId.Value = m_sessionData.SelectedGotchiId;
    //    m_playerController.SetNetworkGotchiIdServerRpc(m_sessionData.SelectedGotchiId);

    //    // Restore other tracked variables
    //    m_playerController.m_totalKilledEnemies.Value = m_sessionData.TotalKilledEnemies;
    //    m_playerController.m_totalDestroyedDestructibles.Value = m_sessionData.TotalDestroyedDestructibles;

    //    Debug.Log($"Restored stats: GotchiId={m_sessionData.SelectedGotchiId}, KilledEnemies={m_sessionData.TotalKilledEnemies}, DestroyedDestructibles={m_sessionData.TotalDestroyedDestructibles}");
    //}

    //public override void OnNetworkDespawn()
    //{
    //    base.OnNetworkDespawn();

    //    if (IsLocalPlayer)
    //    {
    //        SavePlayerSession();
    //    }
    //}

    //private void SavePlayerSession()
    //{
    //    if (!IsLocalPlayer) return;

    //    var sessionData = new SessionPlayerData
    //    {
    //        IsConnected = false,
    //        ClientID = m_clientId,
    //        SelectedGotchiId = m_playerController.NetworkGotchiId.Value,
    //        TotalKilledEnemies = m_playerController.GetTotalKilledEnemies(),
    //        TotalDestroyedDestructibles = m_playerController.GetTotalDestroyedDestructibles()
    //    };

    //    SessionManager<SessionPlayerData>.Instance.SetPlayerData(m_clientId, sessionData);

    //    Debug.Log("Saved player session.");
    //}
}

//[Serializable]
//public struct SessionPlayerData : ISessionPlayerData
//{
//    // Implementing the interface properties correctly
//    public bool IsConnected { get; set; }
//    public ulong ClientID { get; set; }
//    public int SelectedGotchiId { get; set; }
//    public int LocalGotchiId { get; set; }
//    public int TotalKilledEnemies { get; set; }
//    public int TotalDestroyedDestructibles { get; set; }

//    public void Reinitialize()
//    {
//        SelectedGotchiId = 0;
//        LocalGotchiId = 0;
//        TotalKilledEnemies = 0;
//        TotalDestroyedDestructibles = 0;
//        IsConnected = false; // Optionally reset connection status
//    }
//}

