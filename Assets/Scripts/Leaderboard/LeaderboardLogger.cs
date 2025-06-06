using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

public static class LeaderboardLogger
{
    private static string leaderboardDbUri = "https://db.playdropt.io/leaderboard";

    public enum DungeonType { Adventure, Gauntlet }

    public static async UniTask LogEndOfDungeonResults(PlayerController playerController, DungeonType dungeonType, bool isEscaped)
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is null. Cannot log leaderboard data.");
            return;
        }

        try
        {
            Debug.Log("LogEndOfDungeonResults()");

            // Extract data from PlayerController
            var gotchiId = playerController.NetworkGotchiId.Value;
            Debug.Log("gotchiId: " + gotchiId);
            if (gotchiId > 25001)
            {
                Debug.Log("Can not log offchain gotchi");
                return;
            }

            // we wrap this call in the event that the graph is down for some reason
            // note: we can always check gotchis against wallets after the fact but
            // we still want to guarantee that scores get logged for gotchis
            PortalDefender.AavegotchiKit.GotchiData gotchiData = null;
            try
            {
                gotchiData = await GotchiHub.GotchiDataManager.Instance.GetGotchiDataFromGraph(gotchiId);
                Debug.Log("GotchiData received");
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            var gotchiName = gotchiData != null ? gotchiData.name : $"Gotchi ID: {gotchiId}";
            Debug.Log("gotchiName: " + gotchiName);

            var walletAddress = gotchiData != null ? gotchiData.owner.id : "TBC";
            Debug.Log("walletAddress: " + walletAddress);

            var playerOffchainData = playerController.GetComponent<PlayerOffchainData>();
            Debug.Log("playerOffchainData: " + playerOffchainData);

            var formation = playerOffchainData?.dungeonFormation ?? "unknown";
            Debug.Log("formation: " + formation);

            var dustBalance = playerOffchainData?.GetDustDeltaValue_PostDungeon() ?? 0;
            Debug.Log("dustBalance: " + dustBalance);

            var kills = playerController.GetTotalKilledEnemies();
            Debug.Log("kills: " + kills);

            var completionTime = (int)Time.timeSinceLevelLoad; // Example completion time in seconds
            Debug.Log("completionTime: " + completionTime);

            var leaderboardEntry = new LeaderboardEntry
            {
                gotchi_id = gotchiId,
                gotchi_name = gotchiName,
                wallet_address = walletAddress,
                formation = formation,
                dust_balance = dustBalance,
                kills = kills,
                completion_time = completionTime
            };

            Debug.Log("Created temporary leaderboardEntry data object");

            if (dungeonType == DungeonType.Adventure)
            {
                Debug.Log("Did we escape?");
                if (isEscaped)
                {
                    Debug.Log("Yes escaped. HandleAdventureLogging");
                    await HandleAdventureLogging(leaderboardEntry);
                }
                else
                {
                    Debug.Log("Did not escape, do not update leaderboard");
                }
            }
            else
            {
                Debug.Log("HandleGauntletLogging");
                await HandleGauntletLogging(leaderboardEntry);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    private static async UniTask HandleAdventureLogging(LeaderboardEntry entry)
    {
        try
        {
            await UpsertLeaderboardEntry("adventure_leaderboard", entry, Bootstrap.Instance.DbSecret);
            Debug.Log("Successfully updated adventure_leaderboard for " + entry.gotchi_name);
        }
        catch (Exception e)
        {
            Debug.LogError($"Adventure leaderboard update failed: {e.Message}");
        }
    }

    private static async UniTask HandleGauntletLogging(LeaderboardEntry entry)
    {
        try
        {
            await UpsertLeaderboardEntry("gauntlet_leaderboard", entry, Bootstrap.Instance.DbSecret);
            Debug.Log("Successfully updated gauntlet_leader for " + entry.gotchi_name);
        }
        catch (Exception e)
        {
            Debug.LogError($"Gauntlet leaderboard update failed: {e.Message}");
        }
    }

    private static async UniTask<LeaderboardEntry> GetLeaderboardEntry(string leaderboard, int gotchiId)
    {
        string url = $"{leaderboardDbUri}/{leaderboard}/{gotchiId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonUtility.FromJson<LeaderboardEntry>(json);
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch leaderboard entry: {request.error}\nResponse: {request.downloadHandler.text}");
            }

            return null;
        }
    }

    private static async UniTask UpsertLeaderboardEntry(string leaderboard, LeaderboardEntry entry, string secretKey)
    {
        string url = $"{leaderboardDbUri}/{leaderboard}/{entry.gotchi_id}";
        string json = JsonUtility.ToJson(entry);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", secretKey); // Add secret key to headers

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to update {leaderboard}: {request.error}\nResponse: {request.downloadHandler.text}");
            }
        }
    }



    public static async UniTask<List<LeaderboardEntry>> GetAllLeaderboardEntries(string leaderboard)
    {
        if (string.IsNullOrEmpty(leaderboard))
        {
            Debug.LogError("Leaderboard name cannot be null or empty.");
            return new List<LeaderboardEntry>(); // Return an empty list instead of null
        }

        string apiUrl = $"{leaderboardDbUri}/{leaderboard}";

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            var operation = request.SendWebRequest().ToUniTask();

            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;

                try
                {
                    // Wrap JSON response in an object if necessary
                    string wrappedJson = "{\"entries\":" + jsonResult + "}";

                    // Parse JSON into a list of LeaderboardEntry
                    Wrapper wrapper = JsonUtility.FromJson<Wrapper>(wrappedJson);

                    if (wrapper != null && wrapper.entries != null)
                    {
                        return wrapper.entries;
                    }
                    else
                    {
                        Debug.LogError("Parsed wrapper or entries were null.");
                        return new List<LeaderboardEntry>();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse leaderboard JSON: {e.Message}\nJSON: {jsonResult}");
                    return new List<LeaderboardEntry>(); // Return empty list on exception
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch leaderboard entries: {request.error}");
                return new List<LeaderboardEntry>(); // Return empty list on failure
            }
        }
    }


    [Serializable]
    public class LeaderboardEntry
    {
        public int gotchi_id;
        public string gotchi_name;
        public string wallet_address;
        public string formation;
        public int dust_balance;
        public int kills;
        public int completion_time;
    }

    // Wrapper class to handle the JSON array format
    [Serializable]
    public class Wrapper
    {
        public List<LeaderboardEntry> entries;
    }
}
