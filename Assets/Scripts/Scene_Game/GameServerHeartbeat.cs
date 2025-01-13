using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using Newtonsoft.Json; // For handling JSON
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Unity.Mathematics;


public class GameServerHeartbeat : MonoBehaviour
{
    public static GameServerHeartbeat Instance { get; private set; }

    private int m_playerCount = 0;
    private float m_heartbeatTimer = 0f; 
    private float k_heartbeatInterval = 2f;
    public bool IsPublic = false;

    private bool m_isFirstPlayerJoined = false;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_isFirstPlayerJoined = false;
    }

    private void Start()
    {
        if (Bootstrap.IsClient() || Bootstrap.IsHost() || (Bootstrap.IsServer() && Bootstrap.IsLocalConnection()))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (!Bootstrap.IsUseServerManager()) return;

        m_heartbeatTimer -= Time.deltaTime;

        if (m_heartbeatTimer <= 0)
        {
            SendServerHeartbeat().Forget(); // Properly calling the async method
            m_heartbeatTimer = k_heartbeatInterval; // Reset the timer
        }
    }

    private async UniTaskVoid SendServerHeartbeat()
    {
        m_playerCount = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;

        if (m_playerCount > 0)
        {
            m_isFirstPlayerJoined = true;
        }

        string HEARTBEAT_SECRET = Bootstrap.Instance.HeartbeatSecret;
        if (string.IsNullOrEmpty(HEARTBEAT_SECRET))
        {
            Debug.LogError("Ensure there is a .env file with a HEARTBEAT_ENCRYPTION_SECRET in the same folder as the .x86_64");
            return;
        }

        try
        {
            var playerCount = m_playerCount;

            // determine the status of our server
            string status = Game.Instance.isServerReady ? "ready" : "starting";
            if (m_isFirstPlayerJoined && m_playerCount <= 0) status = "destroying";
            if (m_playerCount >= 3) status = "full";

            // Create the payload
            var payload = new
            {
                gameId = Bootstrap.Instance.GameId,
                playerCount = playerCount,
                isPublic = IsPublic,
                isVillage = LevelManager.Instance.IsDegenapeVillage(),
                status = status,
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Encrypt the payload
            string encryptedPayload = EncryptPayload(jsonPayload, HEARTBEAT_SECRET);

            // Send the encrypted payload to worker.js
            var postData = new PostData { encryptedPayload = encryptedPayload };
            string json = JsonConvert.SerializeObject(postData);
            var workerUri = "http://" + Bootstrap.Instance.IpAddress + ":" + Bootstrap.Instance.WorkerPort;
            var responseStr = await PostRequest(workerUri + "/gameheartbeat", json);

            //Debug.Log($"/gameheartbeat success, gameId: {payload.gameId}, playerCount: {m_playerCount}, isPublic: {IsPublic}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error sending heartbeat: " + e.Message);
        }
    }


    private string EncryptPayload(string jsonPayload, string secret)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(secret));
            aes.GenerateIV();

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to the ciphertext
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(jsonPayload);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

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

    [Serializable]
    struct PostData
    {
        public string encryptedPayload;
    }
}
