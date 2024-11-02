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
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    // reconnecting to new game from gaeover
    private bool m_isTryConnectClientOrHostGame = false;

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
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;

            ConnectServerGame();

            // hide loading canvas
            LoadingCanvas.Instance.gameObject.SetActive(false);
        }
        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            m_isTryConnectClientOrHostGame = true;
            //ConnectClientGame();
        }
        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
            ConnectHostGame();
        }

        SetInputSystemEnabled(!Bootstrap.IsServer());
    }

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



    public void Update()
    {
        if (m_isTryConnectClientOrHostGame)
        {
            if (!NetworkManager.Singleton.ShutdownInProgress)
            {
                m_isTryConnectClientOrHostGame = false;
                if (Bootstrap.IsClient())
                {
                    ConnectClientGame();
                }
                else if (Bootstrap.IsHost())
                {
                    ConnectHostGame();
                }
            }
        }
    }

    public async UniTaskVoid ConnectClientGame()
    {
        Debug.Log("ConnectClientGame()");

        m_transport.UseEncryption = (Bootstrap.Instance.UseServerManager || Bootstrap.IsRemoteConnection()) && !Bootstrap.IsHost();

        if (m_transport.UseEncryption)
        {
            // try find an empty game instance to join
            var response = await ServerManagerAgent.Instance.GetEmpty(Bootstrap.GetRegionString());
            Debug.Log("response: " + response);

            // if no valid response, give error and go back to title
            if (response == null)
            {
                ErrorDialogCanvas.Instance.Show("The Dropt server manager is not currently online, please try again later or check our Discord for updates.");
                SceneManager.LoadScene("Title");
                return;
            }

            // check for non-succes
            if (response.responseCode != 200)
            {
                ErrorDialogCanvas.Instance.Show(response.message);
                SceneManager.LoadScene("Title");
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

    //public void TriggerGameOver(ulong playerNetworkObjectId, REKTCanvas.TypeOfREKT typeOfREKT, NetworkManager networkManager)
    //{
    //    var networkObject = networkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
    //    if (networkObject == null) { Debug.LogWarning("Invalid playerNetworkObjectId in TriggerGameOver on server"); return; }

    //    var playerController = networkObject.GetComponent<PlayerController>();
    //    if (playerController == null) { Debug.LogWarning("Invalid playerController in TriggerGameOver on server"); return; }

    //    NetworkManager.Singleton.Shutdown();
    //}

    private void LoadCertificateFiles()
    {
        if (!Bootstrap.IsServer()) return;
        if (!Bootstrap.IsRemoteConnection()) return;

        try
        {
            m_serverCertificate = Environment.GetEnvironmentVariable("SERVER_CERTIFICATE");
            m_serverPrivateKey = Environment.GetEnvironmentVariable("SERVER_PRIVATE_KEY");

            Debug.Log("Certificates loaded successfully.");
            Debug.Log("m_serverCertificate: " + m_serverCertificate);
            Debug.Log("m_serverPrivateKey: " + m_serverPrivateKey);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading certificates: {e.Message}");
        }
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