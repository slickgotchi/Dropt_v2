using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using Audio.Game;

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
    public bool IsConnected = false;

    private string createGameUri = "https://alphaserver.playdropt.io/creategame";
    private string joinGameUri = "https://alphaserver.playdropt.io/joingame";

    private float k_pollInterval = 2f;

    // params for connecting
    public bool m_isTryConnect = false;
    private float m_tryConnectTimer = 0f;

    // params for serverManager (nodejs app) connections
    private bool m_isTryCreateGame = false;
    private float m_tryCreateGameTimer = 0f;

    UnityTransport m_transport;

    // certs
    private string m_serverCommonName;
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    private void Awake()
    {
        Instance = this;
        SetupServerCerts();
    }

    void SetupServerCerts()
    {
        if (Bootstrap.IsServer() && Bootstrap.IsRemoteConnection() && Bootstrap.IsUseServerManager())
        {
            m_serverCertificate = System.Environment.GetEnvironmentVariable("DROPT_SERVER_CERTIFICATE");
            m_serverPrivateKey = System.Environment.GetEnvironmentVariable("DROPT_SERVER_PRIVATE_KEY");
        }
    }

    private void Start()
    {
        // 1. start tryConnect and tryCreateGame as false
        m_isTryCreateGame = false;
        m_isTryConnect = false;

        // 2. ensure we have access to UnityTransport
        m_transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;
        if (m_transport == null) { Debug.Log("Could not get UnityTransport"); return; }

        // 3. Server instances
        if (Bootstrap.IsServer())
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            m_isTryConnect = true;
        }

        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            if (Bootstrap.Instance.UseServerManager)
            {
                Debug.Log("Try create game using server manager...");
                ProgressBarCanvas.Instance.Show("Engaging remote server manager...", 0.1f);

                // try create game (via server manager)
                m_isTryCreateGame = true;
            }
            else
            {
                m_isTryConnect = true;
            }
        }

        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
            m_isTryConnect = true;
        }

        // disable audio duplicate audio listeners
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (audioListeners.Length > 1 && GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.GetComponent<AudioListener>().enabled = false;
        }
    }

    bool isCreatedGameOnce = false;

    private void Update()
    {
        // CreateGame polling
        m_tryCreateGameTimer -= Time.deltaTime;
        if (m_isTryCreateGame && m_tryCreateGameTimer <= 0 && !isCreatedGameOnce)
        {
            Debug.Log("Calling CreateGameViaServerManager()");
            isCreatedGameOnce = true;
            m_isTryCreateGame = false;
            CreateGameViaServerManager().Forget(); // Use UniTask with Forget to avoid unhandled exceptions
        }

        // Connect polling
        m_tryConnectTimer -= Time.deltaTime;
        if (m_isTryConnect && m_tryConnectTimer <= 0 && !NetworkManager.Singleton.ShutdownInProgress)
        {
            Debug.Log("TryConnect");
            if (TryStartServerClientOrHost())
            {
                IsConnected = true;
                m_isTryConnect = false;
            }
            else
            {
                m_tryConnectTimer = k_pollInterval;
            }
        }
    }

    public void TriggerGameOver(REKTCanvas.TypeOfREKT typeOfREKT)
    {
        REKTCanvas.Instance.Show(typeOfREKT);
        NetworkManager.Singleton.Shutdown();
    }

    public void TryCreateGame()
    {
        m_isTryConnect = false;
        m_isTryCreateGame = true;
    }

    bool TryStartServerClientOrHost()
    {
        // set encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        var ipAddress = Bootstrap.IsRemoteConnection() ? Bootstrap.Instance.IpAddress : "127.0.0.1";

        if (Bootstrap.IsServer() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port, "0.0.0.0");
            if (m_transport.UseEncryption)
            {
                //m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
                m_transport.SetServerSecrets(SecureParameters.serverCert, SecureParameters.serverPrivKey);
                Debug.Log("Server Secrets Set");
            }
        }
        else if (Bootstrap.IsClient() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port);
            if (m_transport.UseEncryption)
            {
                //m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
                m_transport.SetClientSecrets(SecureParameters.serverCommonName, SecureParameters.clientCA);
                Debug.Log("Client Secrets Set");
            }
        }

        ProgressBarCanvas.Instance.Show("Connection data set, awaiting final server setup...", 0.4f);

        // store a bool for our connection success
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

            ProgressBarCanvas.Instance.Show("Client connected to server with gameId: " + Bootstrap.Instance.GameId + " on port: " + Bootstrap.Instance.Port, 0.5f);
        }

        return success;
    }

    public UniTask CreateGameViaServerManager()
    {
        return CreateGameViaServerManagerAsync();
    }

    private async UniTask CreateGameViaServerManagerAsync()
    {
        ProgressBarCanvas.Instance.Show("Requesting game instance... ", 0.2f);
        Debug.Log("Requesting game isntance");

        // create a new game
        try
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(createGameUri))
            {
                await webRequest.SendWebRequest().ToUniTask();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ConnectionError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.DataProcessingError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ProtocolError: " + webRequest.error);
                        m_isTryCreateGame = true;
                        m_tryCreateGameTimer = k_pollInterval;
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(webRequest.result);
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);

                        // using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client side cert data
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;
                        Debug.Log(m_serverCommonName);
                        Debug.Log(m_clientCA);

                        Debug.Log("Got back server instance with port: " + data.port + ", gameId: " + data.gameId +
                            ", ipAddress: " + data.ipAddress);

                        // update progress bar
                        ProgressBarCanvas.Instance.Show("Allocated gameId: " + data.gameId + "on port: " + data.port + ", connecting...", 0.3f);

                        // save gameid for joins and try connect
                        m_isTryConnect = true;

                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception occurred: " + e.Message);
            ErrorDialogCanvas.Instance.Show(e.Message);

            // try to create the game again
            m_isTryCreateGame = true;
            m_tryCreateGameTimer = k_pollInterval;
        }
    }

    // other gameobjects call this to try join a game
    public void TryJoinGame(string gameId)
    {
        JoinGameViaServerManager(gameId).Forget(); // Use UniTask with Forget to avoid unhandled exceptions
    }

    public UniTask JoinGameViaServerManager(string gameId)
    {
        return JoinGameViaServerManagerAsync(gameId);
    }

    private async UniTask JoinGameViaServerManagerAsync(string gameId)
    {
        try
        {
            // 1. assemble post data into json
            var postData = new JoinGamePostData { gameId = gameId };
            string json = JsonUtility.ToJson(postData);

            // 2. Create a new UnityWebRequest
            Debug.Log("Game: Sending POST request to " + joinGameUri + " with json data: " + json);
            using (UnityWebRequest webRequest = new UnityWebRequest(joinGameUri, "POST"))
            {
                // 3. populate the requests body
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // 4. perform the request
                await webRequest.SendWebRequest().ToUniTask();

                // 5. process results
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ConnectionError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.ProtocolError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        ErrorDialogCanvas.Instance.Show("UnityWebRequest.Result.DataProcessingError: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log("Response: " + webRequest.downloadHandler.text);
                        CreateOrJoinGameResponseData data = JsonUtility.FromJson<CreateOrJoinGameResponseData>(webRequest.downloadHandler.text);

                        // Using data configure bootstrap
                        Bootstrap.Instance.IpAddress = data.ipAddress;
                        Bootstrap.Instance.Port = data.port;
                        Bootstrap.Instance.GameId = data.gameId;

                        // set client side cert data if using server manager
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;

                        // shut down our existing server
                        NetworkManager.Singleton.Shutdown();

                        // We can now connect direct
                        m_isTryConnect = true;

                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);

            // just display error message (player can choose to hit join again once they've checked their params)
            ErrorDialogCanvas.Instance.Show(ex.Message);
        }
    }

    [System.Serializable]
    public struct CreateOrJoinGameResponseData
    {
        public string ipAddress;
        public ushort port;
        public string serverCommonName;
        public string clientCA;
        public string gameId;
    }

    [System.Serializable]
    struct JoinGamePostData
    {
        public string gameId;
    }







}


