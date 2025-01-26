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

        public static async UniTask<string> PostEncryptedRequest(string url, string json, string secretKey = "")
        {
            var encryptedJson = EncryptPayload(json, secretKey);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(encryptedJson);
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

        public static string EncryptPayload(string jsonPayload, string secret)
        {
            // Add nonce and timestamp to payload
            var enhancedPayload = new
            {
                data = jsonPayload,
                nonce = Bootstrap.Instance.GameId + Interlocked.Increment(ref Http.nonce).ToString(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            string enhancedJson = JsonConvert.SerializeObject(enhancedPayload); // Use Newtonsoft for better serialization

            using (Aes aes = Aes.Create())
            {
                // Derive key using PBKDF2
                using (var deriveBytes = new Rfc2898DeriveBytes(secret, Encoding.UTF8.GetBytes("YourSaltHere"), 10000))
                {
                    aes.Key = deriveBytes.GetBytes(32); // AES-256
                }

                aes.GenerateIV(); // Generate a new IV for every encryption
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt payload
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to ciphertext

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(enhancedJson);
                    }

                    // Add HMAC for integrity check
                    using (HMACSHA256 hmac = new HMACSHA256(aes.Key))
                    {
                        byte[] hash = hmac.ComputeHash(ms.ToArray());
                        ms.Write(hash, 0, hash.Length);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

    }
}
