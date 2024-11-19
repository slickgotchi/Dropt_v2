using UnityEngine;
using DG.Tweening;

public class CrystalDoor : Door<CrystalDoorType>
{
    [SerializeField] private SpriteRenderer m_dooricon;
    [SerializeField] private Transform _leftDoor;
    [SerializeField] private Transform _rightDoor;

    [SerializeField] private Sprite m_R_Open;
    [SerializeField] private Sprite m_R_Closed;

    [SerializeField] private Sprite m_Ghost_Open;
    [SerializeField] private Sprite m_Ghost_Closed;

    [SerializeField] private Sprite m_Snake_Open;
    [SerializeField] private Sprite m_Snake_Closed;

    [SerializeField] private Sprite m_Cross_Open;
    [SerializeField] private Sprite m_Cross_Closed;

    [SerializeField] private Sprite m_Moustache_Open;
    [SerializeField] private Sprite m_Moustache_Closed;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetDoorIcon();
        State.OnValueChanged += OnDoorStateChange;
    }

    private void OnDoorStateChange(DoorState previousValue, DoorState newValue)
    {
        SetDoorIcon();
    }

    public override void OpenDoorAnimation()
    {
        _ = _leftDoor.DOLocalMoveX(-1, 0.3f).SetEase(Ease.OutQuint);
        _ = _rightDoor.DOLocalMoveX(1, 0.3f).SetEase(Ease.OutQuint);
    }

    private void SetDoorIcon()
    {
        if (State.Value == DoorState.Open)
        {
            switch (Type.Value)
            {
                case CrystalDoorType.R:
                    m_dooricon.sprite = m_R_Open;
                    break;
                case CrystalDoorType.Ghost:
                    m_dooricon.sprite = m_Ghost_Open;
                    break;
                case CrystalDoorType.Snake:
                    m_dooricon.sprite = m_Snake_Open;
                    break;
                case CrystalDoorType.Cross:
                    m_dooricon.sprite = m_Cross_Open;
                    break;
                case CrystalDoorType.Moustache:
                    m_dooricon.sprite = m_Moustache_Open;
                    break;
            }
        }
        else if (State.Value == DoorState.Closed)
        {
            switch (Type.Value)
            {
                case CrystalDoorType.R:
                    m_dooricon.sprite = m_R_Closed;
                    break;
                case CrystalDoorType.Ghost:
                    m_dooricon.sprite = m_Ghost_Closed;
                    break;
                case CrystalDoorType.Snake:
                    m_dooricon.sprite = m_Snake_Closed;
                    break;
                case CrystalDoorType.Cross:
                    m_dooricon.sprite = m_Cross_Closed;
                    break;
                case CrystalDoorType.Moustache:
                    m_dooricon.sprite = m_Moustache_Closed;
                    break;
            }
        }
    }
}