public class SecureParameters
{
    public static string serverCommonName = "alphaserver.playdropt.io";
    public static string clientCA =
        @"-----BEGIN CERTIFICATE-----
MIICwjCCAaoCCQCXeUQ0rdG30jANBgkqhkiG9w0BAQsFADAjMSEwHwYDVQQDDBhh
bHBoYXNlcnZlci5wbGF5ZHJvcHQuaW8wHhcNMjQwODExMDcxNTM4WhcNMjcwODEx
MDcxNTM4WjAjMSEwHwYDVQQDDBhhbHBoYXNlcnZlci5wbGF5ZHJvcHQuaW8wggEi
MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDPN8j6taqkNUysXjW0xrmsYbAA
aFgy20IYm4tLN9xxqNmUrqaiGA2ktrsfW43x/Iv7tsAiP8o/j7TEuicAKKAaQOL4
+znyDJ31AQH8aot4NDhvleLagXltCdmd308+s0bcduguXcZhEUUx3RAdIXiUkWQQ
EI6HacwQm+QEkdmDvTu1igHKS8Q3ORTpakFduox3sUQmB0Nxm0FpnhjHcZHck+cd
XoyZlNVhu30e2s2orTNvBHtbxGmOde81GATXt0idskv3xDZGu7eXHMjxHmXKVRrO
809IZxeokapFcYtFcw+6vjQiNx9dA6CKH1VtQRWfQuF704Jnx2IZk1jtIznbAgMB
AAEwDQYJKoZIhvcNAQELBQADggEBAFf1K3ZRc2WFX/1GDPblYcY68Zlg46SPAwDQ
h/uTcf5dU91hIR85t8bpu0Dr9tzCaHxtUfxs3JA/1OhmkVMgoMhnmImc4nUODewm
oyUka02qK1Mb2ZgXmI5jTyRCpMu/2h+KLOub3JK9aS/wxaisrafFVH6YqYrFsPs4
BD23sHya4BHBxTdoVtr0EIvMxxCmSVJw2kGnlhhjHpMZDBczPswfI1ulFu0DhybJ
L0syomyNjQNUby6Tn8X4+AF9O6GTejQJqw01NwpvpUkrPW8HU5ZVdws9P/ngYw2j
MTohCE7+ySBc05b9th1CC9IvxsrENapURWqGCE7zmUXK59iyCA4=
-----END CERTIFICATE-----";

#if CLIENT
    public static string serverCert = "";
    public static string serverPrivKey = "";
#else
    public static string serverCert =
        @"-----BEGIN CERTIFICATE-----
MIICwjCCAaoCCQCrDfq7bynlvjANBgkqhkiG9w0BAQsFADAjMSEwHwYDVQQDDBhh
bHBoYXNlcnZlci5wbGF5ZHJvcHQuaW8wHhcNMjQwODExMDcxNzUxWhcNMjUwODEx
MDcxNzUxWjAjMSEwHwYDVQQDDBhhbHBoYXNlcnZlci5wbGF5ZHJvcHQuaW8wggEi
MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCiwxnDVXVO3aUZYA+CQ4eLOpW2
DNhimRovF2gqvpXEDedgnGLJqHPlctbGPT0IdTwjPdRbwNlU5U0/fauRx3UwL3Af
MWCvEw/+ulsBfp+BB+8ejNlV2gZtZUNgUrDck0X7zM5m2+Yi0S+PQXnBroNL0zP6
vPPWah0gyAm4MududoIgDDv8oPDQmYfZ288jACEUNS/AgE0TwLaIkSD2YopR1Xfw
wUqdbcyb4Fqu/OREPQShK51F++u+r9d3jCXl+A+CRLr94BMZ7pxrDe5/K3Y7qs6+
h7h+rsX7JkNI+5cqXd8CYRXRL9AlEhpvUvky6jVL4YsQ4NaEzJ6k595LXyMZAgMB
AAEwDQYJKoZIhvcNAQELBQADggEBALcf8GIBszwmI4V0MjajQJgk/KUy4pt/MCQo
Hv/eOEVe9d4GTtkvWWUFFozlt0A1OwQjS19V1r6NHs5E6wF+sV7fS88i+OuK30B4
mmXO8Iq4pphPCJLJVCJTYENftSjPX/bVkVt82kGo8a5bsi5rfc9fybErXLPYVfsu
AwUEuClkUIdxMxTYJoDhjpJoIPpwudoS/nUgD4lwaPM+Nlb1sDjGjdyLXLZvVdLu
H5FkMDSCXZsgKLpNdBSHcco1FbxljV7gZch6HKRU8sFRmVM+5cbh749UAo05EDB4
oJWwI6f4aqYK1Gh7nsx/prR2AhXre+BRBlOr9ZIU5WKYpLdBcIo=
-----END CERTIFICATE-----";

