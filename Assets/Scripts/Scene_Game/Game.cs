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

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    UnityTransport m_transport;

    // certificate variables for WSS and encryption
    private string m_serverCommonName = "web.playdropt.io";
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    private void Awake()
    {
        Instance = this;
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
            Application.targetFrameRate = 15;
            QualitySettings.vSyncCount = 0;

            DisableInputSystem();
            Debug.Log("Disabled input system");

            ConnectServerGame();
        }
        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            if (Bootstrap.Instance.UseServerManager)
            {
                ConnectClientGame();
            }
        }
        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
            // Additional logic for Host, if needed
        }
    }

    private void LoadCertificateFiles()
    {
        try
        {
            // Load the certificates into strings
            if (Bootstrap.IsClient())
            {
                string certPath = Path.Combine(Application.dataPath, "../certs/chain.pem");
                m_clientCA = File.ReadAllText(certPath);

                //m_clientCA = File.ReadAllText("/certs/chain.pem");
            }
            else if (Bootstrap.IsServer())
            {
                string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certs/cert.pem");
                m_serverCertificate = File.ReadAllText(certPath);

                string privkeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certs/privkey.pem");
                m_serverPrivateKey = File.ReadAllText(privkeyPath);

                //m_serverCertificate = File.ReadAllText("/certs/cert.pem");
                //m_serverPrivateKey = File.ReadAllText("/certs/privkey.pem");
            }

            Debug.Log("Certificates loaded successfully.");
            Debug.Log("m_serverCommonName: " + m_serverCommonName);
            Debug.Log("m_clientCA: " + m_clientCA);
            Debug.Log("m_serverCertificate: " + m_serverCertificate);
            Debug.Log("m_serverPrivateKey: " + m_serverPrivateKey);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading certificates: {e.Message}");
        }
    }

    private void ConnectServerGame()
    {
        Debug.Log("ConnectServerGame()");
        // set if using encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.Port);
        Debug.Log(m_serverCertificate);
        Debug.Log(m_serverPrivateKey);

        // set connection data
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.Port, "0.0.0.0");
        if (m_transport.UseEncryption)
        {
            m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
        }

        // start server
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }

    private async void ConnectClientGame()
    {
        Debug.Log("ConnectClientGame()");
        var response = await ServerManagerAgent.Instance.JoinEmpty();

        if (response == null)
        {
            Debug.LogError("Failed to join empty instance.");
            //return;
        }

        // set IP address and port
        //Bootstrap.Instance.IpAddress = response.ipAddress;
        //Bootstrap.Instance.Port = ushort.Parse(response.nodePort);

        Debug.Log(Bootstrap.Instance.IpAddress);
        Debug.Log(Bootstrap.Instance.Port);
        Debug.Log(m_serverCommonName);
        Debug.Log(m_clientCA);

        // set if using encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.Port);
        if (m_transport.UseEncryption)
        {
            m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
        }

        // start client
        NetworkManager.Singleton.StartClient();
        Debug.Log("StartClient()");
    }

    private void Update()
    {
        // Additional update logic if needed
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
            // Logic for creating a game remotely using server manager
        }
        else if (Bootstrap.IsHost())
        {
            LevelManager.Instance.GoToDegenapeVillageLevel();
        }
    }

    private void DisableInputSystem()
    {
        var inputModules = FindObjectsOfType<InputSystemUIInputModule>();
        foreach (var inputModule in inputModules)
        {
            inputModule.enabled = false;
        }

        var playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (var playerInput in playerInputs)
        {
            playerInput.enabled = false;
        }
    }
}
