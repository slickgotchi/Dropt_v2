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
    [System.Serializable]
    public struct PlayerOffchainDataStruct
    {
        // wallet (offchain data)
        public int ectoBalance_offchain;
        public int dustBalance_offchain;
        public int bombBalance_offchain;

        // gotchi (offchain data)
        public int ectoDungeonStartAmount_offchain;
        public int bombDungeonCapacity_offchain;
        public int healSalveDungeonCharges_offchain;
        public bool isEssenceInfused_offchain;

        // dungeon (dungeon run data set at the start of a dungeon run)
        public int ectoDebitStartAmount_dungeon;
        public int ectoDebitCount_dungeon;
        public int ectoLiveCount_dungeon;
        public int dustLiveCount_dungeon;
        public int bombStartCount_dungeon;
        public int bombLiveCount_dungeon;
        public int healSalveChargeCount_dungeon;

        // post dungeon deltas
        public int postDungeonEctoDelta;
        public int postDungeonDustDelta;
        public int postDungeonBombDelta;
    }

    public NetworkVariable<PlayerOffchainDataStruct> data =
        new NetworkVariable<PlayerOffchainDataStruct>(new PlayerOffchainDataStruct());

    /*
    // wallet (offchain data)
    public NetworkVariable<int> ectoBalance_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> dustBalance_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombBalance_offchain = new NetworkVariable<int>(0);

    // gotchi (offchain data)
    public NetworkVariable<int> ectoDungeonStartAmount_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombDungeonCapacity_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> healSalveDungeonCharges_offchain = new NetworkVariable<int>(3);
    public NetworkVariable<bool> isEssenceInfused_offchain = new NetworkVariable<bool>(false);

    // dungeon (dungeon run data set at the start of a dungeon run)
    public NetworkVariable<int> ectoDebitStartAmount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> ectoDebitCount_dungeon = new NetworkVariable<int>(0);       // this is the ecto out of your offchain bank account you start with
    public NetworkVariable<int> ectoLiveCount_dungeon = new NetworkVariable<int>(0);        // this is the ecto that gets added to as you collect ecto, starts at 0
    public NetworkVariable<int> dustLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombStartCount_dungeon = new NetworkVariable<int>(3);
    public NetworkVariable<int> bombLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> healSalveChargeCount_dungeon = new NetworkVariable<int>(0);

    private NetworkVariable<int> m_postDungeonEctoDelta = new NetworkVariable<int>(0);
    private NetworkVariable<int> m_postDungeonDustDelta = new NetworkVariable<int>(0);
    private NetworkVariable<int> m_postDungeonBombDelta = new NetworkVariable<int>(0);
    */


    // if player dies
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain
    // if player escapes
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain + ectoLiveCount_dungeon

    public string dungeonFormation = "solo";

    // uri for accessing database
    private string dbUri = "https://db.playdropt.io";

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
    }

    public override void OnNetworkDespawn()
    {
        // WARNING: we need a way to differentiate between full disconnects and temporary internet loss disconnect/reconnects
        //if (IsServer)
        //{
        //    if (LevelManager.Instance.IsDungeon() || LevelManager.Instance.IsDungeonRest())
        //    {
        //        ExitDungeonCalculateBalances(false);
        //    }
        //}
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
        }
    }

    private void CheckCurrentLevelType_SERVER()
    {
        if (!IsServer) return;

        var newLevelType = LevelManager.Instance.GetCurrentLevelType();

        if (m_currentLevelType == newLevelType) return;

        // perform once off code when moving from degenape to dungeon
        if (newLevelType == Level.NetworkLevel.LevelType.Dungeon &&
            m_currentLevelType == Level.NetworkLevel.LevelType.DegenapeVillage)
        {
//<<<<<<< HEAD
            EnterDungeonCalculateBalances();
            StartDungeonTimer();
//=======
            //var newLevelType = LevelManager.Instance.GetCurrentLevelType();

            // perform once off code when moving from degenape to dungeon
            if (newLevelType == Level.NetworkLevel.LevelType.Dungeon &&
                m_currentLevelType == Level.NetworkLevel.LevelType.DegenapeVillage)
            {
                EnterDungeonCalculateBalances();

                // determine dungeon formation
                var players = FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (players.Length == 3) dungeonFormation = "trio";
                else if (players.Length == 2) dungeonFormation = "duo";
                else if (players.Length == 1) dungeonFormation = "solo";
                else dungeonFormation = "illegal: " + players.Length.ToString();

            }

            // perform once off code if we go from a dungeon or dungeonrest back to the village (via hole)
            // THIS SHOULD ONLY HAPPEN DURING TESTING AND NOT IN ACTUAL GAMEPLAY
            if (newLevelType == Level.NetworkLevel.LevelType.DegenapeVillage &&
                m_currentLevelType == Level.NetworkLevel.LevelType.Dungeon)
            {
                ExitDungeonCalculateBalances(true);
            }

            if (newLevelType == Level.NetworkLevel.LevelType.DegenapeVillage &&
                m_currentLevelType == Level.NetworkLevel.LevelType.DungeonRest)
            {
                ExitDungeonCalculateBalances(true);
            }

            m_currentLevelType = newLevelType;
//>>>>>>> main
        }

        // perform once off code if we go from a dungeon or dungeonrest back to the village (via hole)
        // THIS SHOULD ONLY HAPPEN DURING TESTING AND NOT IN ACTUAL GAMEPLAY
        if (newLevelType == Level.NetworkLevel.LevelType.DegenapeVillage &&
            m_currentLevelType == Level.NetworkLevel.LevelType.Dungeon)
        {
            ExitDungeonCalculateBalances(true);
            StopDungeonTimer();
            ResetTimer();
        }

        if (newLevelType == Level.NetworkLevel.LevelType.DegenapeVillage &&
            m_currentLevelType == Level.NetworkLevel.LevelType.DungeonRest)
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
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        // try get latest wallet address
        try
        {
            if (ThirdwebManager.Instance == null) return;

            // get wallet
            var wallet = ThirdwebManager.Instance.GetActiveWallet();
            if (wallet == null)
            {
                TryGetOffchainTestData();
                return;
            }

            // check if wallet is connected
            var isConnected = await wallet.IsConnected();
            if (!isConnected)
            {
                TryGetOffchainTestData();
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
            TryGetOffchainTestData();
        }
    }

    private void TryGetOffchainTestData()
    {
        if ((IsHost || Bootstrap.IsLocalConnection()) && m_walletAddress != Bootstrap.Instance.TestWalletAddress)
        {
            m_walletAddress = Bootstrap.Instance.TestWalletAddress;
            PlayerPrefs.SetString("WalletAddress", m_walletAddress);
            GetLatestOffchainWalletDataServerRpc(m_walletAddress);
        }
    }

    [Rpc(SendTo.Server)]
    private void GetLatestOffchainWalletDataServerRpc(string walletAddress)
    {
        _ = GetLatestOffchainWalletDataServerRpcAsync(walletAddress);
    }

    private async UniTaskVoid GetLatestOffchainWalletDataServerRpcAsync(string walletAddress)
    {
        // save the current wallet address for this player
        m_walletAddress = walletAddress;

        // first check if the wallet exists
        try
        {
            var responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/wallets/" + walletAddress);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var walletData = JsonUtility.FromJson<Wallet_Data>(responseStr);

                var newData = data.Value;

                newData.ectoBalance_offchain = walletData.ecto_balance;
                newData.bombBalance_offchain = walletData.bomb_balance;

                data.Value = newData;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }

        // create a new entry with defaults if we didn't find a valid wallet
        try
        {
            var json = JsonUtility.ToJson(new Wallet_Data
            {
                id = walletAddress,
                ecto_balance = 0,
                bomb_balance = 0,
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data walletData = JsonUtility.FromJson<Wallet_Data>(responseStr);

                var newData = data.Value;

                newData.ectoBalance_offchain = walletData.ecto_balance;
                newData.bombBalance_offchain = walletData.bomb_balance;

                data.Value = newData;

                Debug.Log("Create new wallet entry in offchain database for " + walletAddress);
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
        // save gotchi id to server m_gotchiId
        m_gotchiId = gotchiId;

        // try get data
        try
        {
            string responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/gotchis/" + gotchiId.ToString());
            if (!string.IsNullOrEmpty(responseStr))
            {
                Gotchi_Data gotchiData = JsonUtility.FromJson<Gotchi_Data>(responseStr);

                var newData = data.Value;

                newData.ectoDungeonStartAmount_offchain = gotchiData.ecto_dungeon_start_amount;
                newData.bombDungeonCapacity_offchain = gotchiData.bomb_dungeon_capacity;
                newData.healSalveDungeonCharges_offchain = gotchiData.heal_salve_dungeon_charges;
                newData.dustBalance_offchain = gotchiData.dust_balance;
                newData.isEssenceInfused_offchain = gotchiData.is_essence_infused;

                data.Value = newData;

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }

        // add a new entry
        try
        {
            var json = JsonUtility.ToJson(new Gotchi_Data
            {
                id = gotchiId,
                ecto_dungeon_start_amount = 0,
                bomb_dungeon_capacity = 1,
                heal_salve_dungeon_charges = 1,
                dust_balance = 0,
                is_essence_infused = false
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/gotchis", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var gotchiData = JsonUtility.FromJson<Gotchi_Data>(responseStr);

                var newData = data.Value;

                newData.ectoDungeonStartAmount_offchain = gotchiData.ecto_dungeon_start_amount;
                newData.bombDungeonCapacity_offchain = gotchiData.bomb_dungeon_capacity;
                newData.healSalveDungeonCharges_offchain = gotchiData.heal_salve_dungeon_charges;
                newData.dustBalance_offchain = gotchiData.dust_balance;
                newData.isEssenceInfused_offchain = gotchiData.is_essence_infused;

                data.Value = newData;

                Debug.Log("Created new database entry for gotchi: " + gotchiId);
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
        //Debug.Log("EnterDungeonCalculateBalances()");

        if (!IsServer) return;

        var newData = data.Value;

        // ecto calcs
        newData.ectoDebitStartAmount_dungeon =
            math.min(newData.ectoDungeonStartAmount_offchain, newData.ectoBalance_offchain);
        newData.ectoDebitCount_dungeon = newData.ectoDebitStartAmount_dungeon;
        newData.ectoLiveCount_dungeon = 0;

        //Debug.Log($"ectoDebitStartCount: {ectoDebitStartAmount_dungeon.Value}");

        // dust starts at 0 always
        newData.dustLiveCount_dungeon = 0;

        // bomb counts
        newData.bombStartCount_dungeon =
            math.min(newData.bombDungeonCapacity_offchain, newData.bombBalance_offchain);
        //Debug.Log("bombStartCount_dungeon -> " + bombStartCount_dungeon.Value);
        newData.bombLiveCount_dungeon = newData.bombStartCount_dungeon;

        // heal charge to full
        newData.healSalveChargeCount_dungeon = newData.healSalveDungeonCharges_offchain;

        data.Value = newData;
    }

    // exit dungeon calculates new balances and updates the database
    public async void ExitDungeonCalculateBalances(bool isEscaped)
    {
        //Debug.Log("ExitDungeonCalculateBalances()");

        if (!IsServer) return;

        var newData = data.Value;

        newData.postDungeonEctoDelta = isEscaped ?
            newData.ectoDebitCount_dungeon - newData.ectoDebitStartAmount_dungeon + newData.ectoLiveCount_dungeon :
            newData.ectoDebitCount_dungeon - newData.ectoDungeonStartAmount_offchain;
        newData.postDungeonDustDelta = isEscaped ?
            (int)(newData.dustLiveCount_dungeon * CodeInjector.Instance.GetOutputMultiplier()) :
            0;
        newData.postDungeonBombDelta = newData.bombLiveCount_dungeon - newData.bombStartCount_dungeon;

        data.Value = newData;

        //Debug.Log($"postDungeonEctoDelta: " + m_postDungeonEctoDelta);

        // log wallet deltas
        try
        {
            await LogWalletDeltaDataServerRpcAsync(newData.postDungeonEctoDelta, newData.postDungeonBombDelta);

            // successfully logged deltas so zero all the balances
            newData.ectoDebitStartAmount_dungeon = 0;
            newData.ectoDebitCount_dungeon = 0;
            newData.ectoLiveCount_dungeon = 0;
            newData.bombStartCount_dungeon = 0;
            newData.bombLiveCount_dungeon = 0;

            data.Value = newData;
        }
        catch
        {
            Debug.LogWarning("Could not log post-dungeon wallet delta data, is server running?");
        }

        // log gotchi deltas
        try
        {
            await LogGotchiDeltaDataServerRpcAsync(GetComponent<PlayerController>().NetworkGotchiId.Value, newData.postDungeonDustDelta);
            newData.dustLiveCount_dungeon = 0;

            data.Value = newData;
        }
        catch
        {
            Debug.LogWarning("Could not log post-dungeon gotchi delta data, is server running?");
        }
    }

    private async UniTask LogWalletDeltaDataServerRpcAsync(int ectoDelta, int bombDelta)
    {
        try
        {
            var json = JsonUtility.ToJson(new WalletDelta_Data
            {
                ecto_delta = ectoDelta,
                bomb_delta = bombDelta
            });

            Debug.Log($"LogWalletDelta, ecto: {ectoDelta}, bomb: {bombDelta}");

            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets/delta/" + m_walletAddress, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data walletData = JsonUtility.FromJson<Wallet_Data>(responseStr);

                var newData = data.Value;

                newData.ectoBalance_offchain = walletData.ecto_balance;
                newData.bombBalance_offchain = walletData.bomb_balance;

                data.Value = newData;

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

            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/gotchis/delta/" + gotchiId, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Gotchi_Data gotchiData = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                var newData = data.Value;
                newData.dustBalance_offchain = gotchiData.dust_balance;
                data.Value = newData;
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

        var newData = data.Value;
        newData.dustLiveCount_dungeon += value;
        data.Value = newData;
    }

    // Method to add ecto
    public bool AddEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;

        var newData = data.Value;
        newData.ectoLiveCount_dungeon += value;
        data.Value = newData;

        return true;
    }

    // Method to remove ecto
    public async UniTask<bool> RemoveEcto(int value)
    {
        if (!IsServer) return false;
        if (value <= 0) return false;

        // if in village, deduct straight from database
        if (LevelManager.Instance.IsDegenapeVillage())
        {
            // check if we have sufficent ecto balance
            if (value <= data.Value.ectoBalance_offchain)
            {
                try
                {
                    await LogWalletDeltaDataServerRpcAsync(-value, 0);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        // if in dungeon, deduct from ecto live count
        else
        {
            return TrySpendDungeonEcto(value);
        }

        return true;
    }

    public bool DoWeHaveEctoGraterThanOrEqualTo(int value)
    {
        return value >= data.Value.ectoBalance_offchain;
    }

    // Method to remove ecto
    private bool TrySpendDungeonEcto(int value)
    {
        if (!IsServer) return false;

        var ectoLive = data.Value.ectoLiveCount_dungeon;
        var ectoDebit = data.Value.ectoDebitCount_dungeon;

        // check if we have enough ecto
        if ((ectoLive + ectoDebit) < value) return false;

        // spend down all ectoLive value
        var debitDelta = ectoLive - value;

        var newData = data.Value;

        if (debitDelta < 0)
        {
            newData.ectoLiveCount_dungeon = 0;
            newData.ectoDebitCount_dungeon += debitDelta;
        }
        else
        {
            newData.ectoLiveCount_dungeon -= value;
        }

        data.Value = newData;

        return true;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        GetComponent<PlayerCharacter>().Essence.Value += value;
        PopupEssenceTextClientRpc(value, GetComponent<NetworkObject>().NetworkObjectId);
    }

    private void StartDungeonTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
        playerDungeonTime.StartTimer();
    }

    private void StopDungeonTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
        playerDungeonTime.StopTimer();
    }

    private void ResetTimer()
    {
        PlayerDungeonTime playerDungeonTime = GetComponent<PlayerDungeonTime>();
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
        public string id;
        public int ecto_balance;
        public int bomb_balance;
    }

    [System.Serializable]
    public class Gotchi_Data
    {
        public int id;
        public int ecto_dungeon_start_amount;
        public int bomb_dungeon_capacity;
        public int heal_salve_dungeon_charges;
        public int dust_balance;
        public bool is_essence_infused;
    }

    [System.Serializable]
    public class WalletDelta_Data
    {
        public int ecto_delta;
        public int bomb_delta;
    }

    [System.Serializable]
    public class GotchiDelta_Data
    {
        public int dust_delta;
    }

    public void UseHealSalveItem()
    {
        var newData = data.Value;
        newData.healSalveChargeCount_dungeon--;
        data.Value = newData;
    }

    public bool IsBombAvailable()
    {
        return data.Value.bombLiveCount_dungeon > 0;
    }

    public void UseBombItem()
    {
        var newData = data.Value;
        newData.bombLiveCount_dungeon--;
        data.Value = newData;
    }

    public int GetEctoDeltaValue()
    {
        return data.Value.postDungeonEctoDelta;
    }

    public int GetDustDeltaValue()
    {
        return data.Value.postDungeonDustDelta;
    }

    public int GetBombDeltaValue()
    {
        return data.Value.postDungeonBombDelta;
    }
}