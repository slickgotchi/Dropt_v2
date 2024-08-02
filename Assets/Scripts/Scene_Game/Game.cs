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
    //public string statusString = "";
    public bool IsConnected = false;

    private string createGameUri = "https://alphaserver.playdropt.io/creategame";
    private string joinGameUri = "https://alphaserver.playdropt.io/joingame";

    public bool m_isRetryConnect = false;
    private float m_retryInterval = 2f;
    private float m_retryConnectTimer = 0f;

    private bool m_isNetworkManagerShuttingDown = false;
    private string m_tryJoinGameId = "";

    private bool m_isRetryCreateGame = false;
    private float m_retryCreateGameTimer = 0f;

    UnityTransport m_transport;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_isRetryConnect = false;
        m_isNetworkManagerShuttingDown = false;

        // 0. ensure we have access to UnityTransport
        m_transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;

        if (m_transport == null)
        {
            Debug.Log("Could not get UnityTransport");
            return;
        }

        // set max framerate if server
        if (Bootstrap.IsServer())
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
        }

        // 1. connect to a server manager if we are in that mode
        if (Bootstrap.Instance.UseServerManager && Bootstrap.IsClient())
        {
            //statusString = "Using remote server manager and IsClient";
            ProgressBarCanvas.Instance.Show("Engaging remote server manager...", 0.1f);

            // create game via server manager
            CreateGameViaServerManager();
        }
        else
        {
            Connect();
        }
    }


    private void Update()
    {
        // poll for creating game
        m_retryCreateGameTimer -= Time.deltaTime;
        if (m_isRetryCreateGame && m_retryCreateGameTimer <= 0)
        {
            m_isRetryCreateGame = false;
            CreateGameViaServerManager();
        }

        // poll for connections
        m_retryConnectTimer -= Time.deltaTime;
        if (m_isRetryConnect && m_retryConnectTimer <= 0)
        {
            if (StartServerClientOrHost())
            {
                IsConnected = true;
                m_isRetryConnect = false;
            } else
            {
                m_retryConnectTimer = m_retryInterval;
            }
        }

        // poll for joining new game instance
        if (!NetworkManager.Singleton.ShutdownInProgress && m_isNetworkManagerShuttingDown)
        {
            Debug.Log("Game.cs: Server shutdown, joining new gameID: " + m_tryJoinGameId);
            m_isNetworkManagerShuttingDown = false;
            //Game.Instance.JoinGameViaServerManager(m_tryJoinGameId);
            Connect();
        }
    }

    bool StartServerClientOrHost()
    {
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

            Bootstrap.Instance.GameId = m_tryJoinGameId;

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
                        Debug.Log("ConnectionError: Is the ServerManager running and has the correct uri been used?");
                        //statusString = "UnityWebRequest.Result.ConnectionError";
                        ProgressBarCanvas.Instance.Show("UnityWebRequest.Result.ConnectionError", 1f);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.Log("DataProcessingError: ");
                        //statusString = "UnityWebRequest.Result.DataProcessingError";
                        ProgressBarCanvas.Instance.Show("UnityWebRequest.Result.DataProcessingError", 1f);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.Log("ProtocolError");
                        //statusString = "UnityWebRequest.Result.ProtocolError";
                        ProgressBarCanvas.Instance.Show("UnityWebRequest.Result.ProtocolError", 1f);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(webRequest.result);
                        CreateGameResponseData data = JsonUtility.FromJson<CreateGameResponseData>(webRequest.downloadHandler.text);

                        // using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        Debug.Log("Got back server instance with port: " + data.port + ", gameId: " + data.gameId +
                            ", ipAddress: " + data.ipAddress);

                        //statusString = "Server manager allocated gameId: " + data.gameId + ", port: " + data.port + ", ipAddress: " + data.ipAddress + ", connecting...";
                        ProgressBarCanvas.Instance.Show("Allocated gameId: " + data.gameId + "on port: " + data.port + ", connecting...", 0.3f);

                        m_tryJoinGameId = data.gameId;

                        // we can now connect direct
                        Connect();

                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception occurred: " + e.Message);
            ErrorDialogCanvas.Instance.Show(e.Message);
            m_isRetryCreateGame = true;
            m_retryCreateGameTimer = m_retryInterval;
        }
    }

    public void TryJoinGame(string gameId)
    {
        m_tryJoinGameId = gameId;
        JoinGameViaServerManager(m_tryJoinGameId);
        //m_isNetworkManagerShuttingDown = true;
        //NetworkManager.Singleton.Shutdown();
    }

    public async void JoinGameViaServerManager(string gameId)
    {
        try
        {
            var postData = new JoinGamePostData
            {
                gameId = gameId,
            };

            string json = JsonUtility.ToJson(postData);

            // Create a new UnityWebRequest
            Debug.Log("Game: Sending POST request to " + joinGameUri + " with json data: " + json);
            using (UnityWebRequest webRequest = new UnityWebRequest(joinGameUri, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                await webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("DataProcessingError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log("Response: " + webRequest.downloadHandler.text);
                        CreateGameResponseData data = JsonUtility.FromJson<CreateGameResponseData>(webRequest.downloadHandler.text);

                        // Using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // shut down our existing server
                        m_isNetworkManagerShuttingDown = true;
                        NetworkManager.Singleton.Shutdown();

                        //statusString = "Server manager allocated gameId: " + data.gameId + ", booting up server instance...";

                        // We can now connect direct
                        //Connect();

                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            ErrorDialogCanvas.Instance.Show(ex.Message);
        }
    }


    void Connect()
    {
        // set encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";
        m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");

        //statusString = "Connection data sent to ipAddress: " + ipAddress + ", port: " + Bootstrap.Instance.Port + ", try connecting...";
        ProgressBarCanvas.Instance.Show("Connection data set, awaiting final server setup...", 0.4f);

        // if using encryption, set secrets
        if (m_transport.UseEncryption)
        {
            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                m_transport.SetServerSecrets(SecureParameters.ServerCertificate, SecureParameters.ServerPrivateKey);
            }

            if (Bootstrap.IsClient() || Bootstrap.IsHost())
            {
                m_transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.ClientCA);
            }
        }


        // start up
        m_isRetryConnect = true;
    }


    [System.Serializable]
    public struct CreateGameResponseData
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
