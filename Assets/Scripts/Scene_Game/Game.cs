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
            m_clientCA = File.ReadAllText("/etc/letsencrypt/live/web.playdropt.io/chain.pem");
            m_serverCertificate = File.ReadAllText("/etc/letsencrypt/live/web.playdropt.io/cert.pem");
            m_serverPrivateKey = File.ReadAllText("/etc/letsencrypt/live/web.playdropt.io/privkey.pem");

            Debug.Log("Certificates loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading certificates: {e.Message}");
        }
    }

    private async void ConnectClientGame()
    {
        var response = await ServerManagerAgent.Instance.JoinEmpty();

        if (response == null)
        {
            Debug.LogError("Failed to join empty instance.");
            return;
        }

        // set IP address and port
        Bootstrap.Instance.IpAddress = response.ipAddress;
        Bootstrap.Instance.Port = ushort.Parse(response.nodePort);

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
    }

    private void ConnectServerGame()
    {
        // set if using encryption
        m_transport.UseEncryption = Bootstrap.IsRemoteConnection();

        // set connection data
        m_transport.SetConnectionData(Bootstrap.Instance.IpAddress, Bootstrap.Instance.Port, "0.0.0.0");
        if (m_transport.UseEncryption)
        {
            m_transport.SetServerSecrets(m_serverCertificate, m_serverPrivateKey);
        }

        // start server
        NetworkManager.Singleton.StartServer();
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
}
