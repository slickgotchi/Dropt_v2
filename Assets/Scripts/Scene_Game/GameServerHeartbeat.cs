using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class GameServerHeartbeat : MonoBehaviour
{
    public static GameServerHeartbeat Instance { get; private set; }

    private int activePlayers = 0;
    //private string nodeServerUrl = "https://alphaserver.playdropt.io/serverheartbeat"; // Change to your Node.js server URL
    private string serverManagerUri = "http://103.253.146.245:3000";
    private float m_timer = 3.0f; // 3 seconds interval
    private float k_heartbeatInterval = 3f;
    public bool IsPublic = false;

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
        }

        startTime = DateTime.UtcNow;
    }

    private void Update()
    {
        return;
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
        // player count
        activePlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;

        // running time
        TimeSpan runningTime = DateTime.UtcNow - startTime;
        int hours = runningTime.Hours;
        int minutes = runningTime.Minutes;
        int seconds = runningTime.Seconds;
        string formattedRunningTime = $"{hours}h {minutes}m {seconds}s";

        // setup post data
        var postData = new ServerHeartbeatPostData
        {
            gameId = Bootstrap.Instance.GameId,
            playerCount = activePlayers,
            runningTime = formattedRunningTime,
        };

        string json = JsonUtility.ToJson(postData);

        await PostRequest(serverManagerUri + "/instanceheartbeat", json);
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
    struct ServerHeartbeatPostData
    {
        public string gameId;
        public int playerCount;
        public string runningTime;
    }
}
