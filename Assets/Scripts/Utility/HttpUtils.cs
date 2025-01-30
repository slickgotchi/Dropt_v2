using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace Dropt.Utils
{
    public static class Http
    {
        public static int nonce = 0;

        public static string web3authUri = "https://db.playdropt.io/web3auth";

        public static async UniTask<string> PostEncryptedRequest(string url, string json, string secretKey = "")
        {
            var encryptedPayload = EncryptPayload(json, secretKey);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(encryptedPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    var operation = await request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await UniTask.Yield();
                    }

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
                    var operation = await request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await UniTask.Yield();
                    }

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

        public static async UniTask<string> GetAddressByAuthToken(string token)
        {
            try
            {
                var url = web3authUri + "/getaddressbytoken";

                using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                {
                    var addressRequest = new AddressRequest
                    {
                        token = token
                    };
                    string jsonPayload = JsonUtility.ToJson(addressRequest);

                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    var operation = await request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await UniTask.Yield();
                    }

                    switch (request.result)
                    {
                        case UnityWebRequest.Result.Success:
                            var response = JsonUtility.FromJson<AddressResponse>(request.downloadHandler.text);
                            if (!string.IsNullOrEmpty(response.address))
                            {
                                return response.address;
                            }
                            break;
                        default:
                            Debug.LogError($"Request error: {request.error}");
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }

            return null;
        }

        public static string EncryptPayload(string jsonPayload, string secret)
        {
            // Add nonce and timestamp to payload
            var enhancedPayload = new EnhancedData
            {
                data = jsonPayload,
                nonce = Bootstrap.Instance.GameId + Interlocked.Increment(ref Http.nonce).ToString(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Serialize the enhanced payload
            string enhancedJson = JsonUtility.ToJson(enhancedPayload);

            // Use HMACSHA256 to encrypt the payload with the secret key
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                // Compute hash of the payload
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enhancedJson));

                // Combine the original JSON payload and its hash as a single output
                var result = new SignedPayload
                {
                    payload = enhancedJson,
                    signature = Convert.ToBase64String(hash)
                };

                // Return the final JSON string containing both payload and signature
                return JsonUtility.ToJson(result);
            }
        }

        [System.Serializable]
        public struct EnhancedData
        {
            public string data;
            public string nonce;
            public long timestamp;
            public string signature;
        }

        [System.Serializable]
        public struct SignedPayload
        {
            public string payload;
            public string signature;
        }

        [System.Serializable]
        struct AddressRequest
        {
            public string token;
        }

        [System.Serializable]
        struct AddressResponse
        {
            public string address;
        }
    }
}
