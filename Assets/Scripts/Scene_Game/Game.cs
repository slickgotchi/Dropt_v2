using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class Game : MonoBehaviour
{
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

    private string createGameUri = "https://alphaserver.playdropt.io/creategame";
    private string joinGameUri = "https://alphaserver.playdropt.io/joingame";

    public bool m_isTryConnecting = false;
    private float m_retryInterval = 2f;
    private float m_retryTimer = 0f;

    private void Start()
    {
        // 0. ensure we have access to UnityTransport
        var transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;

        if (transport == null)
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
            ConnectViaServerManager(transport);
        }
        else
        {
            ConnectDirect(transport);
        }

    }

    private void Update()
    {
        m_retryTimer -= Time.deltaTime;

        if (m_isTryConnecting && m_retryTimer <= 0)
        {
            Debug.Log("Try StartServerClientOrHost()");
            if (StartServerClientOrHost())
            {
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
        }

        return success;
    }

    async void ConnectViaServerManager(UnityTransport transport)
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

                    // we can now connect direct
                    ConnectDirect(transport);

                    break;
            }
        }
    }

    void ConnectDirect(UnityTransport transport)
    {
        // set encryption
        transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";
        transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");

        // if using encryption, set secrets
        if (transport.UseEncryption)
        {
            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                Debug.Log("Set Server Secrets");
                transport.SetServerSecrets(SecureParameters.ServerCertificate, SecureParameters.ServerPrivateKey);
            }

            if (Bootstrap.IsClient() || Bootstrap.IsHost())
            {
                transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.ClientCA);
            }
        }


        // start up
        m_isTryConnecting = true;
        //StartServerClientOrHost();
        
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
}
