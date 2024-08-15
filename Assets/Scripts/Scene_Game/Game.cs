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

    // certificate variables for wss and encryption
    // IMPORTANT: The only certs that seem to work correctly when deployed in browser are those for the actual website (web.playdropt.io)
    // and the ones that are used to secure the website itself (letsencrypt)
    private string m_serverCommonName;
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;


    private void Awake()
    {
        Instance = this;
        SetupServerCerts();

        NetworkManager.Singleton.OnTransportFailure += HandleTransportFailure;
        NetworkManager.Singleton.OnConnectionEvent += HandleConnectionEvent;
    }

    void HandleTransportFailure()
    {
        Debug.Log("There was a connection failure");
    }

    void HandleConnectionEvent(NetworkManager nm, ConnectionEventData ev)
    {
        Debug.Log("Connection event: " + ev);
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
                ProgressBarCanvas.Instance.ResetProgress();
                ProgressBarCanvas.Instance.Show("Engaging remote server manager...");

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

    bool m_isCreatedGameOnce = false;

    private void Update()
    {
        // CreateGame polling
        m_tryCreateGameTimer -= Time.deltaTime;
        if (m_isTryCreateGame && m_tryCreateGameTimer <= 0 && !m_isCreatedGameOnce)
        {
            m_isCreatedGameOnce = true;
            m_isTryCreateGame = false;
            CreateGameViaServerManager().Forget(); // Use UniTask with Forget to avoid unhandled exceptions
        }

        // Connect polling
        m_tryConnectTimer -= Time.deltaTime;
        if (m_isTryConnect &&
            m_tryConnectTimer <= 0 &&
            !NetworkManager.Singleton.ShutdownInProgress)
        {
            if (AvailableGamesHeartbeat.Instance.IsServerReady(Bootstrap.Instance.GameId))
            {
                ProgressBarCanvas.Instance.Show("Server is ready, starting client...");

                if (TryStartServerClientOrHost())
                {
                    IsConnected = true;
                    m_isTryConnect = false;
                    ProgressBarCanvas.Instance.Show("Client started, loading level...");
                    ProgressBarCanvas.Instance.Hide();
                }
                else
                {
                    Debug.Log("Connection failed, restart try timer");
                    m_tryConnectTimer = k_pollInterval;
                }

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
        m_isCreatedGameOnce = false;
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
                m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
                //m_transport.SetServerSecrets(SecureParameters.serverCert, SecureParameters.serverPrivKey);
            }
        }
        else if (Bootstrap.IsClient() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port);
            if (m_transport.UseEncryption)
            {
                m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
                //m_transport.SetClientSecrets(SecureParameters.serverCommonName, SecureParameters.clientCA);
                ProgressBarCanvas.Instance.Show("Client certificate provided to server. Validating...");
            }
        }

        ProgressBarCanvas.Instance.Show("Connection data set, awaiting final server setup...");

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
        }

        return success;
    }

    public UniTask CreateGameViaServerManager()
    {
        return CreateGameViaServerManagerAsync();
    }

    private async UniTask CreateGameViaServerManagerAsync()
    {
        ProgressBarCanvas.Instance.Show("Requesting game server instance... ");

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

                        // set client side cert data if using server manager
                        m_serverCommonName = data.serverCommonName;
                        m_clientCA = data.clientCA;

                        // update progress bar
                        ProgressBarCanvas.Instance.Show("Allocated gameId: " + data.gameId + "on port: " + data.port + ", connecting...");

                        // save gameid for joins and try connect
                        m_isTryConnect = true;
                        //m_tryConnectTimer = 10f;

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

#if !CLIENT
public class SecureParameters
{
    public static string serverCommonName = "web.playdropt.io";
    public static string clientCA =
        @"-----BEGIN CERTIFICATE-----
MIIFBTCCAu2gAwIBAgIQS6hSk/eaL6JzBkuoBI110DANBgkqhkiG9w0BAQsFADBP
MQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJuZXQgU2VjdXJpdHkgUmVzZWFy
Y2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBYMTAeFw0yNDAzMTMwMDAwMDBa
Fw0yNzAzMTIyMzU5NTlaMDMxCzAJBgNVBAYTAlVTMRYwFAYDVQQKEw1MZXQncyBF
bmNyeXB0MQwwCgYDVQQDEwNSMTAwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQDPV+XmxFQS7bRH/sknWHZGUCiMHT6I3wWd1bUYKb3dtVq/+vbOo76vACFL
YlpaPAEvxVgD9on/jhFD68G14BQHlo9vH9fnuoE5CXVlt8KvGFs3Jijno/QHK20a
/6tYvJWuQP/py1fEtVt/eA0YYbwX51TGu0mRzW4Y0YCF7qZlNrx06rxQTOr8IfM4
FpOUurDTazgGzRYSespSdcitdrLCnF2YRVxvYXvGLe48E1KGAdlX5jgc3421H5KR
mudKHMxFqHJV8LDmowfs/acbZp4/SItxhHFYyTr6717yW0QrPHTnj7JHwQdqzZq3
DZb3EoEmUVQK7GH29/Xi8orIlQ2NAgMBAAGjgfgwgfUwDgYDVR0PAQH/BAQDAgGG
MB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATASBgNVHRMBAf8ECDAGAQH/
AgEAMB0GA1UdDgQWBBS7vMNHpeS8qcbDpHIMEI2iNeHI6DAfBgNVHSMEGDAWgBR5
tFnme7bl5AFzgAiIyBpY9umbbjAyBggrBgEFBQcBAQQmMCQwIgYIKwYBBQUHMAKG
Fmh0dHA6Ly94MS5pLmxlbmNyLm9yZy8wEwYDVR0gBAwwCjAIBgZngQwBAgEwJwYD
VR0fBCAwHjAcoBqgGIYWaHR0cDovL3gxLmMubGVuY3Iub3JnLzANBgkqhkiG9w0B
AQsFAAOCAgEAkrHnQTfreZ2B5s3iJeE6IOmQRJWjgVzPw139vaBw1bGWKCIL0vIo
zwzn1OZDjCQiHcFCktEJr59L9MhwTyAWsVrdAfYf+B9haxQnsHKNY67u4s5Lzzfd
u6PUzeetUK29v+PsPmI2cJkxp+iN3epi4hKu9ZzUPSwMqtCceb7qPVxEbpYxY1p9
1n5PJKBLBX9eb9LU6l8zSxPWV7bK3lG4XaMJgnT9x3ies7msFtpKK5bDtotij/l0
GaKeA97pb5uwD9KgWvaFXMIEt8jVTjLEvwRdvCn294GPDF08U8lAkIv7tghluaQh
1QnlE4SEN4LOECj8dsIGJXpGUk3aU3KkJz9icKy+aUgA+2cP21uh6NcDIS3XyfaZ
QjmDQ993ChII8SXWupQZVBiIpcWO4RqZk3lr7Bz5MUCwzDIA359e57SSq5CCkY0N
4B6Vulk7LktfwrdGNVI5BsC9qqxSwSKgRJeZ9wygIaehbHFHFhcBaMDKpiZlBHyz
rsnnlFXCb5s8HKn5LsUgGvB24L7sGNZP2CX7dhHov+YhD+jozLW2p9W4959Bz2Ei
RmqDtmiXLnzqTpXbI+suyCsohKRg6Un0RC47+cpiVwHiXZAW+cn8eiNIjqbVgXLx
KPpdzvvtTnOPlC7SQZSYmdunr3Bf9b77AiC/ZidstK36dRILKz7OA54=
-----END CERTIFICATE-----";

#if CLIENT
    public static string serverCert = "";
    public static string serverPrivKey = "";
#else
    public static string serverCert =
        @"-----BEGIN CERTIFICATE-----
MIIE8DCCA9igAwIBAgISBJzyJOvmWW3e6ZMn4p/Gre34MA0GCSqGSIb3DQEBCwUA
MDMxCzAJBgNVBAYTAlVTMRYwFAYDVQQKEw1MZXQncyBFbmNyeXB0MQwwCgYDVQQD
EwNSMTAwHhcNMjQwNzA1MTIzNTMxWhcNMjQxMDAzMTIzNTMwWjAbMRkwFwYDVQQD
ExB3ZWIucGxheWRyb3B0LmlvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC
AQEAs9I4O7aSN4xtzVNDE4VDEtIY7KtD0pANiD9eNMtK0cv5j+esEOl9xJu6ywbo
YoNzD2KLQviNTIphZamasMwWWFJ1si/qpCscIV4+EvOUC926f6wGqz/j1qXJ5iX+
gwIG9KfYpcI39AZMoo1SCLw/NlnuOvC0qX6eIqDjc0ik6tK0qMEQJjz51lPAJiel
0ncaH9N6CiNPAaBhs+IRfZ5PaoOwsp06tatSsNKxCM5kKj4pxZ9u7UH7/LALFvhI
aQbpoF9+uOAmxzxaln4yuXvHa0DYYCcRDf11o7w5IYIhFx6+1H1zyGw/Jt03p6Rg
y8heCNMbZMYqrqAM/c5hOlZYswIDAQABo4ICFDCCAhAwDgYDVR0PAQH/BAQDAgWg
MB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAMBgNVHRMBAf8EAjAAMB0G
A1UdDgQWBBTdHP/qDT6wW7EYD/5C8O4OmjMd3DAfBgNVHSMEGDAWgBS7vMNHpeS8
qcbDpHIMEI2iNeHI6DBXBggrBgEFBQcBAQRLMEkwIgYIKwYBBQUHMAGGFmh0dHA6
Ly9yMTAuby5sZW5jci5vcmcwIwYIKwYBBQUHMAKGF2h0dHA6Ly9yMTAuaS5sZW5j
ci5vcmcvMBsGA1UdEQQUMBKCEHdlYi5wbGF5ZHJvcHQuaW8wEwYDVR0gBAwwCjAI
BgZngQwBAgEwggEEBgorBgEEAdZ5AgQCBIH1BIHyAPAAdwA/F0tP1yJHWJQdZRyE
vg0S7ZA3fx+FauvBvyiF7PhkbgAAAZCDG27VAAAEAwBIMEYCIQCU2jl4q6XKMeqh
MQvqM4Aeo/cDNUW9/zGiRIKvS0CK1QIhANGlzKT4Xu8S2h1UGU/jKWXhsxOnx4HU
Fg6BW3yH2l6wAHUA3+FW66oFr7WcD4ZxjajAMk6uVtlup/WlagHRwTu+UlwAAAGQ
gxtvlwAABAMARjBEAiBGSIZwK2MymgVc84H06sxC4LTRAp30tDWTzQ42WHOWfwIg
cpUPBDtlnYDiVOsIvAMvrc85flXfdy6EXJQxU/SRwSQwDQYJKoZIhvcNAQELBQAD
ggEBABASS/EENRMkOow280Zv1XPz8kG/TV4cqmBAr5rXIK7pD1+aTWaq6dJGanov
dlrDJ3jYBqE2P2NaQ5kXCdPTGKP0Wto2jBzKqd+8MfswpVT2z2M3nqTBs7HRJ2Vc
YUkUy2uYgPJvIHnGSZvtiHs9up8WV9Z8DC+EXzanfp5kiCxx5QTbQxj1XPxKcUBB
3RzatLcOPtJrnLMnd2engR/YEmSm7zD0xcyUdc7idYiz/bSSTn7bFoGYd+Td0nQb
8KaledqOKhyCzCPkOkTU2r9hJ+pI8rrslELpORMXyRuXAXAtYSXyCD9VeW8EATWB
rbVFeEi1ufm/bekePcKhBoOdRoA=
-----END CERTIFICATE-----";

    public static string serverPrivKey =
        @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCz0jg7tpI3jG3N
U0MThUMS0hjsq0PSkA2IP140y0rRy/mP56wQ6X3Em7rLBuhig3MPYotC+I1MimFl
qZqwzBZYUnWyL+qkKxwhXj4S85QL3bp/rAarP+PWpcnmJf6DAgb0p9ilwjf0Bkyi
jVIIvD82We468LSpfp4ioONzSKTq0rSowRAmPPnWU8AmJ6XSdxof03oKI08BoGGz
4hF9nk9qg7CynTq1q1Kw0rEIzmQqPinFn27tQfv8sAsW+EhpBumgX3644CbHPFqW
fjK5e8drQNhgJxEN/XWjvDkhgiEXHr7UfXPIbD8m3TenpGDLyF4I0xtkxiquoAz9
zmE6VlizAgMBAAECggEATtsTrN/xu+lZp25pXlCUqta2dmbebaKdRI/hXI5/x4PG
83vZYWs7K3JNVkY3tyfE18zTnDFKKXQPpRjczMYa0G2MznCj2Y1MHzfoScBGPnPk
GqPSItv4KoN2h/ZGZ6YGbdiDMaF7jwgKKEiH3mvK3qdOSMuQtjlf2Hisdbw4Ef9L
BsZZsSuMwEuXKAiWcDozr3VV0OJEyXEe/kDcHFmfTAbCd5+H0GePfwFgOp1lzMoA
tEs8prLtkvsvDoqXUZk/EcBJsAzjCgKH7k6RiH8LzbvYAzvu8/fepi78F8WxKeou
T5rAkE89W7r8hm2X8TOJY7+WmqeNziZeSNm9hvFsLQKBgQDJHy95q/QiJYctEMPB
0F50Ju99MnWuOwCEE8HrkiXn3QK8kElXSACbrsk7mUhUsZmGHz6wD7F/E3vZR8qo
2w/YsG/fUocMfo4EbVpVEA481gE9em1+4NdfMxkIZ8XTKYneeUeIhjA+3/yXfdpl
0e7cfotF/UjiESUQJXRjuumWvwKBgQDk4yK/EcYiEy43NHrhpRYY2iVTsbDmaeNl
E1nltkI6J2HMo4qXLOoGyvHPuSFo5b/nzcSRr8vj6jC5hyeMrXrIB0+TbMrDSw8H
czvIwU73wId4KnqLKZnGKgoA/MatiW4pdVu68X9k/a0Y634cbQQlWTDl2ZgnXsPR
GjbPjPuPDQKBgH7xLRD55KSs1S4vl688KnHbpWt7LuXince/hLWAUGaRi8mitHff
nWqmHqN8czfpxQHvtKyBq0GO9avF8Xc0lULq0iG9wDm1o0POFab89E+Xr76zCGt9
1NJkuRciEK3gWQHPwMO+FrOIwrCHohKEN+R6BsQNQzRVJ/SR/213KzqXAoGALOR3
zi34RHqql91NNLCicuFmbNHvNmISXaK8ARgMSUesIOz6o8gFZqurXeibqu1VBGwR
X9mxDdTDFcBye18TM+RrMSknY8J3AikR0sBHcsRqTaFXQ7A3Huzj5Wmuth68YplI
EpSHPhGbP8YAiCbBp2mk85AIDcDCe2K+2Vp3hIkCgYEAiZjEbi166xtf/elL690q
0NUTVIAMpsoA/+LeWXh/Djz5Hr42QR3FroxZM7lBrTV4c4CCNPAxTsB41L58Vxhf
95g9rIHFQ5Gsj5HEjlogxI5Ynz75sHe5TA7YkMmaMwOEfKVZrZWd7iuFUvUAcSsN
p5nadIbVmbRfUfJrm2c88g8=
-----END PRIVATE KEY-----";

#endif

}
#endif
