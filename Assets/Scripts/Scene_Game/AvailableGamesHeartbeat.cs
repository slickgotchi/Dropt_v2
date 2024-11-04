using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Cysharp.Threading.Tasks;

public class AvailableGamesHeartbeat : MonoBehaviour
{
    public static AvailableGamesHeartbeat Instance { get; private set; }

    private float k_heartbeatInterval = 2f;
    private float m_heartbeatTimer = 0f;

    private string getGamesUri = "https://dmanager.playdropt.io/getstatus";

    public List<AvailableGame> AvailableGames = new List<AvailableGame>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (Bootstrap.IsServer() || !Bootstrap.IsUseServerManager()) Destroy(gameObject);
    }

    private void Update()
    {
        ////if (!Game.Instance.IsConnected) return;
        if (!Bootstrap.IsClient()) return;
        if (Bootstrap.IsLocalConnection()) return;
        if (!Bootstrap.IsUseServerManager()) return;

        m_heartbeatTimer -= Time.deltaTime;

        if (m_heartbeatTimer <= 0)
        {
            m_heartbeatTimer = k_heartbeatInterval;
            GetGames();
        }

        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    for (int i = 0; i < AvailableGames.Count; i++)
        //    {
        //        var ag = AvailableGames[i];
        //        Debug.Log("Available gameId: " + ag.gameId + ", isServerReady: " + ag.isServerReady + ", isPublic: " + ag.isPublic);
        //    }
        //}
    }

    public bool IsServerReady(string gameId)
    {
        bool isReady = false;
        for (int i = 0; i < AvailableGames.Count; i++)
        {
            var game = AvailableGames[i];
            if (game.gameId.ToUpper() == gameId.ToUpper())
            {
                if (game.isFirstPlayerJoined) isReady = true;
            }
        }

        return isReady;
    }

    async UniTaskVoid GetGames()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(getGamesUri))
        {
            await webRequest.SendWebRequest().ToUniTask();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("ConnectionError: Is the ServerManager running and has the correct uri been used?");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log("DataProcessingError: ");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    //Debug.Log("ProtocolError");
                    break;
                case UnityWebRequest.Result.Success:
                    string jsonResponse = webRequest.downloadHandler.text;
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
                region = game.region,
                workerIpAddress = game.workerIpAddress,
                gamePort = game.gamePort,
                playerCount = game.playerCount,
                isFirstPlayerJoined = game.isFirstPlayerJoined,
                timeLeft = game.timeLeft,
                workerCpuUsage = game.workerCpuUsage,
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
        public string region;
        public string workerIpAddress;
        public ulong gamePort;
        public int playerCount;
        public bool isFirstPlayerJoined;
        public float timeLeft;
        public float workerCpuUsage;
        public bool isPublic;
    }
}
