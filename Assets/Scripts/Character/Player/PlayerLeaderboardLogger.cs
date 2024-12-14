using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public class PlayerLeaderboardLogger : NetworkBehaviour
{
    public enum DungeonType { Adventure, Gauntlet }
    public DungeonType dungeonType = DungeonType.Adventure;

    private string dbUri = "https://db.playdropt.io";

    public async void LogEndOfDungeonResults(bool isEscaped)
    {
        if (!IsServer) return;

        var playerController = GetComponent<PlayerController>();
        var playerOffchainData = GetComponent<PlayerOffchainData>();

        var gotchiId = GetComponent<PlayerController>().NetworkGotchiId.Value;
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        var gotchiName = gotchiData.name;
        var walletAddress = playerController.ConnectedWallet;
        var formation = playerOffchainData.dungeonFormation;
        var dustBalance = playerOffchainData.GetEctoDeltaValue();
        var kills = playerController.GetTotalKilledEnemies();
        var completionTime = (int)Time.timeSinceLevelLoad; // Example completion time in seconds

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

        if (dungeonType == DungeonType.Adventure)
        {
            if (isEscaped)
                await HandleAdventureLogging(leaderboardEntry);
        }
        else
        {
            await HandleGauntletLogging(leaderboardEntry);
        }
    }

    private async UniTask HandleAdventureLogging(LeaderboardEntry entry)
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

    private async UniTask HandleGauntletLogging(LeaderboardEntry entry)
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

    private async UniTask<LeaderboardEntry> GetLeaderboardEntry(string leaderboard, int gotchiId)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"{dbUri}/{leaderboard}/{gotchiId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LeaderboardEntry>(json);
            }
            return null;
        }
    }

    private async UniTask UpdateLeaderboardEntry(string leaderboard, LeaderboardEntry entry)
    {
        using (var client = new HttpClient())
        {
            var json = JsonConvert.SerializeObject(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{dbUri}/{leaderboard}", content);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Failed to update {leaderboard}: {response.ReasonPhrase}");
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