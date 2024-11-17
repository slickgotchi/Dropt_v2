using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Netcode.Transports.WebSocket;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    UnityTransport m_unityTransport = null;
    WebSocketTransport m_webSocketTransport = null;

    // certs files for unity transport connections
    private string m_certPem;
    private string m_privkeyPem;
    private string m_chainPem;
    private string m_commonName;

    // reconnecting to new game from gaeover
    private bool m_isTryConnectClientGame = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // check for web socket
        m_webSocketTransport = NetworkManager.Singleton.GetComponent<WebSocketTransport>();
        m_unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (m_webSocketTransport != null || m_unityTransport != null)
        {
            if (Bootstrap.IsServer())
            {
                // set a reasonably high target frame rate to reduce latency
                Application.targetFrameRate = Bootstrap.IsRemoteConnection() ? 1200 : 60;

                // connect server (call StartServer on NetworkManager)
                ConnectServerGame();

                // hide loading canvas
                LoadingCanvas.Instance.gameObject.SetActive(false);
            }
            else if (Bootstrap.IsClient())
            {
                QualitySettings.vSyncCount = 1;

                // connect to a client game (leave gameId param "" to signify we want an empty game)
                ConnectClientGame();
            }
            else if (Bootstrap.IsHost())
            {
                QualitySettings.vSyncCount = 1;

                // connect to host
                ConnectHostGame();
            }

            SetInputSystemEnabled(!Bootstrap.IsServer());

        }
        else
        {
            Debug.LogError("No transport available");
        }

    }

    public void Update()
    {
        if (m_isTryConnectClientGame && !NetworkManager.Singleton.ShutdownInProgress)
        {
            m_isTryConnectClientGame = false;
            Connect();
        }
    }

    private void ConnectServerGame()
    {
        bool isSecure = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        // WebSocketTransport
        if (m_webSocketTransport != null)
        {
            m_webSocketTransport.SecureConnection = isSecure;
        }

        // UnityTransport
        else if (m_unityTransport != null)
        {
            m_unityTransport.UseEncryption = isSecure;

            if (isSecure)
            {
                m_certPem = File.ReadAllText("/usr/local/unity_server/cert.pem");
                m_privkeyPem = File.ReadAllText("/usr/local/unity_server/privkey.pem");

                m_unityTransport.SetServerSecrets(m_certPem, m_privkeyPem);
            }

            m_unityTransport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort, "0.0.0.0");
        }

        // output the ip and gaem port we're using
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        Debug.Log(m_certPem);
        Debug.Log(m_privkeyPem);

        // set connection data and start server
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }

    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        bool isGetEmptyGame = string.IsNullOrEmpty(gameId);
        bool isSecure = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        if (m_webSocketTransport == null && m_unityTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("Transports not available.");
            if (isGetEmptyGame) SceneManager.LoadScene("Title");
            return;
        }

        // WebSocketTransport
        if (m_webSocketTransport != null)
        {
            m_webSocketTransport.SecureConnection = isSecure;

            if (isSecure)
            {
                // try find an empty game instance to join
                Debug.Log("Get game with id: " + gameId + " and region: " + Bootstrap.GetRegionString());
                var response = await ServerManagerAgent.Instance.GetGame(gameId, Bootstrap.GetRegionString());
                Debug.Log("response: " + response);

                // if no valid response, give error and go back to title
                if (response == null)
                {
                    ErrorDialogCanvas.Instance.Show("The Dropt server manager is not currently online, please try again later or check our Discord for updates.");
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return;
                }

                // check for non-succes
                if (response.responseCode != 200)
                {
                    ErrorDialogCanvas.Instance.Show(response.message);
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return;
                }

                // set IP address and port
                Bootstrap.Instance.IpAddress = response.ipAddress;
                Bootstrap.Instance.GamePort = ushort.Parse(response.gamePort);

                m_webSocketTransport.ConnectAddress = response.commonName;
            }
        }

        // UnityTransport
        if (m_unityTransport != null)
        {
            m_unityTransport.UseEncryption = isSecure;

            if (isSecure)
            {
                // try find an empty game instance to join
                Debug.Log("Get game with id: " + gameId + " and region: " + Bootstrap.GetRegionString());
                var response = await ServerManagerAgent.Instance.GetGame(gameId, Bootstrap.GetRegionString());
                Debug.Log("response: " + response);

                // if no valid response, give error and go back to title
                if (response == null)
                {
                    ErrorDialogCanvas.Instance.Show("The Dropt server manager is not currently online, please try again later or check our Discord for updates.");
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return;
                }

                // check for non-succes
                if (response.responseCode != 200)
                {
                    ErrorDialogCanvas.Instance.Show(response.message);
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return;
                }

                // set IP address and port
                Bootstrap.Instance.IpAddress = response.ipAddress;
                Bootstrap.Instance.GamePort = ushort.Parse(response.gamePort);
                m_chainPem = response.clientCA;
                m_commonName = response.commonName;

                m_unityTransport.SetClientSecrets(m_commonName, m_chainPem);
                m_unityTransport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort);
                Debug.Log(m_commonName);
                Debug.Log(m_chainPem);
            }
        }

        if (!isGetEmptyGame)
        {
            NetworkManager.Singleton.Shutdown();
        }

        m_isTryConnectClientGame = true;
    }

    public void Connect()
    {
        // output ip and port
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        Debug.Log("StartClient()");

        // start client
        var success = NetworkManager.Singleton.StartClient();
        if (!success)
        {
            ErrorDialogCanvas.Instance.Show("NetworkManager.Singleton.Start() client failed.");
            SceneManager.LoadScene("Title");
            return;
        }
    }

    public void ConnectHostGame()
    {
        Debug.Log("ConnectHostGame()");

        if (m_webSocketTransport == null && m_unityTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            SceneManager.LoadScene("Title");
            return;
        }

        // WebSocketTranpsort
        if (m_webSocketTransport != null)
        {
            m_webSocketTransport.SecureConnection = false;
            m_webSocketTransport.ConnectAddress = "127.0.0.1";
            m_webSocketTransport.Port = 9000;
        }

        // UnityTransport
        if (m_unityTransport != null)
        {
            m_unityTransport.UseEncryption = false;
            m_unityTransport.SetConnectionData("127.0.0.1", 9000);
        }

        // Additional logic for Host, if needed
        Bootstrap.Instance.IpAddress = "127.0.0.1";
        Bootstrap.Instance.GamePort = 9000;
        NetworkManager.Singleton.StartHost();
    }

    

    private void SetInputSystemEnabled(bool isEnabled)
    {
        var inputModules = FindObjectsOfType<InputSystemUIInputModule>();
        foreach (var inputModule in inputModules)
        {
            inputModule.enabled = isEnabled;
        }

        var playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (var playerInput in playerInputs)
        {
            playerInput.enabled = isEnabled;
        }
    }
}