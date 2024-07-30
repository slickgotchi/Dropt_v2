using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GameServerHeartbeat : MonoBehaviour
{
    public static GameServerHeartbeat Instance { get; private set; }

    private int activePlayers = 0;
    public string nodeServerUrl = "http://localhost:3000/serverheartbeat"; // Change to your Node.js server URL
    private float m_timer = 5.0f; // 5 seconds interval

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (!Bootstrap.Instance.UseServerManager) Destroy(gameObject);
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0)
        {
            SendServerHeartbeat();
            m_timer = 5.0f; // Reset the timer to 5 seconds
        }
    }

    private void SendServerHeartbeat()
    {
        activePlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;

        var postData = new
        {
            port = Bootstrap.Instance.Port,
            ipAddress = Bootstrap.Instance.IpAddress, // Assuming local IP, adjust as needed
            gameId = Bootstrap.Instance.GameId,
            numberPlayers = activePlayers
        };

        string json = JsonUtility.ToJson(postData);

        PostRequest(nodeServerUrl, json);
    }

    private async void PostRequest(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Heartbeat sent successfully");
            }
            else
            {
                Debug.LogError($"Error sending heartbeat: {request.error}");
            }
        }
    }
}
