using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

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

    // certs
    private string m_serverCommonName;
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    private void Awake()
    {
        Instance = this;
        SetupServerCerts();
    }

    void SetupServerCerts()
    {
        if (Bootstrap.IsServer() && Bootstrap.IsRemoteConnection())
        {
            m_serverCertificate = System.Environment.GetEnvironmentVariable("DROPT_SERVER_CERTIFICATE");
            m_serverPrivateKey = System.Environment.GetEnvironmentVariable("DROPT_SERVER_PRIVATE_KEY");
            Debug.Log("ServerCertificate");
            Debug.Log(m_serverCertificate);
            Debug.Log("ServerPrivateKey");
            Debug.Log(m_serverPrivateKey);
        }
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
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            m_isTryConnect = true;
        }

        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            if (Bootstrap.Instance.UseServerManager)
            {
                ProgressBarCanvas.Instance.Show("Engaging remote server manager...", 0.1f);

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

    bool isCreatedGameOnce = false;

    private void Update()
    {
        // CreateGame polling
        m_tryCreateGameTimer -= Time.deltaTime;
        if (m_isTryCreateGame && m_tryCreateGameTimer <= 0 && !isCreatedGameOnce)
        {
            Debug.Log("TryCreateGame");
            isCreatedGameOnce = true;
            m_isTryCreateGame = false;
            CreateGameViaServerManager();
        }

        // Connect polling
        m_tryConnectTimer -= Time.deltaTime;
        if (m_isTryConnect && m_tryConnectTimer <= 0 && !NetworkManager.Singleton.ShutdownInProgress)
        {
            Debug.Log("TryConnect");
            if (TryStartServerClientOrHost())
            {
                IsConnected = true;
                m_isTryConnect = false;
            } else
            {
                m_tryConnectTimer = k_pollInterval;
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
        m_isTryConnect = false;
        m_isTryCreateGame = true;
    }

    bool TryStartServerClientOrHost()
    {
        // set encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";
        m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");

        ProgressBarCanvas.Instance.Show("Connection data set, awaiting final server setup...", 0.4f);

        // if using encryption, set secrets
        if (m_transport.UseEncryption)
        {
            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
            }

            if (Bootstrap.IsClient() || Bootstrap.IsHost())
            {
                m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
            }
        }

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

            //statusString = "Client connection to server with gameId: " + Bootstrap.Instance.GameId + ", port: " + Bootstrap.Instance.Port + " succeeded";
            ProgressBarCanvas.Instance.Show("Client connected to server with gameId: " + Bootstrap.Instance.GameId + " on port: " + Bootstrap.Instance.Port, 0.5f);
        }

        return success;
    }

    public async void CreateGameViaServerManager()
    {
        //statusString = "Sending web request to: " + createGameUri;
        ProgressBarCanvas.Instance.Show("Requesting game instance... ", 0.2f);

        // create a new game
        try
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(createGameUri))
            {
                await webRequest.SendWebRequest();

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
                        Debug.Log(webRequest.result);
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);

                        // using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client side cert data
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;
                        Debug.Log(m_serverCommonName);
                        Debug.Log(m_clientCA);

                        Debug.Log("Got back server instance with port: " + data.port + ", gameId: " + data.gameId +
                            ", ipAddress: " + data.ipAddress);

                        // update progress bar
                        ProgressBarCanvas.Instance.Show("Allocated gameId: " + data.gameId + "on port: " + data.port + ", connecting...", 0.3f);

                        // save gameid for joins and try connect
                        m_isTryConnect = true;

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
        JoinGameViaServerManager(gameId);
    }

    public async void JoinGameViaServerManager(string gameId)
    {
        try
        {
            // 1. assemble post data into json
            var postData = new JoinGamePostData { gameId = gameId };
            string json = JsonUtility.ToJson(postData);

            // 2. Create a new UnityWebRequest
            Debug.Log("Game: Sending POST request to " + joinGameUri + " with json data: " + json);
            using (UnityWebRequest webRequest = new UnityWebRequest(joinGameUri, "POST"))
            {
                // 3. populate the requests body
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // 4. perform the request
                await webRequest.SendWebRequest();

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
                        Debug.Log("Response: " + webRequest.downloadHandler.text);
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);

                        // Using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client sider cert data
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;

                        // shut down our existing server
                        NetworkManager.Singleton.Shutdown();

                        // We can now connect direct
                        m_isTryConnect = true;

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

    [System.Serializable]
    public struct CreateOrJoinGameResponseData
    {
        public string ipAddress;
        public ushort port;
        public string serverCommonName;
        public string clientCA; // Assuming clientCA should also be included
        public string gameId;
    }

    [System.Serializable]
    struct JoinGamePostData
    {
        public string gameId;
    }
}
