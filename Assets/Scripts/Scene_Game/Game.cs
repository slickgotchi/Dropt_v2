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
        if (m_webSocketTransport != null)
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
        }

        SetInputSystemEnabled(!Bootstrap.IsServer());
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
        if (m_webSocketTransport == null) return;
        m_webSocketTransport.SecureConnection = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        // output the ip and gaem port we're using
        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.GamePort);

        // set connection data and start server
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }

    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        bool isGetEmptyGame = string.IsNullOrEmpty(gameId);

        if (m_webSocketTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            if (isGetEmptyGame) SceneManager.LoadScene("Title");
            return;
        }

        m_webSocketTransport.SecureConnection = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        // REMOTE
        if (Bootstrap.IsRemoteConnection())
        {
            if (m_webSocketTransport.SecureConnection)
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


            // if this was a join request we shut down the NetworkManager
            if (!isGetEmptyGame)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        m_isTryConnectClientGame = true;

    }

    public void Connect()
    {
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

    public void ConnectHostGame()
    {
        Debug.Log("ConnectHostGame()");

        var webSocketTransport = NetworkManager.Singleton.GetComponent<WebSocketTransport>();
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




/*
    public async UniTask<Boolean> TryConnectClientGame(string gameId = "")
    {
        if (m_webSocketTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            SceneManager.LoadScene("Title");
            return false;
        }

        // REMOTE connections
        if (Bootstrap.IsRemoteConnection())
        {
            bool isGetEmptyGame = string.IsNullOrEmpty(gameId);

            m_webSocketTransport.SecureConnection = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

            if (m_webSocketTransport.SecureConnection)
            {
                // try find an empty game instance to join
                Debug.Log("Try get game with id: " + gameId + " and region: " + Bootstrap.GetRegionString());
                var response = await ServerManagerAgent.Instance.GetGame(gameId, Bootstrap.GetRegionString());
                Debug.Log("response: " + response);

                // if no valid response, give error and go back to title
                if (response == null)
                {
                    ErrorDialogCanvas.Instance.Show("The Dropt server manager is not currently online, please try again later or check our Discord for updates.");
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return false;
                }

                // check for non-succes
                if (response.responseCode != 200)
                {
                    ErrorDialogCanvas.Instance.Show(response.message);
                    if (isGetEmptyGame) SceneManager.LoadScene("Title");
                    return false;
                }

                // set IP address and port
                Bootstrap.Instance.IpAddress = response.ipAddress;
                Bootstrap.Instance.GamePort = ushort.Parse(response.gamePort);

                m_webSocketTransport.ConnectAddress = response.commonName;
            }
        }

        return true;
    }
    */