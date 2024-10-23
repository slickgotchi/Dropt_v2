using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Thirdweb;
using GotchiHub;
using Cysharp.Threading.Tasks;

// cGHST bank logic
// - cGhstVillageBank is the total cGhst you have when in the village
// - cGhstDungeonBank is the amount of cGhst from your bank that you can start a dungeon with (automatically available)
// - cGhstDungeonFound is the cGhst collected as you explore the dungeon
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
    // wallet (offchain data)
    public NetworkVariable<int> ectoBalance_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> dustBalance_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombBalance_offchain = new NetworkVariable<int>(0);

    // gotchi (offchain data)
    public NetworkVariable<int> ectoDungeonStartAmount_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombDungeonCapacity_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<int> healSalveDungeonCharges_offchain = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isEssenceInfused_offchain = new NetworkVariable<bool>(false);

    // dungeon (dungeon run data set at the start of a dungeon run)
    public NetworkVariable<int> ectoStartCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> ectoCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> dustStartCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> dustCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombStartCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> healSalveChargeCount_dungeon = new NetworkVariable<int>(0);

    // when a dungeon is complete (or the player dies) we recalculate ecto balance by doing
    //      post-dungeonEctoDelta = ectoCount - ectoStartCount
    // we do a similar calculation for boms and dust
    // heal salves always start at full charge for dungeon

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

    private void Update()
    {
        if (IsLocalPlayer)
        {
            CheckWalletAddressChanged();
            CheckGotchiIdChanged();
        }

        UpdateDebugTopUps();
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
            // get all latest data if address changed
            var connectedWalletAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            if (connectedWalletAddress != m_walletAddress && connectedWalletAddress != null)
            {
                m_walletAddress = connectedWalletAddress;
                PlayerPrefs.SetString("WalletAddress", m_walletAddress);
                GetLatestOffchainWalletDataServerRpc(m_walletAddress);
            }
        }
        catch
        {
            // if we are in host mode, use the test wallet address
            if (IsHost && m_walletAddress != Bootstrap.Instance.TestWalletAddress)
            {
                m_walletAddress = Bootstrap.Instance.TestWalletAddress;
                PlayerPrefs.SetString("WalletAddress", m_walletAddress);
                GetLatestOffchainWalletDataServerRpc(m_walletAddress);
            }
        }
    }

    [Rpc(SendTo.Server)]
    void GetLatestOffchainWalletDataServerRpc(string walletAddress)
    {
        GetLatestOffchainWalletDataServerRpcAsync(walletAddress);
    }

    async UniTaskVoid GetLatestOffchainWalletDataServerRpcAsync(string walletAddress)
    {
        // save the current wallet address for this player
        m_walletAddress = walletAddress;

        // first check if the wallet exists
        try
        {
            var responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/wallets/" + walletAddress);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var data = JsonUtility.FromJson<Wallet_Data>(responseStr);
                ectoBalance_offchain.Value = data.ecto_balance;
                dustBalance_offchain.Value = data.dust_balance;
                bombBalance_offchain.Value = data.bomb_balance;
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
                dust_balance = 0,
                bomb_balance = 0,
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data data = JsonUtility.FromJson<Wallet_Data>(responseStr);
                ectoBalance_offchain.Value = data.ecto_balance;
                dustBalance_offchain.Value = data.dust_balance;
                bombBalance_offchain.Value = data.bomb_balance;
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
            PlayerPrefs.SetInt("GotchiId", m_gotchiId);
            GetLatestOffchainGotchiDataServerRpc(m_gotchiId);
        }
    }

    [Rpc(SendTo.Server)]
    private void GetLatestOffchainGotchiDataServerRpc(int gotchiId)
    {
        GetLatestOffchainGotchiDataServerRpcAsync(gotchiId);
    }

    async UniTask GetLatestOffchainGotchiDataServerRpcAsync(int gotchiId)
    {
        // save gotchi id to server m_gotchiId
        m_gotchiId = gotchiId;

        // try get data
        try
        {
            var responseStr = await Dropt.Utils.Http.GetRequest(dbUri + "/gotchis/" + gotchiId.ToString());
            if (!string.IsNullOrEmpty(responseStr))
            {
                var data = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                ectoDungeonStartAmount_offchain.Value = data.ecto_dungeon_start_amount;
                bombDungeonCapacity_offchain.Value = data.bomb_dungeon_capacity;
                healSalveDungeonCharges_offchain.Value = data.heal_salve_dungeon_charges;
                isEssenceInfused_offchain.Value = data.is_essence_infused;
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
                heal_salve_dungeon_charges = 0,
                is_essence_infused = false
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/gotchis", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var data = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                ectoDungeonStartAmount_offchain.Value = data.ecto_dungeon_start_amount;
                bombDungeonCapacity_offchain.Value = data.bomb_dungeon_capacity;
                healSalveDungeonCharges_offchain.Value = data.heal_salve_dungeon_charges;
                isEssenceInfused_offchain.Value = data.is_essence_infused;
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void UpdateDebugTopUps()
    {
        if (!IsClient) return;

        // ecto top up
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            DebugTopUpWalletDataServerRpc(3, 0, 0);
        }

        // dust top up
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DebugTopUpWalletDataServerRpc(0, 10, 0);
        }

        // bomb top up
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            DebugTopUpWalletDataServerRpc(0, 0, 1);
        }
    }

    [Rpc(SendTo.Server)]
    void DebugTopUpWalletDataServerRpc(int ectoDelta, int dustDelta, int bombDelta)
    {
        DebugTopUpWalletDataServerRpcAsync(ectoDelta, dustDelta, bombDelta);
    }

    async UniTaskVoid DebugTopUpWalletDataServerRpcAsync(int ectoDelta, int dustDelta, int bombDelta)
    {
        try
        {
            var json = JsonUtility.ToJson(new WalletDelta_Data
            {
                ecto_delta = ectoDelta,
                dust_delta = dustDelta,
                bomb_delta = bombDelta
            });

            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets/delta/" + m_walletAddress, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data data = JsonUtility.FromJson<Wallet_Data>(responseStr);
                ectoBalance_offchain.Value = data.ecto_balance;
                dustBalance_offchain.Value = data.dust_balance;
                bombBalance_offchain.Value = data.bomb_balance;
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

        dustCount_dungeon.Value += value;
    }

    // Method to add value to CGHSTCount
    public void AddDungeonEcto(int value)
    {
        if (!IsServer) return;

        ectoCount_dungeon.Value += value;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        GetComponent<PlayerCharacter>().Essence.Value += value;
    }

    [System.Serializable]
    public class Wallet_Data
    {
        public string id;
        public int ecto_balance;
        public int dust_balance;
        public int bomb_balance;
    }

    [System.Serializable]
    public class Gotchi_Data
    {
        public int id;
        public int ecto_dungeon_start_amount;
        public int bomb_dungeon_capacity;
        public int heal_salve_dungeon_charges;
        public bool is_essence_infused;
    }

    [System.Serializable]
    public class WalletDelta_Data
    {
        public int ecto_delta;
        public int dust_delta;
        public int bomb_delta;
    }
}
