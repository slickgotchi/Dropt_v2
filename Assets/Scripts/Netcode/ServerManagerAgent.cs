using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

public class ServerManagerAgent : MonoBehaviour
{
    public static ServerManagerAgent Instance { get; private set; }

    public string serverManagerUri = "http://103.253.146.245:3000";

    private bool isTestGameJS = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // /joinempty
    public async UniTask<GetEmpty_ResponseData> GetEmpty(string region)
    {
        try
        {
            // setup post data and post request
            var getEmptyPostData = new GetEmpty_PostData { region = region };
            string json = JsonUtility.ToJson(getEmptyPostData);
            var responseString = await PostRequest(serverManagerUri + "/getempty", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            GetEmpty_ResponseData responseData = JsonUtility.FromJson<GetEmpty_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log("/getempty success");
            Debug.Log(responseData);

            if (isTestGameJS)
            {
                // setup join instance post data and post request
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.gamePort;
                var joinGamePostData = new JoinGame_PostData { gameId = responseData.gameId };
                json = JsonUtility.ToJson(joinGamePostData);
                responseString = await PostRequest(gameUri + "/joingame", json);

                // Check if the response is not null
                if (string.IsNullOrEmpty(responseString)) return null;

                // success
                Debug.Log("/joingame success");
                Debug.Log(responseString);
            }

            return responseData;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    // /joinexisting
    public async UniTask<GetExisting_ResponseData> GetExisting(string gameId)
    {
        try
        {
            // setup join existing and post request
            var getExistingPostData = new GetExisting_PostData { gameId = gameId };
            string json = JsonUtility.ToJson(getExistingPostData);
            var responseString = await PostRequest(serverManagerUri + "/getexisting", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            GetExisting_ResponseData responseData = JsonUtility.FromJson<GetExisting_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log("/getexisting success");
            Debug.Log(responseData);

            if (isTestGameJS)
            {
                // now send join instance message to game
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.gamePort;
                var joinGamePostData = new JoinGame_PostData { gameId = responseData.gameId };
                json = JsonUtility.ToJson(joinGamePostData);
                responseString = await PostRequest(gameUri + "/joingame", json);

                // Check if the response is not null
                if (string.IsNullOrEmpty(responseString)) return null;

                // success
                Debug.Log("/joingame success");
                Debug.Log(responseString);
            }

            return responseData;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    // /leaveexisting
    public async UniTask<LeaveExisting_ResponseData> LeaveExisting(string gameId)
    {
        return null;
        try
        {
            // create post data and post leave exsting request
            var leaveExistingPostData = new LeaveExisting_PostData { gameId = gameId };
            string json = JsonUtility.ToJson(leaveExistingPostData);
            var responseString = await PostRequest(serverManagerUri + "/leaveexisting", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            LeaveExisting_ResponseData responseData = JsonUtility.FromJson<LeaveExisting_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log("/leaveexisting success");
            Debug.Log(responseData);

            if (isTestGameJS)
            {
                // now send join instance message to game
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.gamePort;
                var leaveInstancePostData = new LeaveGame_PostData { gameId = responseData.gameId };
                json = JsonUtility.ToJson(leaveInstancePostData);
                responseString = await PostRequest(gameUri + "/leaveinstance", json);

                // Check if the response is not null
                if (string.IsNullOrEmpty(responseString)) return null;

                // success
                Debug.Log("/leaveinstance success");
                Debug.Log(responseString);
            }

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
                        Debug.LogError($"PostRequest() error: {request.error}");
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
    public class GetEmpty_PostData
    {
        public string region;
    }

    [System.Serializable]
    public class GetEmpty_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string gamePort;
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
    public class GetExisting_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string gamePort;
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
