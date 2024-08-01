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
    public string statusString = "";
    public bool IsConnected = false;

    private string createGameUri = "https://alphaserver.playdropt.io/creategame";
    private string joinGameUri = "https://alphaserver.playdropt.io/joingame";

    public bool m_isTryConnecting = false;
    private float m_retryInterval = 2f;
    private float m_retryTimer = 0f;

    UnityTransport m_transport;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
            CreateGameViaServerManager();
        }
        else
        {
            Connect();
        }

    }

    private void Update()
    {
        m_retryTimer -= Time.deltaTime;

        if (m_isTryConnecting && m_retryTimer <= 0)
        {
            if (StartServerClientOrHost())
            {
                IsConnected = true;
                m_isTryConnecting = false;
            } else
            {
                m_retryTimer = m_retryInterval;
            }
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
            statusString = "Client connection to server succeeded";
        }

        return success;
    }

    public async void CreateGameViaServerManager()
    {
        // create a new game
        using (UnityWebRequest webRequest = UnityWebRequest.Get(createGameUri))
        {
            await webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("ConnectionError: Is the ServerManager running and has the correct uri been used?");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log("DataProcessingError: ");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log("ProtocolError");
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(webRequest.result);
                    CreateGameResponseData data = JsonUtility.FromJson<CreateGameResponseData>(webRequest.downloadHandler.text);

                    // using data configure bootstrap
                    Bootstrap.Instance.IpAddress = data.ipAddress;
                    Bootstrap.Instance.Port = data.port;

                    statusString = "Server manager allocated gameId: " + data.gameId + ", booting up server instance...";

                    // we can now connect direct
                    Connect();

                    break;
            }
        }
    }

    public async void JoinGameViaServerManager(string gameId)
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

                    statusString = "Server manager allocated gameId: " + data.gameId + ", booting up server instance...";

                    // We can now connect direct
                    Connect();

                    break;
            }
        }
    }


    void Connect()
    {
        // set encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";
        m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");

        // if using encryption, set secrets
        if (m_transport.UseEncryption)
        {
            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                Debug.Log("Set Server Secrets");
                m_transport.SetServerSecrets(SecureParameters.ServerCertificate, SecureParameters.ServerPrivateKey);
            }

            if (Bootstrap.IsClient() || Bootstrap.IsHost())
            {
                m_transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.ClientCA);
            }
        }


        // start up
        m_isTryConnecting = true;
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
