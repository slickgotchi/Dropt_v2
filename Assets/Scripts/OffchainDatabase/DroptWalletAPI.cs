using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class DroptWalletAPI
{
    public static string URI =  "https://alphadb.playdropt.io";

    public static async UniTask<string> CreateDroptWalletDataAsync(DroptWalletData data, string apiUrl = "")
    {
        if (apiUrl == "") apiUrl = URI;

        string jsonData = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/create", "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest().ToUniTask();

            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
        }
    }

    public static async UniTask<DroptWalletData> FetchDroptWalletDataAsync(string walletAddress, string apiUrl = "")
    {
        if (apiUrl == "") apiUrl = URI;

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/get?walletAddress=" + walletAddress))
        {
            var operation = request.SendWebRequest().ToUniTask();

            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                DroptWalletData data = JsonUtility.FromJson<DroptWalletData>(jsonResult);
                return data;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
        }
    }

    public static async UniTask<string> UpdateDroptWalletDataAsync(DroptWalletData data, string apiUrl = "")
    {
        if (apiUrl == "") apiUrl = URI;

        string jsonData = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/update", "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest().ToUniTask();

            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
        }
    }
}
