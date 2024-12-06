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

    private IDoorAnimation m_doorAnimation;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_doorAnimation = GetComponent<IDoorAnimation>();
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
        if (IsServer)
        {
            PlayerOffchainData playerDungeonData = localPlayerController.GetComponent<PlayerOffchainData>();
            if (playerDungeonData.DoWeHaveEctoGraterThanOrEqualTo(m_costToOpenTheDoor))
            {
                NotifyMessageClientRpc("You do not have enough Ecto to use this door");
            }
            else
            {
                NotifyMessageClientRpc("Hold F to use Ecto to open this door");
            }
        }
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();
        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
    }

    [Rpc(SendTo.Server)]
    private void TryOpenDoorServerRpc(ulong playerNetworkObjectId)
    {
        _ = TryOpenDoorServerRpcAsync(playerNetworkObjectId);
    }

    private async UniTaskVoid TryOpenDoorServerRpcAsync(ulong playerNetworkObjectId)
    {
        PlayerController playerController = GetPlayerController(playerNetworkObjectId);
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not in range of EctoDoor");
            return;
        }

        PlayerOffchainData playerDungeonData = playerController.GetComponent<PlayerOffchainData>();
        bool isSuccess = await playerDungeonData.RemoveEcto(m_costToOpenTheDoor);
        if (!isSuccess)
        {
            //NotifyNotEnoughBalanceClientRpc();
            return;
        }

        m_doorAnimation.OpenDoor();
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
    private void NotifyMessageClientRpc(string message)
    {
        if (!GetPlayerController(localPlayerNetworkObjectId).IsLocalPlayer)
        {
            return;
        }
        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(message, interactableType);
    }
}