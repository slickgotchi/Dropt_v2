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
        PlayerOffchainData playerDungeonData = localPlayerController.GetComponent<PlayerOffchainData>();
        if (playerDungeonData.DoWeHaveEctoGraterThanOrEqualTo(m_costToOpenTheDoor))
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii("You do not have enough Ecto to use this door",
                                                                 interactableType);
        }
        else
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii("Hold F to use Ecto to open this door",
                                                                 interactableType);
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
        _ = TryOpenDoorAsync(playerNetworkObjectId);
    }

    private async UniTaskVoid TryOpenDoorAsync(ulong playerNetworkObjectId)
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
        if (IsServer && !IsHost)
        {
            OpenDoor();
        }
        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        OpenDoor();
    }

    private void OpenDoor()
    {
        m_costText.gameObject.SetActive(false);
        m_costIcon.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        m_closeCollider.enabled = false;
        m_leftOpenCollider.enabled = true;
        m_rightOpenCollider.enabled = true;
    }
}