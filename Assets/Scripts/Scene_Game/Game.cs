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
    private string m_clientCA = @"
-----BEGIN CERTIFICATE-----
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
    //private string m_serverCertificate;

    private string m_serverCertificate = @"
-----BEGIN CERTIFICATE-----
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

    //private string m_serverPrivateKey;

    private string m_serverPrivateKey = @"
-----BEGIN PRIVATE KEY-----
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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. Load certificate files
        //LoadCertificateFiles();

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
            m_clientCA = File.ReadAllText("/certs/chain.pem");
            m_serverCertificate = File.ReadAllText("/certs/cert.pem");
            m_serverPrivateKey = File.ReadAllText("/certs/privkey.pem");

            Debug.Log("Certificates loaded successfully.");
            Debug.Log(m_serverCertificate);
            Debug.Log(m_serverPrivateKey);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading certificates: {e.Message}");
        }
    }

    private async void ConnectClientGame()
    {
        Debug.Log("ConnectClientGame()");
        var response = await ServerManagerAgent.Instance.JoinEmpty();

        if (response == null)
        {
            Debug.LogError("Failed to join empty instance.");
            return;
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
