using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class GameServerHeartbeat : MonoBehaviour
{
    public static GameServerHeartbeat Instance { get; private set; }

    private int activePlayers = 0;
    private string nodeServerUrl = "https://alphaserver.playdropt.io/serverheartbeat"; // Change to your Node.js server URL
    private float m_timer = 5.0f; // 5 seconds interval
    public bool IsPublic = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Bootstrap.IsClient() || Bootstrap.IsHost() || (Bootstrap.IsServer() && Bootstrap.IsLocalConnection()))
        {
            Destroy(gameObject); 
        }
    }

    private void Update()
    {
        if (!Bootstrap.IsUseServerManager()) return;

        m_timer -= Time.deltaTime;

        if (m_timer <= 0)
        {
            SendServerHeartbeat().Forget(); // Properly calling the async method
            m_timer = 5.0f; // Reset the timer to 5 seconds
        }
    }

    private async UniTaskVoid SendServerHeartbeat()
    {
        activePlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;

        var postData = new ServerHeartbeatPostData
        {
            port = Bootstrap.Instance.Port,
            ipAddress = Bootstrap.Instance.IpAddress, // Assuming local IP, adjust as needed
            gameId = Bootstrap.Instance.GameId,
            numberPlayers = activePlayers,
            isPublic = IsPublic,
            isLocked = LevelManager.Instance.CurrentLevelIndex.Value != LevelManager.Instance.DegenapeVillageLevel,
        };

        string json = JsonUtility.ToJson(postData);

        await PostRequest(nodeServerUrl, json);
    }

    private async UniTask PostRequest(string url, string json)
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
                        Debug.Log("Heartbeat sent successfully with isPublic: " + IsPublic);
                        Debug.Log(json);
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"Error sending heartbeat: {request.error}");
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError($"HTTP Error: {request.error}");
                        break;
                    default:
                        Debug.LogError($"Unexpected Error: {request.error}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
            }
        }
    }

    [Serializable]
    struct ServerHeartbeatPostData
    {
        public ushort port;
        public string ipAddress;
        public string gameId;
        public int numberPlayers;
        public bool isPublic;
        public bool isLocked;
    }
}
