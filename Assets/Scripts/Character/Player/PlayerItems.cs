using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine;

public class PlayerItems : NetworkBehaviour
{
    private InputAction m_useItem1;
    private InputAction m_useItem2;
    private InputAction m_useItem3;
    private InputAction m_useItem4;

    private PlayerOffchainData m_playerOffchainData;
    private SoundFX_Player m_soundFX_Player;

    [SerializeField] private GameObject m_healSlaveEffect;
    [SerializeField] private GameObject m_bombObject;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_playerOffchainData = GetComponent<PlayerOffchainData>();
        m_soundFX_Player = GetComponent<SoundFX_Player>();
        if (!IsLocalPlayer) return;
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;

        m_useItem1 = inputActions.FindAction("UseItem_1");
        m_useItem2 = inputActions.FindAction("UseItem_2");
        m_useItem3 = inputActions.FindAction("UseItem_3");
        m_useItem4 = inputActions.FindAction("UseItem_4");

        m_useItem1.performed += OnUseItem1Performed;
        m_useItem2.performed += OnUseItem2Performed;
        m_useItem3.performed += OnUseItem3Performed;
        m_useItem4.performed += OnUseItem4Performed;

        m_useItem1.Enable();
        m_useItem2.Enable();
        m_useItem3.Enable();
        m_useItem4.Enable();
    }

    #region Input Action Listners

    private void OnUseItem1Performed(InputAction.CallbackContext obj)
    {
        UseHealSalveItemServerRpc();
    }

    private void OnUseItem2Performed(InputAction.CallbackContext obj)
    {
        UseBombItemServerRpc();
    }

    private void OnUseItem3Performed(InputAction.CallbackContext obj)
    {
    }

    private void OnUseItem4Performed(InputAction.CallbackContext obj)
    {
    }

    #endregion

    private bool IsInDungeons()
    {
        return !LevelManager.Instance.IsDegenapeVillage();
    }

    #region HealSalve Item

    [ServerRpc]
    private void UseHealSalveItemServerRpc()
    {
        if (!IsInDungeons())
        {
            WarningClientRpc("Heal Salve Can Only Be Used In Dungeons");
            return;
        }

        if (!IsHealSalveChargeAvailable())
        {
            WarningClientRpc("You Dont Have Heal Items.");
            return;
        }

        PlayerCharacter playerCharacter = GetComponent<PlayerCharacter>();
        if (playerCharacter.IsHpFullyCharged()) return;
        playerCharacter.RecoverHealthByPercentageOfTotalHp(40);
        m_playerOffchainData.UseHealSalveItem();
        UpdateHealSalveItemClientRpc(m_playerOffchainData.healSalveChargeCount_dungeon.Value,
                                     m_playerOffchainData.healSalveDungeonCharges_offchain.Value);
        GenerateHealSalveEffectClientRpc();
    }

    [ClientRpc]
    private void UpdateHealSalveItemClientRpc(int currentHealCharge, int maxHealCharge)
    {
        if (!IsLocalPlayer) return;
        PlayerHUDCanvas.Instance.UpdateHealSlaveUpItem(currentHealCharge, maxHealCharge);
    }

    private bool IsHealSalveChargeAvailable()
    {
        return m_playerOffchainData.healSalveChargeCount_dungeon.Value > 0;
    }

    [ClientRpc]
    private void GenerateHealSalveEffectClientRpc()
    {
        GameObject effect = Instantiate(m_healSlaveEffect, transform);
        effect.transform.localPosition = new Vector3(0, 0.5f, 0);
        Destroy(effect, 3.0f);
        m_soundFX_Player.PlayHealSound();
    }

    #endregion

    #region Bomb Item

    [ServerRpc]
    private void UseBombItemServerRpc()
    {
        if (!IsInDungeons())
        {
            WarningClientRpc("Bomb Item Can Only Be Used In Dungeons");
            return;
        }

        if (!IsBombAvailable())
        {
            WarningClientRpc("You Dont Have Bomb Items.");
            return;
        }

        m_playerOffchainData.UseBombItem();
        PlaceBomb();
    }

    private bool IsBombAvailable()
    {
        return m_playerOffchainData.IsBombAvailable();
    }

    private void PlaceBomb()
    {
        GameObject bombItem = Instantiate(m_bombObject, transform.position, Quaternion.identity);
        bombItem.GetComponent<BombItem>().OwnerId = GetComponent<NetworkObject>().NetworkObjectId;
        NetworkObject networkObject = bombItem.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }

    #endregion

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsLocalPlayer) return;

        m_useItem1.performed -= OnUseItem1Performed;
        m_useItem2.performed -= OnUseItem2Performed;
        m_useItem3.performed -= OnUseItem3Performed;
        m_useItem4.performed -= OnUseItem4Performed;

        m_useItem1.Disable();
        m_useItem2.Disable();
        m_useItem3.Disable();
        m_useItem4.Disable();
    }

    [ClientRpc]
    private void WarningClientRpc(string message)
    {
        if (!IsLocalPlayer) return;
        NotifyCanvas.Instance.SetVisible(message);
    }
}