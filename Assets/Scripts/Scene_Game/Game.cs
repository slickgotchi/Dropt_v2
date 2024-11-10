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

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    UnityTransport m_transport;

    // certificate variables for WSS and encryption
    private string m_commonName = "web.playdropt.io";
    //private string m_clientCA;
    //private string m_serverCertificate;
    //private string m_serverPrivateKey;

    // reconnecting to new game from gaeover
    private bool m_isTryConnectClientOrHostGame = false;
    private string m_tryConnectGameId = "";

    private void Awake()
    {
        Instance = this;
    }

    public void TryConnectClientOrHostGame()
    {
        if (m_isTryConnectClientOrHostGame) return;

        m_isTryConnectClientOrHostGame = true;
    }

    private void Start()
    {
        // check for web socket
        var webSocketTransport = NetworkManager.Singleton.GetComponent<Netcode.Transports.WebSocket.WebSocketTransport>();
        if (webSocketTransport != null)
        {
            if (Bootstrap.IsServer())
            {
                // set a reasonably high target frame rate to reduce latency
                Application.targetFrameRate = 1200;

                // connect server (call StartServer on NetworkManager)
                ConnectServerGame(webSocketTransport);

                // hide loading canvas
                LoadingCanvas.Instance.gameObject.SetActive(false);
            }
            else if (Bootstrap.IsClient())
            {
                m_isTryConnectClientOrHostGame = true;
                m_tryConnectGameId = "";
            }
            else if (Bootstrap.IsHost())
            {
                ConnectHostGame();
            }

            return;
        }




        /*
        // 1. Load certificate files
        LoadCertificateFiles();

        // 2. Ensure we have access to UnityTransport
        m_transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;
        if (m_transport == null)
        {
            Debug.Log("Could not get UnityTransport");
            return;
        }

        // 3. Server instances
        if (Bootstrap.IsServer())
        {
            // we limit frame rate for server because when deployed it will go as
            // high as it possible can and consume ALL of our remote servers resources.
            // NOTE: setting this value low (e.g. 15fps) will increase the ping and RTT
            // of our game. 60fps keeps it fairly low at 50-60ms ping on a local connection
            Application.targetFrameRate = 20;
            QualitySettings.vSyncCount = 0;

            ConnectServerGame();

            // hide loading canvas
            LoadingCanvas.Instance.gameObject.SetActive(false);
        }
        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            m_isTryConnectClientOrHostGame = true;
            m_tryConnectGameId = "";
        }
        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
            ConnectHostGame();
        }
        */

        SetInputSystemEnabled(!Bootstrap.IsServer());
    }

    private void ConnectServerGame(Netcode.Transports.WebSocket.WebSocketTransport webSocketTransport)
    {
        webSocketTransport.SecureConnection = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        // output the ip and gaem port we're using
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        // set connection data and start server
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }

    /*
    private void ConnectServerGame()
    {
        Debug.Log("ConnectServerGame()");

        m_transport.UseEncryption = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        if (m_transport.UseEncryption)
        {
            // set connection data
            if (m_transport.UseEncryption)
            {
                Debug.Log(m_serverCertificate);
                Debug.Log(m_serverPrivateKey);
                m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
            }
        }

        // output the ip and gaem port we're using
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        // set connection data and start server
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort, "0.0.0.0");
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }
    */


    public void Update()
    {
        if (m_isTryConnectClientOrHostGame)
        {
            if (!NetworkManager.Singleton.ShutdownInProgress)
            {
                m_isTryConnectClientOrHostGame = false;
                if (Bootstrap.IsClient())
                {
                    ConnectClientGame(m_tryConnectGameId);
                }
                else if (Bootstrap.IsHost())
                {
                    ConnectHostGame();
                }
            }
        }
    }

    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        bool isGetEmptyGame = string.IsNullOrEmpty(gameId);

        var webSocketTransport = NetworkManager.Singleton.GetComponent<Netcode.Transports.WebSocket.WebSocketTransport>();
        if (webSocketTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            SceneManager.LoadScene("Title");
            return;
        }

        webSocketTransport.SecureConnection = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        if (webSocketTransport.SecureConnection)
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
            m_commonName = response.commonName;
            webSocketTransport.ConnectAddress = m_commonName;
            //m_clientCA = response.clientCA;

            Debug.Log("CommonName: " + m_commonName);
            //Debug.Log(m_clientCA);
            //m_transport.SetClientSecrets(m_commonName, m_clientCA);
        }

        // output ip and port
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        // set connection data and start
        //m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort);
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

    /*
    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        bool isGetEmptyGame = string.IsNullOrEmpty(gameId);

        m_transport.UseEncryption = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        if (m_transport.UseEncryption)
        {
            // try find an empty game instance to join
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
            m_commonName = response.commonName;
            m_clientCA = response.clientCA;

            Debug.Log(m_commonName);
            Debug.Log(m_clientCA);
            m_transport.SetClientSecrets(m_commonName, m_clientCA);
        }

        // output ip and port
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        // set connection data and start
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort);
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
    */

    /*
    public void ConnectHostGame()
    {
        Debug.Log("ConnectHostGame()");

        m_transport.UseEncryption = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        // Additional logic for Host, if needed
        Bootstrap.Instance.IpAddress = "127.0.0.1";
        Bootstrap.Instance.GamePort = 9000;
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort, "0.0.0.0");
        NetworkManager.Singleton.StartHost();
    }
    */

    public void ConnectHostGame()
    {
        Debug.Log("ConnectHostGame()");

        var webSocketTransport = NetworkManager.Singleton.GetComponent<Netcode.Transports.WebSocket.WebSocketTransport>();
        if (webSocketTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            SceneManager.LoadScene("Title");
            return;
        }

        webSocketTransport.SecureConnection = false;
        webSocketTransport.ConnectAddress = "127.0.0.1";
        webSocketTransport.Port = 9000;

        // Additional logic for Host, if needed
        Bootstrap.Instance.IpAddress = "127.0.0.1";
        Bootstrap.Instance.GamePort = 9000;
        NetworkManager.Singleton.StartHost();
    }

    public async UniTaskVoid TryJoinGame(string gameId)
    {
        // first check existing is legit


        NetworkManager.Singleton.Shutdown();
        m_isTryConnectClientOrHostGame = true;
        m_tryConnectGameId = gameId;
    }

    //private void LoadCertificateFiles()
    //{
    //    if (!Bootstrap.IsServer()) return;
    //    if (!Bootstrap.IsRemoteConnection()) return;

    //    try
    //    {
    //        m_serverCertificate = Environment.GetEnvironmentVariable("SERVER_CERTIFICATE");
    //        m_serverPrivateKey = Environment.GetEnvironmentVariable("SERVER_PRIVATE_KEY");

    //        Debug.Log("Certificates loaded successfully.");
    //        Debug.Log("m_serverCertificate: " + m_serverCertificate);
    //        Debug.Log("m_serverPrivateKey: " + m_serverPrivateKey);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"Error loading certificates: {e.Message}");
    //    }
    //}

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