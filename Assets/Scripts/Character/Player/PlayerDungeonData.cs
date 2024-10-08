using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Thirdweb;

// cGHST bank logic
// - cGhstVillageBank is the total cGhst you have when in the village
// - cGhstDungeonBank is the amount of cGhst from your bank that you can start a dungeon with (automatically available)
// - cGhstDungeonFound is the cGhst collected as you explore the dungeon
// - players automatically enter dungeon by withdrawing from their bank up to cGhstDungeonStartAmount
// - when players have more than their wallet amount

public class PlayerDungeonData : NetworkBehaviour
{
    // Public properties with private setters
    public NetworkVariable<int> SpiritDust = new NetworkVariable<int>(0);
    public NetworkVariable<int> cGHST = new NetworkVariable<int>(0);
    public NetworkVariable<float> Essence = new NetworkVariable<float>(300);

    public int cGhstDungeonStartAmount = 10;

    public NetworkVariable<int> cGhstVillageBank = new NetworkVariable<int>(0);
    public NetworkVariable<int> cGhstDungeonBank = new NetworkVariable<int>(0);
    public NetworkVariable<int> cGhstDungeon = new NetworkVariable<int>(0);

    private string m_walletAddress;
    private float k_walletUpdateInterval = 0.2f;
    private float m_walletUpdateTimer = 0f;

    private void Update()
    {
        UpdateEssence();
        UpdateGameOver();
        UpdateWalletData();
        UpdateDebugTopUps();
    }

    private void UpdateEssence()
    {
        if (!IsServer) return;

        Essence.Value -= Time.deltaTime;

        if (LevelManager.Instance.IsDegenapeVillage())
        {
            Essence.Value = 5;
        }
    }

    private void UpdateGameOver()
    {
        if (!IsServer) return;

        if (Essence.Value <= 0 && !LevelManager.Instance.IsDegenapeVillage())
        {
            GetComponent<PlayerController>().KillPlayer(REKTCanvas.TypeOfREKT.Essence);
        }
    }

    private async void UpdateWalletData()
    {
        if (!IsLocalPlayer) return;

        m_walletUpdateTimer -= Time.deltaTime;
        if (m_walletUpdateTimer > 0) return;
        m_walletUpdateTimer = k_walletUpdateInterval;

        try
        {
            // address check
            var address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            if (address != m_walletAddress && address != null)
            {
                m_walletAddress = address;

                CreateOrFetchDroptWalletDataServerRpc(m_walletAddress);
            }
        }
        catch (System.Exception e)
        {
            // don't do anything, if we got here it just means we don't have a wallet account
        }
    }

    private void UpdateDebugTopUps()
    {
        if (!IsClient) return;

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            DebugTopUpsServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void DebugTopUpsServerRpc()
    {
        cGHST.Value = 100;
        SpiritDust.Value = 100;
    }

    [Rpc(SendTo.Server)]
    void CreateOrFetchDroptWalletDataServerRpc(string walletAddress)
    {
        CreateOrFetchDroptWalletData(walletAddress);
    }

    async void CreateOrFetchDroptWalletData(string walletAddress)
    {
        try
        {
            // fetch wallet data (or create new)
            var fetchResponse = await DroptWalletAPI.FetchDroptWalletDataAsync(walletAddress);

            // if no data, make a new database entry
            if (fetchResponse == null)
            {
                DroptWalletData droptData = new DroptWalletData
                {
                    walletAddress = m_walletAddress,
                    cGhst = 0,
                    spiritDust = 0,
                };

                var createResponse = await DroptWalletAPI.CreateDroptWalletDataAsync(droptData);

                cGHST.Value = 0;
                SpiritDust.Value = 0;
            }

            // if got data, populate cghst and gltr
            else
            {
                cGHST.Value = fetchResponse.cGhst;
                SpiritDust.Value = fetchResponse.spiritDust;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    

    // Method to add value to GltrCount
    public void AddGltr(int value)
    {
        if (!IsServer) return;

        SpiritDust.Value += value;
    }

    // Method to add value to CGHSTCount
    public void AddCGHST(int value)
    {
        if (!IsServer) return;

        cGHST.Value += value;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        Essence.Value += value;
    }

    // Method to reset counts
    public void Reset()
    {
        if (!IsServer) return;

        SpiritDust.Value = 0;
        cGHST.Value = 0;
        Essence.Value = 300;
    }
}
