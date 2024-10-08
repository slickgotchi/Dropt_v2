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


public class GameServerHeartbeat : MonoBehaviour
{
    public static GameServerHeartbeat Instance { get; private set; }

    private int m_playerCount = 0;
    private string serverManagerUri = "http://103.253.146.245:3000";
    private float m_timer = 3.0f; // 3 seconds interval
    private float k_heartbeatInterval = 5f;
    public bool IsPublic = false;
    private bool m_isFinished = false;

    private DateTime startTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Bootstrap.IsClient() || Bootstrap.IsHost() || (Bootstrap.IsServer() && Bootstrap.IsLocalConnection()))
        {
            Destroy(gameObject);
            return;
        }

        startTime = DateTime.UtcNow;


    }

    private void Update()
    {
        if (!Bootstrap.IsUseServerManager()) return;

        m_timer -= Time.deltaTime;

        if (m_timer <= 0)
        {
            SendServerHeartbeat().Forget(); // Properly calling the async method
            m_timer = k_heartbeatInterval; // Reset the timer
        }
    }

    private async UniTaskVoid SendServerHeartbeat()
    {
        if (m_isFinished) return;

        // player count
        m_playerCount = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;

        string HEARTBEAT_SECRET = EnvLoader.GetEnv("HEARTBEAT_SECRET");
        if (HEARTBEAT_SECRET == null)
        {
            Debug.LogError("Ensure there is a .env file with a HEARTBEAT_SECRET in the same folder as the .x86_64");
            return;
        }

        try
        {
            // 1. generate nonce
            byte[] nonceBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonceBytes);
            }
            StringBuilder nonceBuilder = new StringBuilder();
            foreach (byte b in nonceBytes)
            {
                nonceBuilder.Append(b.ToString("x2"));
            }
            string nonce = nonceBuilder.ToString();
            Debug.Log("Made a nonce: " + nonce);

            // 2. create payload
            var payload = new
            {
                gameId = Bootstrap.Instance.GameId,
                playerCount = m_playerCount,
                nonce = nonce,
                exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() // Expiration time
            };

            // 3. Serialize payload to JSON
            string payloadJson = JsonConvert.SerializeObject(payload);
            string payloadBase64 = ToUrlSafeBase64String(Encoding.UTF8.GetBytes(payloadJson));

            // 4. Create JWT header
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };
            string headerJson = JsonConvert.SerializeObject(header);
            string headerBase64 = ToUrlSafeBase64String(Encoding.UTF8.GetBytes(headerJson));

            // 5. Create the unsigned token
            string unsignedToken = $"{headerBase64}.{payloadBase64}";

            // 6. Sign the token
            string signature;
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(HEARTBEAT_SECRET)))
            {
                byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
                signature = ToUrlSafeBase64String(signatureBytes);
            }

            // 7. Create the final token
            string token = $"{unsignedToken}.{signature}";
            Debug.Log("Made a token: " + token);

            // 8. Create HTTP request
            var instanceHeartbeatPostData = new InstanceHeartbeat_PostData
            {
                gameId = Bootstrap.Instance.GameId,
                playerCount = m_playerCount,
                nonce = nonce,
                token = token,
            };
            string json = JsonUtility.ToJson(instanceHeartbeatPostData);
            var workerUri = "http://" + Bootstrap.Instance.IpAddress + ":" + Bootstrap.Instance.WorkerPort;
            var responseStr = await PostRequest(workerUri + "/instanceheartbeat", json);

            Debug.Log("/instancehearbeat success");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error sending hearbeat: " + e.Message);
        }
    }

    private string ToUrlSafeBase64String(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
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
    struct InstanceHeartbeat_PostData
    {
        public string gameId;
        public int playerCount;
        public string nonce;
        public string token;
    }
}
