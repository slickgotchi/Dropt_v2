using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GauntletLeaderboardTrigger : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // find all players and set their gauntlet trigger

        if (IsServer)
        {
            var playerLeaderboardLoggers = FindObjectsByType<PlayerLeaderboardLogger>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var pll in playerLeaderboardLoggers)
            {
                pll.dungeonType = PlayerLeaderboardLogger.DungeonType.Gauntlet;
            }
        }
    }
}
