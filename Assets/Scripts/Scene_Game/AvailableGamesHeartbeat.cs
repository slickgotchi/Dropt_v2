using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class AvailableGamesHeartbeat : MonoBehaviour
{
    public static AvailableGamesHeartbeat Instance { get; private set; }

    private float k_heartbeatInterval = 3f;
    private float m_heartbeatTimer = 0f;

    private string getGamesUri = "https://alphaserver.playdropt.io/getgames";

    public List<AvailableGame> AvailableGames = new List<AvailableGame>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (Bootstrap.IsServer()) Destroy(gameObject);
    }

    private void Update()
    {
        //if (!Game.Instance.IsConnected) return;

        m_heartbeatTimer -= Time.deltaTime;

        if (m_heartbeatTimer <= 0)
        {
            m_heartbeatTimer = k_heartbeatInterval;
            GetGames();
        }
    }

    async void GetGames()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(getGamesUri))
        {
            await webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("ConnectionError: Is the ServerManager running and has the correct uri been used?");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log("DataProcessingError: ");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log("ProtocolError");
                    break;
                case UnityWebRequest.Result.Success:
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log("Response: " + jsonResponse);
                    GetGamesResponseData responseData = JsonUtility.FromJson<GetGamesResponseData>(jsonResponse);
                    HandleGetGamesResponse(responseData);
                    break;
            }
        }
    }

    void HandleGetGamesResponse(GetGamesResponseData responseData)
    {
        AvailableGames.Clear();

        foreach (var game in responseData.availableGames)
        {
            AvailableGames.Add(new AvailableGame
            {
                gameId = game.gameId,
                numberPlayers = game.numberPlayers,
                isPublic = game.isPublic,
            });
        }
    }

    [Serializable]
    public class GetGamesResponseData
    {
        public AvailableGame[] availableGames;
    }

    [Serializable]
    public class AvailableGame
    {
        public string gameId;
        public int numberPlayers;
        public bool isPublic;
    }
}