    public static string serverPrivKey =
        @"-----BEGIN RSA PRIVATE KEY-----
MIIEogIBAAKCAQEAosMZw1V1Tt2lGWAPgkOHizqVtgzYYpkaLxdoKr6VxA3nYJxi
yahz5XLWxj09CHU8Iz3UW8DZVOVNP32rkcd1MC9wHzFgrxMP/rpbAX6fgQfvHozZ
VdoGbWVDYFKw3JNF+8zOZtvmItEvj0F5wa6DS9Mz+rzz1modIMgJuDLnbnaCIAw7
/KDw0JmH2dvPIwAhFDUvwIBNE8C2iJEg9mKKUdV38MFKnW3Mm+BarvzkRD0EoSud
Rfvrvq/Xd4wl5fgPgkS6/eATGe6caw3ufyt2O6rOvoe4fq7F+yZDSPuXKl3fAmEV
0S/QJRIab1L5Muo1S+GLEODWhMyepOfeS18jGQIDAQABAoIBAB7FT8uUDnd4g8wG
UyhHaAq0arVePFJ3q3GXtUPPgDTug/3J0wtY44BPc7dKwI0mzNXEzK8ECJJ6P15v
fc4zrT4M2d+r0CGJMw7vYGEp9THJtDVMX5JRg8GO0WwWdgVdem+eSq87h4ixj5I/
yKsLORtOtJcEvfydVyBpcRz30rUZOU9SoZSPN4yDasxr0o8vHh6Bpb8CNh65NNbn
CCJVXSuTrY78wE70y3Muiw6rEJKRgh6pJWmxxvAypRWXmDFWXcCmoG/SraUTOolh
l+tbVPF3TaaV39TrH2RFoZMAYw5IlRxCRKnNhFTLT5mkm8CjlVCbOTWVDOsyyIqy
5wCpr7kCgYEA0XsWkSqd2ChHTQAvydcaKSMBL3KLfCwmTedwNw5p2r67Ct89wKmh
2grzoKmU8Q+7c4lRvikyETVC4myzkFuUSaecbePKh6qZMKAz/dNDZC+WFf6VvOuH
2knPShUvGVyJOqCY9nhwz0Tmd2z0hQeEN4Ym4RzduoSsKEwUIBRsdI8CgYEAxugS
CbdXhK7fKGHHewxO0i4ed2K+NETNSYKs+rzsLM6iEXCtAESaWcsGobKh3DIGaHfx
lvw+AFk1o8D8lNwy8nZwkw20ka4LgEa8p/atLZf01ovjI49KChP/3cqsF63vriiF
HiWVbdb2cfA5O9HSjKpnMaS06ii2ePYIpLoYUdcCgYBiKSGcCLJKdiVjKbE7DbbO
i/6kMzK1jyKr4sWspu5neHTBVXbkbxjOyc77/Ds08sBOFYzeZQN3GNQsse86uA82
rHoa7GEdTY3XQVrbmEG+EqZrzA5yppPUcD3YYzDc24XamSLUa//AwHKWh9HU/H6y
XgSd/B7SphTeFThhB/ECdQKBgB5PkSgf41tZ1rHtrJtotb47vvLMflWywmHYYwnW
rlrppjLoK8Tlr2vNj5YmhZnrmaRj2tH6YGxnK9BngVYh9DWUrPUL2p90mVYT8X3b
DmrrRClJqfRqSoscnxoqX21AWUz96cM9UPcrEeUtCVu/TsmW0iDzi4o/aAco3wpT
PY+DAoGASrxEcGaVMjlpiO1LZC8H/AvBTG4hv4SMh2crlN/CdqikwR6c2VWLknfK
GPWVnAA+lPJPHehg9Hh+BTCZICrvCs2Npqs2ktdNnKaJ+uvHvxGL7UVYVf0JAs1M
qtN8A4cN3Qeq8DTgU6t3IMhmTqeFjpRzY3JaPbByyqT43vJ6I/4=
-----END RSA PRIVATE KEY-----";

#endif

}