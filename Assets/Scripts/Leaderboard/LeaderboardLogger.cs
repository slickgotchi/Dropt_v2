using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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

        Debug.Log("LogEndOfDungeonResults: Got Player");

        // Extract data from PlayerController
        var gotchiId = playerController.NetworkGotchiId.Value;
        Debug.Log("gotchiId: " + gotchiId);
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        Debug.Log("gotchiData: " + gotchiData);
        var gotchiName = gotchiData.name;
        Debug.Log("gotchiName: " + gotchiName);
        var walletAddress = playerController.ConnectedWallet;
        Debug.Log("walletAddress: " + walletAddress);
        var playerOffchainData = playerController.GetComponent<PlayerOffchainData>();
        Debug.Log("playerOffchainData: " + playerOffchainData);
        var formation = playerOffchainData?.dungeonFormation ?? "unknown";
        var dustBalance = playerOffchainData?.GetEctoDeltaValue() ?? 0;
        var kills = playerController.GetTotalKilledEnemies();
        var completionTime = (int)Time.timeSinceLevelLoad; // Example completion time in seconds

        Debug.Log("LogEndOfDungeonResults: Create leaderboard entry");
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

        try
        {
            if (dungeonType == DungeonType.Adventure)
            {
                if (isEscaped)
                {
                    Debug.Log("HandleAdventureLogging: Try");
                    await HandleAdventureLogging(leaderboardEntry);
                    Debug.Log("HandleAdventureLogging: Success");
                }
            }
            else
            {
                Debug.Log("HandleGauntletLogging: Try");
                await HandleGauntletLogging(leaderboardEntry);
                Debug.Log("HandleGauntletLogging: Success");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }

    }

    private static async UniTask HandleAdventureLogging(LeaderboardEntry entry)
    {
        try
        {
            var existingEntry = await GetLeaderboardEntry("adventure_leaderboard", entry.gotchi_id);

            if (existingEntry == null ||
                entry.dust_balance > existingEntry.dust_balance ||
                (entry.dust_balance == existingEntry.dust_balance && entry.kills > existingEntry.kills) ||
                (entry.dust_balance == existingEntry.dust_balance && entry.kills == existingEntry.kills && entry.completion_time < existingEntry.completion_time))
            {
                await UpdateLeaderboardEntry("adventure_leaderboard", entry);
            }
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
            var existingEntry = await GetLeaderboardEntry("gauntlet_leaderboard", entry.gotchi_id);

            if (existingEntry == null ||
                entry.kills > existingEntry.kills ||
                (entry.kills == existingEntry.kills && entry.completion_time < existingEntry.completion_time) ||
                (entry.kills == existingEntry.kills && entry.completion_time == existingEntry.completion_time && entry.dust_balance > existingEntry.dust_balance))
            {
                await UpdateLeaderboardEntry("gauntlet_leaderboard", entry);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Gauntlet leaderboard update failed: {e.Message}");
        }
    }

    private static async UniTask<LeaderboardEntry> GetLeaderboardEntry(string leaderboard, int gotchiId)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"{leaderboardDbUri}/{leaderboard}/{gotchiId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LeaderboardEntry>(json);
            }
            return null;
        }
    }

    private static async UniTask UpdateLeaderboardEntry(string leaderboard, LeaderboardEntry entry)
    {
        using (var client = new HttpClient())
        {
            var json = JsonConvert.SerializeObject(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{leaderboardDbUri}/{leaderboard}", content);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Failed to update {leaderboard}: {response.ReasonPhrase}");
            }
        }
    }

    /// <summary>
    /// Fetches all leaderboard entries for a given leaderboard.
    /// </summary>
    /// <param name="leaderboard">The leaderboard name ("adventure_leaderboard" or "gauntlet_leaderboard").</param>
    /// <returns>A list of leaderboard entries.</returns>
    public static async UniTask<List<LeaderboardEntry>> GetAllLeaderboardEntries(string leaderboard)
    {
        if (string.IsNullOrEmpty(leaderboard))
        {
            Debug.LogError("Leaderboard name cannot be null or empty.");
            return null;
        }

        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync($"{leaderboardDbUri}/{leaderboard}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var entries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
                    Debug.Log($"Successfully fetched {entries.Count} entries from {leaderboard}.");
                    return entries;
                }
                else
                {
                    Debug.LogError($"Failed to fetch leaderboard entries: {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error fetching leaderboard entries: {e.Message}");
                return null;
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
}
