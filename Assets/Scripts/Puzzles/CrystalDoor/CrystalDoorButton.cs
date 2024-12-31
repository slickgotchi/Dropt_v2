using Unity.Netcode;
using UnityEngine;

public class CrystalDoorButton : DoorButton<CrystalDoorType>
{
    [SerializeField] private Sprite m_R_Up;
    [SerializeField] private Sprite m_R_Down;

    [SerializeField] private Sprite m_Ghost_Up;
    [SerializeField] private Sprite m_Ghost_Down;

    [SerializeField] private Sprite m_Snake_Up;
    [SerializeField] private Sprite m_Snake_Down;

    [SerializeField] private Sprite m_Cross_Up;
    [SerializeField] private Sprite m_Cross_Down;

    [SerializeField] private Sprite m_Moustache_Up;
    [SerializeField] private Sprite m_Moustache_Down;

    public override void Awake()
    {
        base.Awake();
        DoorType = new NetworkVariable<CrystalDoorType>(CrystalDoorType.R);
    }

    public override Door<CrystalDoorType>[] GetAllOtherDoor()
    {
        return DoorManager<CrystalDoorType>.Instance.GetDoors().ToArray();
    }

    public override DoorButton<CrystalDoorType>[] GetAllOtherDoorButtons()
    {
        return DoorManager<CrystalDoorType>.Instance.GetButtons().ToArray();
    }

    public override void UpdateSprite()
    {
        if (State.Value == ButtonState.Up)
        {
            switch (DoorType.Value)
            {
                case CrystalDoorType.R: m_spriteRenderer.sprite = m_R_Up; break;
                case CrystalDoorType.Ghost: m_spriteRenderer.sprite = m_Ghost_Up; break;
                case CrystalDoorType.Snake: m_spriteRenderer.sprite = m_Snake_Up; break;
                case CrystalDoorType.Cross: m_spriteRenderer.sprite = m_Cross_Up; break;
                case CrystalDoorType.Moustache: m_spriteRenderer.sprite = m_Moustache_Up; break;
                default: break;
            }
        }
        else
        {
            switch (DoorType.Value)
            {
                case CrystalDoorType.R: m_spriteRenderer.sprite = m_R_Down; break;
                case CrystalDoorType.Ghost: m_spriteRenderer.sprite = m_Ghost_Down; break;
                case CrystalDoorType.Snake: m_spriteRenderer.sprite = m_Snake_Down; break;
                case CrystalDoorType.Cross: m_spriteRenderer.sprite = m_Cross_Down; break;
                case CrystalDoorType.Moustache: m_spriteRenderer.sprite = m_Moustache_Down; break;
                default: break;
            }
        }
    }
}