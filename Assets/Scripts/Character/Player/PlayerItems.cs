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

    [SerializeField] private GameObject m_zenCricketEffect;
    [SerializeField] private GameObject m_bombPrefab;
    [SerializeField] private GameObject m_holePrefab;

    private PlayerGotchi m_playerGotchi;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_playerOffchainData = GetComponent<PlayerOffchainData>();
        m_soundFX_Player = GetComponent<SoundFX_Player>();
        if (!IsLocalPlayer) return;
        InitializeInputActions();
        m_playerGotchi = GetComponent<PlayerGotchi>();
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

    Vector3 GetPlacementOffset(PlayerGotchi.Facing facing)
    {
        switch (facing)
        {
            case PlayerGotchi.Facing.Front: return new Vector3(0, -0.5f, 0);
            case PlayerGotchi.Facing.Back: return new Vector3(0, 1.5f, 0);
            case PlayerGotchi.Facing.Left: return new Vector3(-1.5f, 0.5f, 0);
            case PlayerGotchi.Facing.Right: return new Vector3(1.5f, 0.5f, 0);
            default: break;
        }

        return Vector3.zero;
    }


    #region Input Action Listners

    private void OnUseItem1Performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Use Item1");
        UseBombItemServerRpc(GetPlacementOffset(m_playerGotchi.GetFacing()));
    }

    private void OnUseItem2Performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Use Item2");
        UsePortaHoleItemServerRpc(GetPlacementOffset(m_playerGotchi.GetFacing()));
    }

    private void OnUseItem3Performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Use Item3");
        UseZenCricketItemServerRpc();
    }

    private void OnUseItem4Performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Use Item4");
    }

    #endregion

    private bool IsInDungeons()
    {
        return !LevelManager.Instance.IsDegenapeVillage();
    }

    #region Bomb Item

    [ServerRpc]
    private void UseBombItemServerRpc(Vector3 placementOffset)
    {
        if (!IsInDungeons())
        {
            WarningClientRpc("Bomb item can only be used in dungeons");
            return;
        }

        var success = m_playerOffchainData.TryUseDungeonBomb();
        if (success)
        {
            GameObject bombItem = Instantiate(m_bombPrefab,
                transform.position + placementOffset, Quaternion.identity);
            bombItem.GetComponent<BombItem>().OwnerId = GetComponent<NetworkObject>().NetworkObjectId;
            bombItem.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            DebugLogClientRpc("You Dont Have Bomb Items.");
        }
    }

    #endregion

    #region PortaHole Item

    [ServerRpc]
    private void UsePortaHoleItemServerRpc(Vector3 placementOffset)
    {
        if (!IsInDungeons())
        {
            WarningClientRpc("Port-a-Hole item can only be used in dungeons");
            return;
        }

        var success = m_playerOffchainData.TryUseDungeonPortaHole();
        if (success)
        {
            GameObject holeItem = Instantiate(m_holePrefab,
                transform.position + placementOffset, Quaternion.identity);
            holeItem.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            DebugLogClientRpc("You Dont Have PortaHole Items.");
        }
    }

    #endregion

    #region HealSalve Item

    [ServerRpc]
    private void UseZenCricketItemServerRpc()
    {
        if (!IsInDungeons())
        {
            WarningClientRpc("Zen Cricket item can only be used in dungeons");
            return;
        }

        PlayerCharacter playerCharacter = GetComponent<PlayerCharacter>();
        if (playerCharacter != null)
        {
            if (playerCharacter.IsHpFullyCharged()) return;

            var success = m_playerOffchainData.TryUseDungeonZenCricket();
            if (success)
            {
                playerCharacter.RecoverHealthByPercentageOfTotalHp(40);
                GenerateHealSalveEffectClientRpc();
            }
            else
            {
                DebugLogClientRpc("You don't have any zen crickets");
            }
        }
    }

    [ClientRpc]
    private void GenerateHealSalveEffectClientRpc()
    {
        GameObject effect = Instantiate(m_zenCricketEffect, transform);
        effect.transform.localPosition = new Vector3(0, 0.5f, 0);
        Destroy(effect, 3.0f);
        m_soundFX_Player.PlayHealSound();
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

    [ClientRpc]
    private void DebugLogClientRpc(string message)
    {
        if (!IsLocalPlayer) return;
        Debug.Log(message);
    }
}