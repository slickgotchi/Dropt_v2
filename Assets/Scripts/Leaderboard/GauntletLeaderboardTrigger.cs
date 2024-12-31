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
            var playerControllers = Game.Instance.playerControllers;

            foreach (var pc in playerControllers)
            {
                var pll = pc.GetComponent<PlayerLeaderboardLogger>(); ;
                pll.dungeonType = LeaderboardLogger.DungeonType.Gauntlet;
            }
        }
    }
}
