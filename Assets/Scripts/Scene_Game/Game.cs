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

    UnityTransport m_unityTransport = null;

    // certs files for unity transport connections
    private string m_certPem;
    private string m_privkeyPem;
    private string m_chainPem;
    private string m_commonName;

    // reconnecting to new game from gaeover
    private bool m_isTryConnectClientGame = false;

    // store current game Id
    private string m_currentGameId = "";

    public bool isReconnecting = true;

    public float reconnectTimer = 30f;
    public bool isReconnectTimerActive = false;



    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_currentGameId = "";

        // check for web socket
        m_unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (m_unityTransport != null)
        {

            if (Bootstrap.IsServer())
            {
                // set a reasonably high target frame rate to reduce latency
                Application.targetFrameRate = Bootstrap.IsRemoteConnection() ? 1200 : 60;
                QualitySettings.vSyncCount = 0;

                // connect server (call StartServer on NetworkManager)
                ConnectServerGame();

                // enable profiling
                if (Bootstrap.IsRemoteConnection())
                {
                    UnityEngine.Profiling.Profiler.enabled = true;
                }

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

        if (Bootstrap.IsServer())
        {
            GetObjectCounts();

            if (isReconnectTimerActive)
            {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer < 0) isReconnectTimerActive = false;
            }
        }

    }

    private float m_noTimer = 0f;

    void GetObjectCounts()
    {
        m_noTimer -= Time.deltaTime;

        if (m_noTimer < 0)
        {
            m_noTimer = 1f;

            var networkObjects = FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var gameObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"GameObjects: {gameObjects.Length}, Network Objects: {networkObjects.Length}");
        }
    }

    private void ConnectServerGame()
    {
        if (m_unityTransport == null)
        {
            Debug.LogError("Unity Transport does not exist or is misconfigured!");
            return;
        }

        // do encryption
        if (m_unityTransport.UseEncryption)
        {
            m_certPem = File.ReadAllText("/usr/local/unity_server/cert.pem");
            m_privkeyPem = File.ReadAllText("/usr/local/unity_server/privkey.pem");

            m_unityTransport.SetServerSecrets(m_certPem, m_privkeyPem);
        }

        // set connection data
        m_unityTransport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort, "0.0.0.0");

        // output the ip and gaem port we're using
        Debug.Log($"Connect to IP: {Bootstrap.Instance.IpAddress}, Port: {Bootstrap.Instance.GamePort}");

        Debug.Log("cert.pem");
        Debug.Log(m_certPem);
        Debug.Log("privkey.pem");
        Debug.Log(m_privkeyPem);

        // set connection data and start server
        NetworkManager.Singleton.StartServer();
        Debug.Log("StartServer()");
    }

    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        bool isGetEmptyGame = string.IsNullOrEmpty(gameId);

        if (m_unityTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("Transports not available.");
            if (isGetEmptyGame) SceneManager.LoadScene("Title");
            return;
        }

        // UnityTransport
        if (m_unityTransport != null)
        {
            if (Bootstrap.IsUseServerManager())
            {
                Debug.Log("Managed Remote Client - Use ServerManager to locate a game");
                // try find an empty game instance to join
                Debug.Log("Get game with id: " + gameId + " and region: " + Bootstrap.GetRegionString());
                var response = await ServerManagerAgent.Instance.GetGame(gameId, Bootstrap.GetRegionString());

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
            }
            else
            {
                // REMOTE CONNECTION
                if (Bootstrap.IsRemoteConnection())
                {
                    Debug.Log("Direct Remote Client - Establish direct remote connection");
                    m_chainPem = m_testGameServerChainPem;
                    m_commonName = m_testGameServerCommonName;
                }
                // LOCAL CONNECTION
                else
                {
                    Debug.Log("Local Client - Establish local environment connection");
                    Bootstrap.Instance.IpAddress = "127.0.0.1";
                    Bootstrap.Instance.GamePort = 9000;
                }
            }

            // set secrets for encrypted connections
            if (m_unityTransport.UseEncryption)
            {
                m_unityTransport.SetClientSecrets(m_commonName, m_chainPem);

            }

            // set connection data
            m_unityTransport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.GamePort);

            // output some info
            Debug.Log(Bootstrap.Instance.IpAddress);
            Debug.Log(Bootstrap.Instance.GamePort);
            Debug.Log(m_commonName);
            Debug.Log(m_chainPem);
        }

        Debug.Log($"Connect to IP: {Bootstrap.Instance.IpAddress}, Port: {Bootstrap.Instance.GamePort}");

        //if (!isGetEmptyGame)
        {
            NetworkManager.Singleton.Shutdown();
        }

        m_isTryConnectClientGame = true;
        m_currentGameId = gameId;
    }


    public void Connect()
    {
        // start client
        var success = NetworkManager.Singleton.StartClient();
        if (!success)
        {
            ErrorDialogCanvas.Instance.Show("NetworkManager.Singleton.Start() client failed.");
            SceneManager.LoadScene("Title");
            return;
        }
        Debug.Log("StartClient()");
    }

    public void ReconnectClientGame()
    {
        if (!Bootstrap.IsClient()) return;

        Debug.Log("ReconnectClientGame");
        ConnectClientGame(m_currentGameId);
    }

    public void StartClientReconnectionTimer()
    {
        if (!Bootstrap.IsServer()) return;

        isReconnectTimerActive = true;
        reconnectTimer = 30f;
    }

    public bool IsClientReconnecting()
    {
        return isReconnectTimerActive && reconnectTimer > 0;
    }

    public void ConnectHostGame()
    {
        Debug.Log("ConnectHostGame()");

        if (m_unityTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("WebSocketTransport not available.");
            SceneManager.LoadScene("Title");
            return;
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

    private string m_testGameServerCommonName = "worker-0001.playdropt.io";

    private string m_testGameServerChainPem = @"
-----BEGIN CERTIFICATE-----
MIIFBjCCAu6gAwIBAgIRAIp9PhPWLzDvI4a9KQdrNPgwDQYJKoZIhvcNAQELBQAw
TzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2Vh
cmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMjQwMzEzMDAwMDAw
WhcNMjcwMzEyMjM1OTU5WjAzMQswCQYDVQQGEwJVUzEWMBQGA1UEChMNTGV0J3Mg
RW5jcnlwdDEMMAoGA1UEAxMDUjExMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIB
CgKCAQEAuoe8XBsAOcvKCs3UZxD5ATylTqVhyybKUvsVAbe5KPUoHu0nsyQYOWcJ
DAjs4DqwO3cOvfPlOVRBDE6uQdaZdN5R2+97/1i9qLcT9t4x1fJyyXJqC4N0lZxG
AGQUmfOx2SLZzaiSqhwmej/+71gFewiVgdtxD4774zEJuwm+UE1fj5F2PVqdnoPy
6cRms+EGZkNIGIBloDcYmpuEMpexsr3E+BUAnSeI++JjF5ZsmydnS8TbKF5pwnnw
SVzgJFDhxLyhBax7QG0AtMJBP6dYuC/FXJuluwme8f7rsIU5/agK70XEeOtlKsLP
Xzze41xNG/cLJyuqC0J3U095ah2H2QIDAQABo4H4MIH1MA4GA1UdDwEB/wQEAwIB
hjAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwEwEgYDVR0TAQH/BAgwBgEB
/wIBADAdBgNVHQ4EFgQUxc9GpOr0w8B6bJXELbBeki8m47kwHwYDVR0jBBgwFoAU
ebRZ5nu25eQBc4AIiMgaWPbpm24wMgYIKwYBBQUHAQEEJjAkMCIGCCsGAQUFBzAC
hhZodHRwOi8veDEuaS5sZW5jci5vcmcvMBMGA1UdIAQMMAowCAYGZ4EMAQIBMCcG
A1UdHwQgMB4wHKAaoBiGFmh0dHA6Ly94MS5jLmxlbmNyLm9yZy8wDQYJKoZIhvcN
AQELBQADggIBAE7iiV0KAxyQOND1H/lxXPjDj7I3iHpvsCUf7b632IYGjukJhM1y
v4Hz/MrPU0jtvfZpQtSlET41yBOykh0FX+ou1Nj4ScOt9ZmWnO8m2OG0JAtIIE38
01S0qcYhyOE2G/93ZCkXufBL713qzXnQv5C/viOykNpKqUgxdKlEC+Hi9i2DcaR1
e9KUwQUZRhy5j/PEdEglKg3l9dtD4tuTm7kZtB8v32oOjzHTYw+7KdzdZiw/sBtn
UfhBPORNuay4pJxmY/WrhSMdzFO2q3Gu3MUBcdo27goYKjL9CTF8j/Zz55yctUoV
aneCWs/ajUX+HypkBTA+c8LGDLnWO2NKq0YD/pnARkAnYGPfUDoHR9gVSp/qRx+Z
WghiDLZsMwhN1zjtSC0uBWiugF3vTNzYIEFfaPG7Ws3jDrAMMYebQ95JQ+HIBD/R
PBuHRTBpqKlyDnkSHDHYPiNX3adPoPAcgdF3H2/W0rmoswMWgTlLn1Wu0mrks7/q
pdWfS6PJ1jty80r2VKsM/Dj3YIDfbjXKdaFU5C+8bhfJGqU3taKauuz0wHVGT3eo
6FlWkWYtbt4pgdamlwVeZEW+LM7qZEJEsMNPrfC03APKmZsJgpWCDWOKZvkZcvjV
uYkQ4omYCTX5ohy+knMjdOmdH9c7SpqEWBDC86fiNex+O0XOMEZSa8DA
-----END CERTIFICATE-----
";
}