using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class JoostInteractable : Interactable
{
    public Joost.Type type = Joost.Type.Null;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    public int NumberLevels = 3;

    private Sprite m_sprite;
    private string m_name;
    private string m_description;
    private int m_cost;
    private BuffObject m_buffObject;

    private void Start()
    {
        Init(type);
    }

    public void Init(Joost.Type joostType)
    {
        if (JoostDataManager.Instance == null)
        {
            Debug.Log("no joost data manager");
            return;
        }

        var joostData = JoostDataManager.Instance.GetJoostData(joostType);
        m_sprite = joostData.sprite;
        m_name = AddSpacesToCamelCase(joostType.ToString());
        m_description = joostData.description;
        m_cost = joostData.cost;
        m_buffObject = joostData.buffObject;

        m_spriteRenderer.sprite = m_sprite;
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        if (localPlayerController == null) return;

        if (IsClient)
        {
            JoostInteractionCanvas.Instance.Container.SetActive(true);

            bool isBuffAlreadyOnLocalPlayer = HasLocalPlayerGotBuff();
            if (isBuffAlreadyOnLocalPlayer)
            {
                PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
                JoostInteractionCanvas.Instance.Set(m_name, m_description, m_cost.ToString(), JoostInteractionCanvas.PurchaseState.Consumed);
                return;
            }
            else
            {
                var playerOffchainData = localPlayerController.GetComponent<PlayerOffchainData>();
                if (playerOffchainData == null) { Debug.LogWarning("playerOffchainData = null"); return; }

                bool isLocalPlayerEctoSufficient = playerOffchainData.IsDungeonEctoGreaterThanOrEqualTo(m_cost);

                if (isLocalPlayerEctoSufficient)
                {
                    PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
                    JoostInteractionCanvas.Instance.Set(m_name, m_description, m_cost.ToString(), JoostInteractionCanvas.PurchaseState.Available);
                }
                else
                {
                    PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
                    JoostInteractionCanvas.Instance.Set(m_name, m_description, m_cost.ToString(), JoostInteractionCanvas.PurchaseState.InsufficientEcto);
                }
            }
        }

    }

    bool HasLocalPlayerGotBuff()
    {
        // see if we already have the buff
        var localPlayerNetworkCharacter = localPlayerController.GetComponent<NetworkCharacter>();
        if (localPlayerNetworkCharacter == null) return false;

        return localPlayerNetworkCharacter.HasBuffName(m_buffObject.name);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        bool hasBuff = HasLocalPlayerGotBuff();
        if (!hasBuff)
        {
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);

        }

        JoostInteractionCanvas.Instance.Container.SetActive(false);
    }

    public override void OnInteractHoldFinish()
    {
        TryAddJoostBuffServerRpc(localPlayerNetworkObjectId);

    }

    [Rpc(SendTo.Server)]
    void TryAddJoostBuffServerRpc(ulong playerNetworkObjectId)
    {
        TryAddJoostBuffServerRpcAsync(playerNetworkObjectId);
    }

    private async UniTaskVoid TryAddJoostBuffServerRpcAsync(ulong playerNetworkObjectId)
    {
        if (!IsServer) return;

        var playerController = GetPlayerController(playerNetworkObjectId);
        if (playerController == null) { Debug.LogWarning("TryAddJoostBuffServerRpcAsync: playerController = null"); return; }

        var networkCharacter = playerController.GetComponent<NetworkCharacter>();
        if (networkCharacter == null) { Debug.LogWarning("TryAddJoostBuffServerRpcAsync: networkCharacter = null"); return; }

        var levelCountedBuffObject = new GameObject();
        var levelCountedBuff = levelCountedBuffObject.AddComponent<LevelCountedBuff>();

        bool isBuffAlreadyOnPlayer = levelCountedBuff.IsBuffAlreadyOnPlayer(m_buffObject, networkCharacter);
        if (isBuffAlreadyOnPlayer) { Debug.Log("TryAddJoostBuffServerRpcAsync: Player already has buff: " + m_buffObject.name); return; }

        var playerDungeonData = playerController.GetComponent<PlayerOffchainData>();
        if (playerDungeonData == null) Debug.LogWarning("TryAddJoostBuffServerRpcAsync: playerDungeonData = null");

        bool isSufficientEcto = playerDungeonData.RemoveDungeonEcto(m_cost);
        if (!isSufficientEcto) { Debug.Log("TryAddJoostBuffServerRpcAsync: Insufficient Ecto"); return; }

        bool isBuffAdded = levelCountedBuff.TryAddBuffToPlayer(m_buffObject, playerController.GetComponent<NetworkCharacter>(), NumberLevels);
        if (!isBuffAdded) { Debug.LogWarning("TryAddJoostBuffServerRpcAsync: Buff could not be added to player"); return; }

        // tell client purchase was successful
        Debug.Log("TryAddJoostBuffServerRpcAsync: Buff added to player, sending confirmation to client");
        ConfirmJoostBuffAddedClientRpc(playerNetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ConfirmJoostBuffAddedClientRpc(ulong playerNetworkObjectId)
    {
        if (localPlayerController == null)
        {
            Debug.Log("ConfirmJoostBuffClientRpc: localPlayerControl = null");
            return;
        }

        var networkObject = localPlayerController.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.Log("ConfirmJoostBuffClientRpc: localPlayerNetworkObject = null");
            return;
        }

        if (networkObject.NetworkObjectId == playerNetworkObjectId)
        {
            JoostInteractionCanvas.Instance.Set(m_name, m_description, m_cost.ToString(), JoostInteractionCanvas.PurchaseState.Consumed);
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        }
    }

    private string AddSpacesToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
            {
                newText.Append(' ');
            }
            newText.Append(text[i]);
        }

        return newText.ToString();
    }
}

namespace Joost
{
    public enum Type
    {
        Null,
        FrozenSlush, GalaxyJuice, WarmMilk, GallonOfMilk, CupOfCoffee,
        ChocolateMartini, ChocolateMilkshake, BatteryJuice, Glue,
        KeroseneKola, AbsintheForAlgernon, Frugalccino,
        DeadMansGambit, HammurabisGambit, ChocoCritCookies,
        EpoxyEcclair, KeroseneCake, FreakyFruitCake
    }
}
