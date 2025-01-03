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
    //private string m_currentGameId = "";

    [HideInInspector] public bool isReconnecting = true;

    public float reconnectTimer = 30f;
    [HideInInspector] public bool isReconnectTimerActive = false;

    public bool isServerReady = false;

    //public List<GameObject> afterConnectSpawnPrefabs_SERVER = new List<GameObject>();

    [HideInInspector] public List<PlayerController> playerControllers = new List<PlayerController>();
    [HideInInspector] public List<EnemyController> enemyControllers = new List<EnemyController>();
    [HideInInspector] public List<PetController> petControllers = new List<PetController>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // check for web socket
        m_unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (m_unityTransport != null)
        {
            if (Bootstrap.IsLocalConnection())
            {
                Bootstrap.Instance.IpAddress = "127.0.0.1";
                Bootstrap.Instance.GamePort = 9000;
                m_unityTransport.UseEncryption = false;
            }

            if (Bootstrap.IsServer())
            {
                // set a reasonably high target frame rate to reduce latency
                //Application.targetFrameRate = Bootstrap.IsRemoteConnection() ? 1200 : 300;
                Application.targetFrameRate = 180;
                QualitySettings.vSyncCount = 0;

                // hide loading canvas
                LoadingCanvas.Instance.gameObject.SetActive(false);

                // connect
                ConnectServerGame();
            }
            else if (Bootstrap.IsClient())
            {
                QualitySettings.vSyncCount = 1;

                Application.targetFrameRate = 120;

                // connect to a client game (leave gameId param "" to signify we want an empty game)
                ConnectClientGame(Bootstrap.Instance.isJoiningFromTitle ? Bootstrap.Instance.GameId : "");
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
            TryConnect();
        }

        if (Bootstrap.IsServer())
        {
            if (isReconnectTimerActive)
            {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer < 0) isReconnectTimerActive = false;
            }
        }

        if (Bootstrap.IsClient())
        {

        }
    }

    private float m_noTimer = 0f;

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

        // connect
        TryConnect();
    }

    public async UniTaskVoid ConnectClientGame(string gameId = "")
    {
        Debug.Log("ConnectClientGame()");

        //m_currentGameId = gameId;

        bool isReturnToTitleOnFail = string.IsNullOrEmpty(gameId) || Bootstrap.Instance.isJoiningFromTitle;

        if (m_unityTransport == null)
        {
            ErrorDialogCanvas.Instance.Show("Transports not available.");
            if (isReturnToTitleOnFail) SceneManager.LoadScene("Title");
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
                    ErrorDialogCanvas.Instance.Show("The Dropt server manager is either full or not online, you can check https//manager.playdropt.io to see available instances.");
                    if (isReturnToTitleOnFail) SceneManager.LoadScene("Title");
                    return;
                }

                // check for non-succes
                if (response.responseCode != 200)
                {
                    ErrorDialogCanvas.Instance.Show(response.message);
                    if (isReturnToTitleOnFail) SceneManager.LoadScene("Title");
                    return;
                }

                // set IP address and port
                Bootstrap.Instance.IpAddress = response.ipAddress;
                Bootstrap.Instance.GamePort = ushort.Parse(response.gamePort);
                Bootstrap.Instance.GameId = response.gameId;
                //m_currentGameId = response.gameId;
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
            Debug.Log("Shutdown NetworkManager");
            NetworkManager.Singleton.Shutdown();
        }

        m_isTryConnectClientGame = true;
        //m_currentGameId = gameId;
        //Bootstrap.Instance.GameId = gameId;
    }


    public bool TryConnect()
    {
        var success = true;

        if (Bootstrap.IsClient())
        {
            success = NetworkManager.Singleton.StartClient();
            Debug.Log("StartClient()");
            Bootstrap.Instance.isJoiningFromTitle = false;
        }
        else if (Bootstrap.IsServer())
        {
            success = NetworkManager.Singleton.StartServer();
            Debug.Log("StartServer()");
            isServerReady = true;
        }
        else if (Bootstrap.IsHost())
        {
            success = NetworkManager.Singleton.StartHost();
            Debug.Log("StartHost()");
        }


        return success;
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

        // connect
        TryConnect();
        //SpawnAfterConnectSpawnPrefabs();
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

    private string m_testGameServerCommonName = "test-game-server.playdropt.io";

    private string m_testGameServerChainPem = @"
-----BEGIN CERTIFICATE-----
MIIEVzCCAj+gAwIBAgIRAIOPbGPOsTmMYgZigxXJ/d4wDQYJKoZIhvcNAQELBQAw
TzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2Vh
cmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMjQwMzEzMDAwMDAw
WhcNMjcwMzEyMjM1OTU5WjAyMQswCQYDVQQGEwJVUzEWMBQGA1UEChMNTGV0J3Mg
RW5jcnlwdDELMAkGA1UEAxMCRTUwdjAQBgcqhkjOPQIBBgUrgQQAIgNiAAQNCzqK
a2GOtu/cX1jnxkJFVKtj9mZhSAouWXW0gQI3ULc/FnncmOyhKJdyIBwsz9V8UiBO
VHhbhBRrwJCuhezAUUE8Wod/Bk3U/mDR+mwt4X2VEIiiCFQPmRpM5uoKrNijgfgw
gfUwDgYDVR0PAQH/BAQDAgGGMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcD
ATASBgNVHRMBAf8ECDAGAQH/AgEAMB0GA1UdDgQWBBSfK1/PPCFPnQS37SssxMZw
i9LXDTAfBgNVHSMEGDAWgBR5tFnme7bl5AFzgAiIyBpY9umbbjAyBggrBgEFBQcB
AQQmMCQwIgYIKwYBBQUHMAKGFmh0dHA6Ly94MS5pLmxlbmNyLm9yZy8wEwYDVR0g
BAwwCjAIBgZngQwBAgEwJwYDVR0fBCAwHjAcoBqgGIYWaHR0cDovL3gxLmMubGVu
Y3Iub3JnLzANBgkqhkiG9w0BAQsFAAOCAgEAH3KdNEVCQdqk0LKyuNImTKdRJY1C
2uw2SJajuhqkyGPY8C+zzsufZ+mgnhnq1A2KVQOSykOEnUbx1cy637rBAihx97r+
bcwbZM6sTDIaEriR/PLk6LKs9Be0uoVxgOKDcpG9svD33J+G9Lcfv1K9luDmSTgG
6XNFIN5vfI5gs/lMPyojEMdIzK9blcl2/1vKxO8WGCcjvsQ1nJ/Pwt8LQZBfOFyV
XP8ubAp/au3dc4EKWG9MO5zcx1qT9+NXRGdVWxGvmBFRAajciMfXME1ZuGmk3/GO
koAM7ZkjZmleyokP1LGzmfJcUd9s7eeu1/9/eg5XlXd/55GtYjAM+C4DG5i7eaNq
cm2F+yxYIPt6cbbtYVNJCGfHWqHEQ4FYStUyFnv8sjyqU8ypgZaNJ9aVcWSICLOI
E1/Qv/7oKsnZCWJ926wU6RqG1OYPGOi1zuABhLw61cuPVDT28nQS/e6z95cJXq0e
K1BcaJ6fJZsmbjRgD5p3mvEf5vdQM7MCEvU0tHbsx2I5mHHJoABHb8KVBgWp/lcX
GWiWaeOyB7RP+OfDtvi2OsapxXiV7vNVs7fMlrRjY1joKaqmmycnBvAq14AEbtyL
sVfOS66B8apkeFX2NY4XPEYV4ZSCe8VHPrdrERk2wILG3T/EGmSIkCYVUMSnjmJd
VQD9F6Na/+zmXCc=
-----END CERTIFICATE-----
";
}