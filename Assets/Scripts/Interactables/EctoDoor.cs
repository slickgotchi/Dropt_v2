using UnityEngine;
using TMPro;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class EctoDoor : Interactable
{
    [SerializeField] private int m_costToOpenTheDoor;
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private GameObject m_costIcon;
    [SerializeField] private BoxCollider2D m_leftOpenCollider;
    [SerializeField] private BoxCollider2D m_rightOpenCollider;
    [SerializeField] private BoxCollider2D m_closeCollider;

    private Animator m_animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_animator = GetComponent<Animator>();
        m_costText.text = m_costToOpenTheDoor.ToString();
    }

    public override void OnInteractHoldFinish()
    {
        base.OnInteractHoldFinish();
        TryOpenDoorServerRpc(localPlayerNetworkObjectId);
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
    }

    [Rpc(SendTo.Server)]
    private void TryOpenDoorServerRpc(ulong playerNetworkObjectId)
    {
        TryOpenDoorServerRpcAsync(playerNetworkObjectId);
    }

    private async UniTaskVoid TryOpenDoorServerRpcAsync(ulong playerNetworkObjectId)
    {
        var playerController = GetPlayerController(playerNetworkObjectId);
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not in range of EctoDoor");
            return;
        }

        var playerDungeonData = playerController.GetComponent<PlayerOffchainData>();

        bool isSuccess = await playerDungeonData.RemoveEcto(m_costToOpenTheDoor);
        if (!isSuccess)
        {
            NotifyNotEnoughBalanceClientRpc();
            return;
        }

        m_animator.Play("ApeDoor_Open");
        OpenDoorClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OpenDoorClientRpc()
    {
        m_costText.gameObject.SetActive(false);
        m_costIcon.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        m_closeCollider.enabled = false;
        m_leftOpenCollider.enabled = true;
        m_rightOpenCollider.enabled = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyNotEnoughBalanceClientRpc()
    {
        if (!GetPlayerController(localPlayerNetworkObjectId).IsLocalPlayer)
        {
            return;
        }
        NotifyCanvas.Instance.SetVisible($"You do not have enough Ecto to open this door");
    }
}