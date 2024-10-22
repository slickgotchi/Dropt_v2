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

    public override void OnHoldFinishInteraction()
    {
        base.OnHoldFinishInteraction();
        TryToOpenTheDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryToOpenTheDoorServerRpc()
    {
        PlayerController playerController = GetPlayerController();
        PlayerOffchainData playerDungeonData = playerController.GetComponent<PlayerOffchainData>();
        int cGhst = playerDungeonData.ectoCount_dungeon.Value;

        if (m_costToOpenTheDoor > cGhst)
        {
            NotifyNotEnoughBalanceClientRpc();
            return;
        }
        playerDungeonData.ectoCount_dungeon.Value -= m_costToOpenTheDoor;
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