using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

public class ServerManagerAgent : MonoBehaviour
{
    public static ServerManagerAgent Instance { get; private set; }

    private string m_serverManagerUri = "https://manager.playdropt.io";

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        m_serverManagerUri = "https://manager.playdropt.io";
    }

    // /joinempty
    public async UniTask<GetGame_ResponseData> GetGame(string gameId, string region)
    {
        try
        {
            // setup post data and post request
            var getGamePostData = new GetGame_PostData { gameId = gameId, region = region };
            string json = JsonUtility.ToJson(getGamePostData);
            var responseString = await PostRequest(m_serverManagerUri + "/getgame", json);
            Debug.Log(responseString);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            GetGame_ResponseData responseData = JsonUtility.FromJson<GetGame_ResponseData>(responseString);

            // return response
            return responseData;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async UniTask<string> PostRequest(string url, string json)
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
                        Debug.LogError($"GetRequest() error: {request.error}");
                        return null; // Return null or an error message as appropriate
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception: {e.Message}");
                ErrorDialogCanvas.Instance.Show(e.Message);
                return null; // Return null in case of exception
            }
        }
    }

    public async UniTask<string> GetRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
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
                        Debug.LogError($"GetRequest() error: {request.error}");
                        return null; // Return null or an error message as appropriate
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception: {e.Message}");
                return null; // Return null in case of exception
            }
        }
    }

    [System.Serializable]
    public class GetGame_PostData
    {
        public string gameId;
        public string region;
    }

    [System.Serializable]
    public class GetGame_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string gamePort;
        public string commonName;
        public string clientCA;
        public int responseCode;
        public string message;
    }

    [System.Serializable]
    public class JoinGame_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class JoinGame_ResponseData
    {
        public string message;
    }

    [System.Serializable]
    public class LeaveGame_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class LeaveGame_ResponseData
    {
        public string message;
    }

    [System.Serializable]
    public class GetExisting_PostData
    {
        public string gameId;
    }



    [System.Serializable]
    public class LeaveExisting_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class LeaveExisting_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string gamePort;
    }
}
