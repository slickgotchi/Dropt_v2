using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using Audio.Game;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public enum Status
    {
        NotConnected,
        RequestedGame,
        Connecting,
        ConnectionErrorServerDown,
        ConnectionErrorNoSlots,
        Playing,
        GameOver,
    }
    public Game.Status status;
    public bool IsConnected = false;

    private string createGameUri = "https://alphaserver.playdropt.io/creategame";
    private string joinGameUri = "https://alphaserver.playdropt.io/joingame";

    private float k_pollInterval = 2f;

    // params for connecting
    public bool m_isTryConnect = false;
    private float m_tryConnectTimer = 0f;

    // params for serverManager (nodejs app) connections
    private bool m_isTryCreateGame = false;
    private float m_tryCreateGameTimer = 0f;

    UnityTransport m_transport;

    // certificate variables for wss and encryption
    // IMPORTANT: The only certs that seem to work correctly when deployed in browser are those for the actual website (web.playdropt.io)
    // and the ones that are used to secure the website itself (letsencrypt)
    private string m_serverCommonName;
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    public bool m_isTutorialCompleted = false;
    private const string TutorialCompletedKey = "TutorialCompleted";

    // TempCarryOverData is data used when joining another instance
    //public int LocalGotchiId = 0;

    private void Awake()
    {
        Instance = this;

        NetworkManager.Singleton.OnTransportFailure += HandleTransportFailure;
        NetworkManager.Singleton.OnConnectionEvent += HandleConnectionEvent;
    }

    void HandleTransportFailure()
    {
        Debug.Log("There was a connection failure");
    }

    void HandleConnectionEvent(NetworkManager nm, ConnectionEventData ev)
    {
        Debug.Log("Connection event: " + ev);
    }

    private void Start()
    {
        // 1. start tryConnect and tryCreateGame as false
        m_isTryCreateGame = false;
        m_isTryConnect = false;

        // 2. ensure we have access to UnityTransport
        m_transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;
        if (m_transport == null) { Debug.Log("Could not get UnityTransport"); return; }

        // 3. Server instances
        if (Bootstrap.IsServer())
        {
            Application.targetFrameRate = 15;
            QualitySettings.vSyncCount = 0;
            m_isTryConnect = true;

            if (Bootstrap.IsRemoteConnection() && Bootstrap.IsUseServerManager())
            {
                m_serverCertificate = System.Environment.GetEnvironmentVariable("DROPT_SERVER_CERTIFICATE");
                m_serverPrivateKey = System.Environment.GetEnvironmentVariable("DROPT_SERVER_PRIVATE_KEY");
                m_isTutorialCompleted = System.Environment.GetEnvironmentVariable("IS_TUTORIAL_COMPLETED") == "true";
            }
        }

        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            if (Bootstrap.Instance.UseServerManager)
            {
                ProgressBarCanvas.Instance.ResetProgress();
                ProgressBarCanvas.Instance.Show("Engaging remote server manager...");

                // try create game (via server manager)
                m_isTryCreateGame = true;
            }
            else
            {
                m_isTryConnect = true;
            }
        }

        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
            m_isTryConnect = true;
        }
    }

    bool m_isCreatedGameOnce = false;

    private void Update()
    {
        // CreateGame polling
        m_tryCreateGameTimer -= Time.deltaTime;
        if (m_isTryCreateGame && m_tryCreateGameTimer <= 0 && !m_isCreatedGameOnce)
        {
            m_isCreatedGameOnce = true;
            m_isTryCreateGame = false;
            CreateGameViaServerManager().Forget(); // Use UniTask with Forget to avoid unhandled exceptions
        }

        // Connect polling
        m_tryConnectTimer -= Time.deltaTime;
        if (m_isTryConnect &&
            m_tryConnectTimer <= 0 &&
            !NetworkManager.Singleton.ShutdownInProgress)
        {
            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                if (TryStartServerClientOrHost())
                {
                    IsConnected = true;
                    m_isTryConnect = false;
                    ProgressBarCanvas.Instance.Hide();
                }
            }

            else if (Bootstrap.IsClient())
            {
                if (Bootstrap.IsUseServerManager() && AvailableGamesHeartbeat.Instance.IsServerReady(Bootstrap.Instance.GameId) ||
                    !Bootstrap.IsUseServerManager())
                {
                    if (TryStartServerClientOrHost())
                    {
                        IsConnected = true;
                        m_isTryConnect = false;
                        ProgressBarCanvas.Instance.Show("Client started, loading level...");
                        ProgressBarCanvas.Instance.Hide();
                    }
                }
            }
        }
    }

    public void TriggerGameOver(REKTCanvas.TypeOfREKT typeOfREKT)
    {
        REKTCanvas.Instance.Show(typeOfREKT);
        NetworkManager.Singleton.Shutdown();
    }

    public void TryCreateGame()
    {
        if (Bootstrap.IsRemoteConnection() && Bootstrap.IsUseServerManager())
        {
            m_isTryConnect = false;
            m_isCreatedGameOnce = false;
            m_isTryCreateGame = true;
        }
        else if (Bootstrap.IsHost())
        {
            LevelManager.Instance.GoToDegenapeVillageLevel();
        }
    }

    bool TryStartServerClientOrHost()
    {
        // set encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";

        if (Bootstrap.IsServer() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");
            if (m_transport.UseEncryption)
            {
                m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
            }
        }
        else if (Bootstrap.IsClient() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port);
            if (m_transport.UseEncryption)
            {
                m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
                ProgressBarCanvas.Instance.Show("Client certificate provided to server. Validating...");
            }
        }

        ProgressBarCanvas.Instance.Show("Connection data set, awaiting final server setup...");

        // store a bool for our connection success
        bool success = false;

        // startup network 
        if (Bootstrap.IsHost())
        {
            success = NetworkManager.Singleton.StartHost();
            if (success) Debug.Log("StartHost() succeeded");
        }
        else if (Bootstrap.IsServer())
        {
            success = NetworkManager.Singleton.StartServer();
            if (success) Debug.Log("StartServer() succeeded");
        }
        else if (Bootstrap.IsClient())
        {
            success = NetworkManager.Singleton.StartClient();
            if (success) Debug.Log("StartClient() succeeded");
        }

        return success;
    }

    public UniTask CreateGameViaServerManager()
    {
        return CreateGameViaServerManagerAsync();
    }

    private async UniTask CreateGameViaServerManagerAsync()
    {
        ProgressBarCanvas.Instance.Show("Requesting game server instance... ");

        // create a new game
        try
        {
            // 1. assemble post data into json
            var postData = new CreateGamePostData { isTutorialCompleted = IsTutorialCompleted() };
            string json = JsonUtility.ToJson(postData);

            // 2. Create a new UnityWebRequest
            using (UnityWebRequest webRequest = new UnityWebRequest(createGameUri, "POST"))
            {
                // 3. populate the requests body
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // 4. send web request
                await webRequest.SendWebRequest().ToUniTask();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ConnectionError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.DataProcessingError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ProtocolError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.Success:
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);
                        //Debug.Log("Response Data to CreateGame below...");
                        //Debug.Log(webRequest.downloadHandler.text);

                        // using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client side cert data if using server manager
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;

                        // update progress bar
                        ProgressBarCanvas.Instance.Show("Allocated gameId & port, connecting...");

                        // save gameid for joins and try connect
                        m_isTryConnect = true;

                        webRequest.Dispose();

                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception occurred: " + e.Message);
            ErrorDialogCanvas.Instance.Show(e.Message);

            // try to create the game again
            m_isTryCreateGame = true;
            m_tryCreateGameTimer = k_pollInterval;
        }
    }

    // other gameobjects call this to try join a game
    public void TryJoinGame(string gameId)
    {
        ProgressBarCanvas.Instance.ResetProgress();
        ProgressBarCanvas.Instance.Show("Try joining gameId: " + gameId);
        Debug.Log("Attempting to join: " + gameId);
        JoinGameViaServerManager(gameId).Forget(); // Use UniTask with Forget to avoid unhandled exceptions
    }

    public UniTask JoinGameViaServerManager(string gameId)
    {
        return JoinGameViaServerManagerAsync(gameId);
    }

    private async UniTask JoinGameViaServerManagerAsync(string gameId)
    {
        try
        {
            // 1. assemble post data into json
            var postData = new JoinGamePostData { gameId = gameId };
            string json = JsonUtility.ToJson(postData);

            // 2. Create a new UnityWebRequest
            using (UnityWebRequest webRequest = new UnityWebRequest(joinGameUri, "POST"))
            {
                // 3. populate the requests body
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // 4. perform the request
                await webRequest.SendWebRequest().ToUniTask();

                // 5. process results
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ConnectionError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ProtocolError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.DataProcessingError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);
                        // Debug.Log("Response Data to JoinGame below...");
                        // Debug.Log(webRequest.downloadHandler.text);

                        // Using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client side cert data if using server manager
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;

                        // shut down our existing server
                        // Debug.Log("Received joinGame data, gameId: " + data.gameId + ", port: " + data.port);
                        ProgressBarCanvas.Instance.Show("Received data for new game, switching...");
                        NetworkManager.Singleton.Shutdown();

                        // We can now connect direct
                        m_isTryConnect = true;

                        webRequest.Dispose();

                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);

            // just display error message (player can choose to hit join again once they've checked their params)
            ErrorDialogCanvas.Instance.Show(ex.Message);
        }
    }

    public void CompleteTutorial()
    {
        if (!Bootstrap.IsClient()) return;

        // Mark the tutorial as completed
        SetTutorialCompleted(true);
        Debug.Log("Tutorial marked as completed.");
    }

    public bool IsTutorialCompleted()
    {
        if (Bootstrap.IsServer())
        {
            return m_isTutorialCompleted;
        } else
        {
            // Retrieve the boolean value from PlayerPrefs
            return PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;
        }

    }

    private void SetTutorialCompleted(bool completed)
    {
        if (!Bootstrap.IsClient()) return;

        // Convert the boolean to an integer (1 for true, 0 for false) and store it in PlayerPrefs
        PlayerPrefs.SetInt(TutorialCompletedKey, completed ? 1 : 0);
        PlayerPrefs.Save(); // Ensure the data is saved
    }

    public void ResetTutorialCompletion()
    {
        if (!Bootstrap.IsClient()) return;

        // Optionally, allow resetting the tutorial completion status
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        Debug.Log("Tutorial completion status reset.");
    }

    [System.Serializable]
    public struct CreateOrJoinGameResponseData
    {
        public string ipAddress;
        public ushort port;
        public string serverCommonName;
        public string clientCA;
        public string gameId;
    }

    [System.Serializable]
    struct JoinGamePostData
    {
        public string gameId;
    }

    [System.Serializable]
    struct CreateGamePostData
    {
        public bool isTutorialCompleted;
    }
}