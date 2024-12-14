using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class SpriteDoorAnimation : NetworkBehaviour, IDoorAnimation
{
    [SerializeField] private Transform m_leftDoor;
    [SerializeField] private Transform m_rightDoor;
    [SerializeField] private float m_leftLimit;
    [SerializeField] private float m_rightLimit;

    public void OpenDoor()
    {
        OpenDoorClientRpc();
    }

    [ClientRpc]
    public void OpenDoorClientRpc()
    {
        _ = m_leftDoor.DOLocalMoveX(m_leftLimit, 0.25f).SetEase(Ease.OutQuint);
        _ = m_rightDoor.DOLocalMoveX(m_rightLimit, 0.25f).SetEase(Ease.OutQuint);
    }
}
