using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CGHSTDoor : Interactable
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
        TryToOpenTheDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryToOpenTheDoorServerRpc()
    {
        PlayerController playerController = GetPlayerController();
        PlayerOffchainData playerDungeonData = playerController.GetComponent<PlayerOffchainData>();

        bool isSuccess = playerDungeonData.TrySpendDungeonEcto(m_costToOpenTheDoor);

        if (!isSuccess)
        {
            NotifyNotEnoughBalanceClientRpc();
            return;
        }

        m_animator.Play("ApeDoor_Open");
        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        m_costText.gameObject.SetActive(false);
        m_costIcon.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        m_closeCollider.enabled = false;
        m_leftOpenCollider.enabled = true;
        m_rightOpenCollider.enabled = true;
    }

    [ClientRpc]
    private void NotifyNotEnoughBalanceClientRpc()
    {
        if (!GetPlayerController().IsLocalPlayer)
        {
            return;
        }
        NotifyCanvas.Instance.SetVisible($"You do not have enough CGHST to open the door");
    }
}