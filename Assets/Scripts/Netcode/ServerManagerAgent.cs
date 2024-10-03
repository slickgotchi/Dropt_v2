using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

public class ServerManagerAgent : MonoBehaviour
{
    public static ServerManagerAgent Instance { get; private set; }

    public string serverManagerUri = "http://103.253.146.245:3000";

    private bool isTestGameJS = false;

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
    public async UniTask<JoinEmpty_ResponseData> JoinEmpty()
    {
        try
        {
            // setup post data and post request
            var joinEmptyPostData = new JoinEmpty_PostData { };
            string json = JsonUtility.ToJson(joinEmptyPostData);
            var responseString = await PostRequest(serverManagerUri + "/joinempty", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            JoinEmpty_ResponseData responseData = JsonUtility.FromJson<JoinEmpty_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log("/joinempty success");
            Debug.Log(responseData);

            if (isTestGameJS)
            {
                // setup join instance post data and post request
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.nodePort;
                var joinInstancePostData = new JoinInstance_PostData { gameId = responseData.gameId };
                json = JsonUtility.ToJson(joinInstancePostData);
                responseString = await PostRequest(gameUri + "/joininstance", json);

                // Check if the response is not null
                if (string.IsNullOrEmpty(responseString)) return null;

                // success
                Debug.Log("/joininstance success");
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
    public async UniTask<JoinExisting_ResponseData> JoinExisting(string gameId)
    {
        try
        {
            // setup join existing and post request
            var joinExistingPostData = new JoinExisting_PostData { gameId = gameId };
            string json = JsonUtility.ToJson(joinExistingPostData);
            var responseString = await PostRequest(serverManagerUri + "/joinexisting", json);

            // Check if the response is not null
            if (string.IsNullOrEmpty(responseString)) return null;

            // Parse the response string into the JoinEmpty_ResponseData struct
            JoinExisting_ResponseData responseData = JsonUtility.FromJson<JoinExisting_ResponseData>(responseString);

            // Now you can access the fields in responseData
            Debug.Log("/joinexisting success");
            Debug.Log(responseData);

            if (isTestGameJS)
            {
                // now send join instance message to game
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.nodePort;
                var joinInstancePostData = new JoinInstance_PostData { gameId = responseData.gameId };
                json = JsonUtility.ToJson(joinInstancePostData);
                responseString = await PostRequest(gameUri + "/joininstance", json);

                // Check if the response is not null
                if (string.IsNullOrEmpty(responseString)) return null;

                // success
                Debug.Log("/joininstance success");
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
                var gameUri = "http://" + responseData.ipAddress + ":" + responseData.nodePort;
                var leaveInstancePostData = new LeaveInstance_PostData { gameId = responseData.gameId };
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
    public class JoinEmpty_PostData
    {

    }

    [System.Serializable]
    public class JoinEmpty_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string nodePort;
    }

    [System.Serializable]
    public class JoinInstance_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class JoinInstance_ResponseData
    {
        public string message;
    }

    [System.Serializable]
    public class LeaveInstance_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class LeaveInstance_ResponseData
    {
        public string message;
    }

    [System.Serializable]
    public class JoinExisting_PostData
    {
        public string gameId;
    }

    [System.Serializable]
    public class JoinExisting_ResponseData
    {
        public string gameId;
        public string ipAddress;
        public string nodePort;
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
        public string nodePort;
    }
}
