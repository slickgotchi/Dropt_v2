using Unity.Netcode;
using UnityEngine;
using Thirdweb.Unity;
using GotchiHub;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;


// ecto bank logic
// - ecto_balance_offchain is the total ecto you have when in the village
// - ectoDungeonStartAmount_offchain is the amount of ecto from your bank that you can start a dungeon with (automatically available)
// - ectoStartCount_dungeon is the ecto you have at the beginning of a dungeon
// - ectoSpentCount_dungeon is ecto spent in dungeon
// - ectoCollectedCount_dungeon is ecto collected in dungeon
// - players automatically enter dungeon by withdrawing from their bank up to cGhstDungeonStartAmount
// - when players have more than their wallet amount

// what states can we have


// wallet flow
// 1. check player prefs for a saved wallet
// 2. if no saved wallet and !IsHost, mark wallet address as null
// 3. if no saved wallet and IsHost, use the Bootstrap TestWalletAddress as wallet address and save to player prefs
// 4. if a wallet address becomes connected, save it to wallet address
// 5. when a wallet address changes, client tells server to update details


public class PlayerOffchainData : NetworkBehaviour
{
    // wallet data - balances DO NOT change in dungeons, only in the village
    public NetworkVariable<int> m_ectoVillageBalance_wallet = new NetworkVariable<int>(0);

    // wallet data - balances DO change in dungeons and the village
    public NetworkVariable<int> m_bombLiveBalance_wallet = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_portaHoleLiveBalance_wallet = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_zenCricketLiveBalance_wallet = new NetworkVariable<int>(0);

    // gotchi data - in and out of dungeons
    public NetworkVariable<int> m_bombCapacity_gotchi = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_portaHoleCapacity_gotchi = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_zenCricketCapacity_gotchi = new NetworkVariable<int>(0);
    public NetworkVariable<bool> m_isEssenceInfused_gotchi = new NetworkVariable<bool>(false);
    public NetworkVariable<int> m_ectoDungeonStartAmount_gotchi = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_dustVillageBalance_gotchi = new NetworkVariable<int>(0);

    // dungeon data - dungeon run data set at the start of a dungeon run
    public NetworkVariable<int> m_ectoDebitStartAmount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_ectoDebitCount_dungeon = new NetworkVariable<int>(0);      // this is the ecto out of your offchain bank account you start with
    public NetworkVariable<int> m_ectoLiveCount_dungeon = new NetworkVariable<int>(0);      // this is the ecto that gets added to as you collect ecto, starts at 0
    public NetworkVariable<int> m_bombLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_portaHoleLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> m_zenCricketLiveCount_dungeon = new NetworkVariable<int>(0);

    // post dungeon data - used to update data
    private NetworkVariable<int> m_ectoDelta_postDungeon = new NetworkVariable<int>(0);
    private NetworkVariable<int> m_dustDelta_postDungeon = new NetworkVariable<int>(0);

    // the shared dust count between all players in the dungeon
    //public static int m_dustLiveCount_dungeon = 0;

    private TeamDustCounter m_teamDustCounter;

    // if player dies
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain
    // if player escapes
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain + ectoLiveCount_dungeon

    public string dungeonFormation = "solo";

    // uri for accessing database
    private string dbUri = "https://db.playdropt.io/offchaindata";

    // for keeping track of current wallet
    private string m_walletAddress = null;
    private float k_walletUpdateInterval = 1f;
    private float m_walletUpdateTimer = 0f;

    // for keeping track of current gotchi
    private int m_gotchiId = 0;
    private float k_gotchiIdUpdateInterval = 1f;
    private float m_gotchiIdUpdateTimer = 0f;

    private Level.NetworkLevel.LevelType m_currentLevelType;

