using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Thirdweb;
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
    public NetworkVariable<int> ectoDebitStartCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> ectoDebitCount_dungeon = new NetworkVariable<int>(0);       // this is the ecto out of your offchain bank account you start with
    public NetworkVariable<int> ectoLiveCount_dungeon = new NetworkVariable<int>(0);        // this is the ecto that gets added to as you collect ecto, starts at 0
    public NetworkVariable<int> dustLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombStartCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> bombLiveCount_dungeon = new NetworkVariable<int>(0);
    public NetworkVariable<int> healSalveChargeCount_dungeon = new NetworkVariable<int>(0);

    // if player dies
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain
    // if player escapes
    //      ectoBankDelta = ectoDebitCount_dungeon - ectoDungeonStartMount_offchain + ectoLiveCount_dungeon

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

    // isEnteredDungeon
    private bool m_isEnteredDungeon = false;
    //private bool m_isDegenapeVillage = true;

    public override void OnNetworkSpawn()
    {
        m_walletAddress = null;
        m_gotchiId = 0;
    }

    public override void OnNetworkDespawn()
    {
        // WARNING: we need a way to differentiate between full disconnects and temporary internet loss disconnect/reconnects
        if (IsServer)
        {
            ExitDungeonCalculateBalances(false);
        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            CheckWalletAddressChanged();
            CheckGotchiIdChanged();
            UpdateDebugTopUps();
        }

        if (IsServer)
        {
            CheckIsEnteredDungeon();
        }
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
                bomb_balance = 0,
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data data = JsonUtility.FromJson<Wallet_Data>(responseStr);
                ectoBalance_offchain.Value = data.ecto_balance;
                bombBalance_offchain.Value = data.bomb_balance;
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
            //PlayerPrefs.SetInt("GotchiId", m_gotchiId);
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
                dustBalance_offchain.Value = data.dust_balance;
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
                heal_salve_dungeon_charges = 1,
                dust_balance = 0,
                is_essence_infused = false
            });
            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/gotchis", json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                var data = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                ectoDungeonStartAmount_offchain.Value = data.ecto_dungeon_start_amount;
                bombDungeonCapacity_offchain.Value = data.bomb_dungeon_capacity;
                healSalveDungeonCharges_offchain.Value = data.heal_salve_dungeon_charges;
                dustBalance_offchain.Value = data.dust_balance;
                isEssenceInfused_offchain.Value = data.is_essence_infused;
                Debug.Log("Created new database entry for gotchi: " + gotchiId);
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    void CheckIsEnteredDungeon()
    {
        if (!IsServer) return;

        var isDegenapeVillage = LevelManager.Instance.IsDegenapeVillage();
        if (isDegenapeVillage) m_isEnteredDungeon = false;

        if (!m_isEnteredDungeon && !isDegenapeVillage)
        {
            EnterDungeonCalculateBalances();
            m_isEnteredDungeon = true;
        }
    }

    // enter dungeon method that calculates balance
    public void EnterDungeonCalculateBalances()
    {
        Debug.Log("EnterDungeonCalculateBalances()");

        if (!IsServer) return;

        ectoDebitStartCount_dungeon.Value = math.min(ectoDungeonStartAmount_offchain.Value, ectoBalance_offchain.Value);
        ectoDebitCount_dungeon.Value = ectoDebitStartCount_dungeon.Value;
        ectoLiveCount_dungeon.Value = 0;

        dustLiveCount_dungeon.Value = 0;

        bombStartCount_dungeon.Value = math.min(bombDungeonCapacity_offchain.Value, bombBalance_offchain.Value);
        bombLiveCount_dungeon.Value = bombStartCount_dungeon.Value;

        healSalveChargeCount_dungeon.Value = healSalveDungeonCharges_offchain.Value;
    }

    // exit dungeon calculates new balances and updates the database
    public async void ExitDungeonCalculateBalances(bool isEscaped)
    {
        Debug.Log("ExitDungeonCalculateBalances()");

        if (!IsServer) return;

        var postDungeonEctoDelta = isEscaped ? ectoDebitCount_dungeon.Value - ectoDebitStartCount_dungeon.Value + ectoLiveCount_dungeon.Value : ectoDebitCount_dungeon.Value - ectoDungeonStartAmount_offchain.Value;
        var postDungeonDustDelta = isEscaped ? (int)(dustLiveCount_dungeon.Value * CodeInjector.Instance.GetOutputMultiplier()) : 0;
        var postDungeonBombDelta = bombLiveCount_dungeon.Value - bombStartCount_dungeon.Value;

        // log wallet deltas
        try
        {
            await LogWalletDeltaDataServerRpcAsync(postDungeonEctoDelta, postDungeonBombDelta);

            // successfully logged deltas so zero all the balances
            ectoDebitStartCount_dungeon.Value = 0;
            ectoDebitCount_dungeon.Value = 0;
            ectoLiveCount_dungeon.Value = 0;
            bombStartCount_dungeon.Value = 0;
            bombLiveCount_dungeon.Value = 0;

        } catch
        {
            Debug.LogWarning("Could not log post-dungeon wallet delta data, is server running?");
        }

        // log gotchi deltas
        try
        {
            await LogGotchiDeltaDataServerRpcAsync(GetComponent<PlayerController>().NetworkGotchiId.Value, postDungeonDustDelta);

            dustLiveCount_dungeon.Value = 0;
        } catch
        {
            Debug.LogWarning("Could not log post-dungeon gotchi delta data, is server running?");
        }
    }

    private void UpdateDebugTopUps()
    {
        if (!IsLocalPlayer) return;

        // ecto top up
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            DebugTopUpWalletDataServerRpc(3, 0);
        }

        // dust top up
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DebugTopUpGotchiDataServerRpc(GetComponent<PlayerController>().NetworkGotchiId.Value, 10);
        }

        // bomb top up
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            DebugTopUpWalletDataServerRpc(0, 1);
        }
    }

    [Rpc(SendTo.Server)]
    void DebugTopUpWalletDataServerRpc(int ectoDelta, int bombDelta)
    {
        LogWalletDeltaDataServerRpcAsync(ectoDelta, bombDelta);
    }

    [Rpc(SendTo.Server)]
    void DebugTopUpGotchiDataServerRpc(int gotchiId, int dustDelta)
    {
        LogGotchiDeltaDataServerRpcAsync(gotchiId, dustDelta);
    }

    async UniTask LogWalletDeltaDataServerRpcAsync(int ectoDelta, int bombDelta)
    {
        try
        {
            var json = JsonUtility.ToJson(new WalletDelta_Data
            {
                ecto_delta = ectoDelta,
                bomb_delta = bombDelta
            });

            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/wallets/delta/" + m_walletAddress, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Wallet_Data data = JsonUtility.FromJson<Wallet_Data>(responseStr);
                ectoBalance_offchain.Value = data.ecto_balance;
                bombBalance_offchain.Value = data.bomb_balance;
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    async UniTask LogGotchiDeltaDataServerRpcAsync(int gotchiId, int dustDelta)
    {
        try
        {
            var json = JsonUtility.ToJson(new GotchiDelta_Data
            {
                dust_delta = dustDelta,
            });

            var responseStr = await Dropt.Utils.Http.PostRequest(dbUri + "/gotchis/delta/" + gotchiId, json);
            if (!string.IsNullOrEmpty(responseStr))
            {
                Gotchi_Data data = JsonUtility.FromJson<Gotchi_Data>(responseStr);
                dustBalance_offchain.Value = data.dust_balance;
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

        dustLiveCount_dungeon.Value += value;
    }

    // Method to add value to ecto
    public void AddDungeonEcto(int value)
    {
        if (!IsServer) return;

        ectoLiveCount_dungeon.Value += value;
    }

    // Method to remove ecto
    public bool TrySpendDungeonEcto(int value)
    {
        if (!IsServer) return false;

        var ectoLive = ectoLiveCount_dungeon.Value;
        var ectoDebit = ectoDebitCount_dungeon.Value;

        // check if we have enough ecto
        if ((ectoLive + ectoDebit) < value) return false;

        // spend down all ectoLive value
        var debitDelta = ectoLive - value;

        if (debitDelta < 0)
        {
            ectoLiveCount_dungeon.Value = 0;
            ectoDebitCount_dungeon.Value += debitDelta;
        } else
        {
            ectoLiveCount_dungeon.Value -= value;
        }

        return true;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        GetComponent<PlayerCharacter>().Essence.Value += value;
        PopupEssenceTextClientRpc(value, GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    void PopupEssenceTextClientRpc(float value, ulong networkObjectId)
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
}
