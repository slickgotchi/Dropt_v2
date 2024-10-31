using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

namespace Dropt.Utils
{
    public static class Http
    {
        public static async UniTask<string> PostRequest(string url, string json)
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
                    return null; // Return null in case of exception
                }
            }
        }

        public static async UniTask<string> GetRequest(string url)
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
    }
}