    public override void OnNetworkSpawn()
    {
        m_walletAddress = null;
        m_gotchiId = 0;
        m_currentLevelType = Level.NetworkLevel.LevelType.Null;

        m_teamDustCounter = FindAnyObjectByType<TeamDustCounter>();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (!playerController.isGameOvered)
                {
                    _ = ExitDungeonCalculateBalances(false);
                }
            }
        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            _ = CheckWalletAddressChanged();
            CheckGotchiIdChanged();
        }

        if (IsServer)
        {
            CheckCurrentLevelType_SERVER();
            UpdateWalletAndGotchiOffchainData();
        }
    }

    private float m_updateWalletAndGotchiOffchainDataTimer = 0f;
    private float k_updateWalletAndGotchiOffchainDataInterval = 3f;

    async UniTaskVoid UpdateWalletAndGotchiOffchainData()
    {
        if (!IsServer) return;
        if (!LevelManager.Instance) return;
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        if (IsHost ||
            (IsServer && Bootstrap.IsLocalConnection()))
        {
            m_walletAddress = Bootstrap.Instance.TestWalletAddress;
        }

        try
        {
            m_updateWalletAndGotchiOffchainDataTimer -= Time.deltaTime;
            if (m_updateWalletAndGotchiOffchainDataTimer < 0)
            {
                m_updateWalletAndGotchiOffchainDataTimer = k_updateWalletAndGotchiOffchainDataInterval;

                Debug.Log("Check backend data for m_walletAddress " + m_walletAddress + ", m_gotchiId " + m_gotchiId);

                if (!string.IsNullOrEmpty(m_walletAddress))
                {
                    GetLatestOffchainWalletDataServerRpcAsync(m_walletAddress);

                }

                if (m_gotchiId > 0)
                {
                    GetLatestOffchainGotchiDataServerRpcAsync(m_gotchiId);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    private void CheckCurrentLevelType_SERVER()
    {
        if (!IsServer) return;
        if (LevelManager.Instance == null) return;

        var newLevelType = LevelManager.Instance.GetCurrentLevelType();

        if (m_currentLevelType == newLevelType) return;

        // DegenapeVillage >> Dungeon/DungeonReset transition
        if (m_currentLevelType == Level.NetworkLevel.LevelType.DegenapeVillage &&
            (newLevelType == Level.NetworkLevel.LevelType.Dungeon || newLevelType == Level.NetworkLevel.LevelType.DungeonRest))
        {
            EnterDungeonCalculateBalances();
            StartDungeonTimer();

            // determine dungeon formation
            var players = Game.Instance.playerControllers.ToArray();
            if (players.Length == 3) dungeonFormation = "trio";
            else if (players.Length == 2) dungeonFormation = "duo";
            else if (players.Length == 1) dungeonFormation = "solo";
            else dungeonFormation = "illegal: " + players.Length.ToString();
        }

        // Dungeon/DungeonRest >> DegenapeVillage transition
        else if ((m_currentLevelType == Level.NetworkLevel.LevelType.Dungeon || m_currentLevelType == Level.NetworkLevel.LevelType.DungeonRest) &&
            newLevelType == Level.NetworkLevel.LevelType.DegenapeVillage)
        {
            ExitDungeonCalculateBalances(true);
            StopDungeonTimer();
            ResetTimer();
        }

        m_currentLevelType = newLevelType;
    }

    
    private async UniTaskVoid CheckWalletAddressChanged()
    {
        // don't check for wallet updates every single frame
        m_walletUpdateTimer -= Time.deltaTime;
        if (m_walletUpdateTimer > 0) return;
        m_walletUpdateTimer = k_walletUpdateInterval;

        // only check for wallet updates if in degenape village
        if (LevelManager.Instance == null) return;
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        // try get latest wallet address
        try
        {
            if (ThirdwebManager.Instance == null) return;

            // see if we have a wallet address in player prefs
            var playerPrefWalletAddress = PlayerPrefs.GetString("WalletAddress");

            // get wallet
            var wallet = ThirdwebManager.Instance.GetActiveWallet();
            if (wallet == null)
            {
                //TryGetOffchainTestData();
                return;
            }

            // check if wallet is connected
            var isConnected = await wallet.IsConnected();
            if (!isConnected)
            {
                //TryGetOffchainTestData();
                return;
            }

            // get all latest data if address changed
            var connectedWalletAddress = await wallet.GetAddress();
            if (connectedWalletAddress != m_walletAddress && connectedWalletAddress != null)
            {
                m_walletAddress = connectedWalletAddress;
                PlayerPrefs.SetString("WalletAddress", m_walletAddress);
                GetLatestOffchainWalletDataServerRpc(m_walletAddress);
            }

        }
        catch
        {
            //TryGetOffchainTestData();
        }
    }
    

    [Rpc(SendTo.Server)]
    private void GetLatestOffchainWalletDataServerRpc(string walletAddress)
    {
        _ = GetLatestOffchainWalletDataServerRpcAsync(walletAddress);
    }

    private async UniTaskVoid GetLatestOffchainWalletDataServerRpcAsync(string walletAddress)
    {
        if (!IsServer) return;

        // save the current wallet address for this player
        m_walletAddress = walletAddress;

        // getsert wallet
        try
        {
            var responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/wallets/" + walletAddress);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var walletData = JsonUtility.FromJson<Wallet_Data>(responseStr);

                m_ectoVillageBalance_wallet.Value = walletData.ecto_balance;

                m_bombLiveBalance_wallet.Value = walletData.bomb_balance;
                m_portaHoleLiveBalance_wallet.Value = walletData.portahole_balance;
                m_zenCricketLiveBalance_wallet.Value = walletData.zencricket_balance;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private void CheckGotchiIdChanged()
    {
        // dont check every frame
        m_gotchiIdUpdateTimer -= Time.deltaTime;
        if (m_gotchiIdUpdateTimer > 0) return;
        m_gotchiIdUpdateTimer = k_gotchiIdUpdateInterval;

        // only check for gotchi updates if in degenape village
        if (LevelManager.Instance == null) return;
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        // get gotchi
        var gotchiId = GotchiDataManager.Instance.GetSelectedGotchiId();
        if (gotchiId != m_gotchiId)
        {
            m_gotchiId = gotchiId;
            GetLatestOffchainGotchiDataServerRpc(m_gotchiId);
        }
    }
    

    [Rpc(SendTo.Server)]
    private void GetLatestOffchainGotchiDataServerRpc(int gotchiId)
    {
        _ = GetLatestOffchainGotchiDataServerRpcAsync(gotchiId);
    }

    private async UniTask GetLatestOffchainGotchiDataServerRpcAsync(int gotchiId)
    {
        if (!IsServer) return;

        // save gotchi id to server m_gotchiId
        m_gotchiId = gotchiId;

        // getsert gotchi data
        try
        {
            string responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/gotchis/" + gotchiId.ToString());
            if (!string.IsNullOrEmpty(responseStr))
            {
                Gotchi_Data gotchiData = JsonUtility.FromJson<Gotchi_Data>(responseStr);

                m_bombCapacity_gotchi.Value = gotchiData.bomb_capacity;
                m_portaHoleCapacity_gotchi.Value = gotchiData.portahole_capacity;
                m_zenCricketCapacity_gotchi.Value = gotchiData.zencricket_capacity;
                m_isEssenceInfused_gotchi.Value = gotchiData.is_essence_infused;
                m_ectoDungeonStartAmount_gotchi.Value = gotchiData.ecto_dungeon_start_amount;
                m_dustVillageBalance_gotchi.Value = gotchiData.dust_balance;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    // enter dungeon method that calculates balance
    public void EnterDungeonCalculateBalances()
    {
        if (!IsServer) return;

        // 6 dungeon values to be set
        m_ectoDebitStartAmount_dungeon.Value = math.min(m_ectoDungeonStartAmount_gotchi.Value, m_ectoVillageBalance_wallet.Value);
        m_ectoDebitCount_dungeon.Value = m_ectoDebitStartAmount_dungeon.Value;
        m_ectoLiveCount_dungeon.Value = 0;
        m_bombLiveCount_dungeon.Value =
            math.min(m_bombCapacity_gotchi.Value, m_bombLiveBalance_wallet.Value);
        m_portaHoleLiveCount_dungeon.Value =
            math.min(m_portaHoleCapacity_gotchi.Value, m_portaHoleLiveBalance_wallet.Value);
        m_zenCricketLiveCount_dungeon.Value =
            math.min(m_zenCricketCapacity_gotchi.Value, m_zenCricketLiveBalance_wallet.Value);

        // 1 dust team counter starts at 0 always
        if (m_teamDustCounter == null) { Debug.LogWarning("TeamDustCounter not found!"); return; }
        m_teamDustCounter.Count.Value = 0;
    }

    // exit dungeon calculates new balances and updates the database
    public async UniTask ExitDungeonCalculateBalances(bool isEscaped)
    {
        if (!IsServer) return;

        var playerLeaderboardLogger = GetComponent<PlayerLeaderboardLogger>();
        if (playerLeaderboardLogger == null) return;

        m_ectoDelta_postDungeon.Value = isEscaped ?
            (int)m_ectoDebitCount_dungeon.Value - (int)m_ectoDebitStartAmount_dungeon.Value + (int)m_ectoLiveCount_dungeon.Value :
            (int)m_ectoDebitCount_dungeon.Value - (int)m_ectoDebitStartAmount_dungeon.Value;

        m_dustDelta_postDungeon.Value = (int)(m_teamDustCounter.Count.Value * CodeInjector.Instance.GetOutputMultiplier());

        // save wallet specific dungeon collectibles
        try
        {
            await LogWalletDeltaDataServerRpcAsync(m_ectoDelta_postDungeon.Value, 0, 0, 0);

            m_ectoDebitStartAmount_dungeon.Value = 0;
            m_ectoDebitCount_dungeon.Value = 0;
            m_ectoLiveCount_dungeon.Value = 0;
        }
        catch
        {
            Debug.LogWarning("Could not log post-dungeon wallet delta data, is server running?");
        }

        // save gotchi specific dungeon collectibles
        try
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                await LogGotchiDeltaDataServerRpcAsync(
                    playerController.NetworkGotchiId.Value,                 // gotchiId
                    isEscaped ? (int)m_teamDustCounter.Count.Value : 0);    // dust delta
            }

        }
        catch
        {
            Debug.LogWarning("Could not log post-dungeon gotchi delta data, is server running?");
        }

    }

    private async UniTask LogWalletDeltaDataServerRpcAsync(int ectoDelta, int bombDelta, int portaHoleDelta, int zenCricketDelta)
    {
        try
        {
            string url = dbUri + "/wallets/delta/" + m_walletAddress;
            var json = JsonUtility.ToJson(new WalletDelta_Data
            {
                ecto_delta = ectoDelta,
                bomb_delta = bombDelta,
                portahole_delta = portaHoleDelta,
                zencricket_delta = zenCricketDelta
            });

            var responseStr = await Dropt.Utils.Http.PostEncryptedRequest(url, json, Bootstrap.Instance.DbSecret);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data walletData = JsonUtility.FromJson<Wallet_Data>(responseStr);

                m_ectoVillageBalance_wallet.Value = walletData.ecto_balance;
                m_bombLiveBalance_wallet.Value = walletData.bomb_balance;
                m_portaHoleLiveBalance_wallet.Value = walletData.portahole_balance;
                m_zenCricketLiveBalance_wallet.Value = walletData.zencricket_balance;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async UniTask LogGotchiDeltaDataServerRpcAsync(int gotchiId, int dustDelta)
    {
        try
        {
            var json = JsonUtility.ToJson(new GotchiDelta_Data
            {
                dust_delta = dustDelta,
            });

            Debug.Log($"LogGotchiDelta, dust: {dustDelta}");

            var responseStr = await Dropt.Utils.Http.PostEncryptedRequest(dbUri + "/gotchis/delta/" + gotchiId, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Gotchi_Data gotchiData = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                m_dustVillageBalance_gotchi.Value = gotchiData.dust_balance;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    // Method to add value to GltrCount
    public void AddDungeonDust(int value)
    {
        if (!IsServer) return;

        m_teamDustCounter.Count.Value += value;
    }

    // Method to add ecto
    public bool AddDungeonEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;

        m_ectoLiveCount_dungeon.Value += value;

        return true;
    }

    public bool AddVillageEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        if (!string.IsNullOrEmpty(m_walletAddress))
        {
            _ = LogWalletDeltaDataServerRpcAsync(value, 0, 0, 0);
        }

        return true;
    }

    public bool AddVillageBombs(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        if (!string.IsNullOrEmpty(m_walletAddress))
        {
            _ = LogWalletDeltaDataServerRpcAsync(0, value, 0, 0);
        }

        return true;
    }

    public bool AddVillagePortaHoles(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        if (!string.IsNullOrEmpty(m_walletAddress))
        {
            _ = LogWalletDeltaDataServerRpcAsync(0, 0, value, 0);
        }

        return true;
    }

    public bool AddVillageZenCrickets(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        if (!string.IsNullOrEmpty(m_walletAddress))
        {
            _ = LogWalletDeltaDataServerRpcAsync(0, 0, 0, value);
        }

        return true;
    }

    public async UniTask<bool> RemoveVillageEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        if (value <= m_ectoVillageBalance_wallet.Value)
        {
            try
            {
                await LogWalletDeltaDataServerRpcAsync(-value, 0, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    public bool RemoveDungeonEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;
        if (LevelManager.Instance.IsDegenapeVillage()) return false;

        var isSpent = TrySpendDungeonEcto(value);
        return isSpent;
    }

    public bool IsVillageEctoGreaterThanOrEqualTo(int value)
    {
        if (!LevelManager.Instance.IsDegenapeVillage()) return false;

        return m_ectoVillageBalance_wallet.Value >= value;
    }

    public bool IsDungeonEctoGreaterThanOrEqualTo(int value)
    {
        if (LevelManager.Instance.IsDegenapeVillage()) return false;

        return (m_ectoLiveCount_dungeon.Value + m_ectoDebitCount_dungeon.Value) >= value;
    }

    // Method to remove ecto
    private bool TrySpendDungeonEcto(int value)
    {
        if (!IsServer) return false;

        var ectoLive = m_ectoLiveCount_dungeon.Value;
        var ectoDebit = m_ectoDebitCount_dungeon.Value;

        // check if we have enough ecto
        if ((ectoLive + ectoDebit) < value) return false;

        // spend down all ectoLive value
        var debitDelta = ectoLive - value;

        if (debitDelta < 0)
        {
            m_ectoLiveCount_dungeon.Value = 0;
            m_ectoDebitCount_dungeon.Value += debitDelta;
        }
        else
        {
            m_ectoLiveCount_dungeon.Value -= value;
        }

        return true;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        var playerCharacter = GetComponent<PlayerCharacter>();
        if (playerCharacter == null) return;

        playerCharacter.Essence.Value += value;

        var networkObject = GetComponent<NetworkObject>();
        if (networkObject == null) return;

        PopupEssenceTextClientRpc(value, networkObject.NetworkObjectId);
    }

    private void StartDungeonTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
        if (playerDungeonTime == null) return;

        playerDungeonTime.StartTimer();
    }

    private void StopDungeonTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
        if (playerDungeonTime == null) return;

        playerDungeonTime.StopTimer();
    }

    private void ResetTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
        if (playerDungeonTime == null) return;

        playerDungeonTime.ResetTimer();
    }

    [ClientRpc]
    private void PopupEssenceTextClientRpc(float value, ulong networkObjectId)
    {
        var player = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        if (player == null) return;

        PopupTextManager.Instance.PopupText(value.ToString("F0"), player.transform.position + new Vector3(0, 1.5f, 0), 30, Dropt.Utils.Color.HexToColor("#94fdff"));
    }

    [System.Serializable]
    public class Wallet_Data
    {
        public int ecto_balance;
        public int bomb_balance;
        public int portahole_balance;
        public int zencricket_balance;
    }

    [System.Serializable]
    public class WalletDelta_Data
    {
        public int ecto_delta;
        public int bomb_delta;
        public int portahole_delta;
        public int zencricket_delta;
    }

    [System.Serializable]
    public class Gotchi_Data
    {
        public int bomb_capacity;
        public int portahole_capacity;
        public int zencricket_capacity;
        public bool is_essence_infused;
        public int ecto_dungeon_start_amount;
        public int dust_balance;
    }

    [System.Serializable]
    public class GotchiDelta_Data
    {
        public int dust_delta;
    }

    public bool TryUseDungeonBomb()
    {
        if (!IsServer) return false;

        if (m_bombLiveCount_dungeon.Value > 0)
        {
            m_bombLiveCount_dungeon.Value--;
            _ = LogWalletDeltaDataServerRpcAsync(0, -1, 0, 0);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryUseDungeonPortaHole()
    {
        if (!IsServer) return false;

        if (m_portaHoleLiveCount_dungeon.Value > 0)
        {
            m_portaHoleLiveCount_dungeon.Value--;
            _ = LogWalletDeltaDataServerRpcAsync(0, 0, -1, 0);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryUseDungeonZenCricket()
    {
        if (!IsServer) return false;

        if (m_zenCricketLiveCount_dungeon.Value > 0)
        {
            m_zenCricketLiveCount_dungeon.Value--;
            _ = LogWalletDeltaDataServerRpcAsync(0, 0, 0, -1);
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetEctoDeltaValue_PostDungeon()
    {
        return m_ectoDelta_postDungeon.Value;
    }

    public int GetDustDeltaValue_PostDungeon()
    {
        return m_dustDelta_postDungeon.Value;
    }

    //public int GetBombDeltaValue()
    //{
    //    return m_postDungeonBombDelta;
    //}
}