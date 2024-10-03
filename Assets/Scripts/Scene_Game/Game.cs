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

    private string serverManagerUri = "http://103.253.146.245:3000";

    UnityTransport m_transport;

    // certificate variables for wss and encryption
    // IMPORTANT: The only certs that seem to work correctly when deployed in browser
    // are those for the actual website (web.playdropt.io)
    // and the ones that are used to secure the website itself (letsencrypt)
    private string m_serverCommonName;
    private string m_clientCA;
    private string m_serverCertificate;
    private string m_serverPrivateKey;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 2. ensure we have access to UnityTransport
        m_transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;
        if (m_transport == null) { Debug.Log("Could not get UnityTransport"); return; }

        // 3. Server instances
        if (Bootstrap.IsServer())
        {
            Application.targetFrameRate = 15;
            QualitySettings.vSyncCount = 0;

            if (Bootstrap.IsRemoteConnection() && Bootstrap.IsUseServerManager())
            {
                m_serverCertificate = System.Environment.GetEnvironmentVariable("DROPT_SERVER_CERTIFICATE");
                m_serverPrivateKey = System.Environment.GetEnvironmentVariable("DROPT_SERVER_PRIVATE_KEY");
            }
        }

        // 4. Client instances
        else if (Bootstrap.IsClient())
        {
            if (Bootstrap.Instance.UseServerManager)
            {

            }
            else
            {

            }
        }

        // 5. Host instances
        else if (Bootstrap.IsHost())
        {
        }
    }

    private void Update()
    {

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
        }
        else if (Bootstrap.IsHost())
        {
            LevelManager.Instance.GoToDegenapeVillageLevel();
        }
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
            }
        }
        else if (Bootstrap.IsClient() || Bootstrap.IsHost())
        {
            m_transport.SetConnectionData(ipAddress, Bootstrap.Instance.Port);
            if (m_transport.UseEncryption)
            {
                m_transport.SetClientSecrets(m_serverCommonName, m_clientCA);
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

    private async UniTask JoinEmpty()
    {
        try
        {
            var joinEmptyPostData = new JoinEmpty_PostData { };
            string json = JsonUtility.ToJson(joinEmptyPostData);

            var responseString = await PostRequest(serverManagerUri + "/joinempty", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return;

            // Parse the response string into the JoinEmpty_ResponseData struct
            JoinEmpty_ResponseData responseData = JsonUtility.FromJson<JoinEmpty_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log($"Game ID: {responseData.gameId}");
            Debug.Log($"IP Address: {responseData.ipAddress}");
            Debug.Log($"Node Port: {responseData.nodePort}");

            // now send join instance message to game
            var gameUri = "http://" + responseData.ipAddress + ":" + responseData.nodePort;
            var joinInstancePostData = new JoinInstance_PostData { gameId = responseData.gameId };
            json = JsonUtility.ToJson(joinInstancePostData);

            responseString = await PostRequest(gameUri + "/joininstance", json);

            Debug.Log(responseString);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    // POST function for 
    private async UniTask<string> PostRequest(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest();

                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        Debug.Log("PostRequest() success");
                        return request.downloadHandler.text; // Return the response content
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                    default:
                        Debug.LogError($"PostRequest() error: {request.error}");
                        return null; // Return null or an error message as appropriate
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
                return null; // Return null in case of exception
            }
        }
    }

    [System.Serializable]
    struct JoinEmpty_PostData
    {

    }

    [System.Serializable]
    struct JoinEmpty_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string nodePort;
    }

    [System.Serializable]
    struct JoinInstance_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    struct JoinInstance_ResponseData
    {
        public string message;
    }
}