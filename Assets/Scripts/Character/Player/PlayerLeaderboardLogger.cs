using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

public class PlayerLeaderboardLogger : NetworkBehaviour
{
    //public enum DungeonType { Adventure, Gauntlet }
    public LeaderboardLogger.DungeonType dungeonType = LeaderboardLogger.DungeonType.Adventure;
}