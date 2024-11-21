using UnityEngine;
using Unity.Netcode;

public class ApeDoorButton : DoorButton<ApeDoorType>
{
    [Header("Sprites")]
    public Sprite CrescentUp;
    public Sprite CrescentDown;

    public Sprite TriangleUp;
    public Sprite TriangleDown;

    public Sprite HexagonUp;
    public Sprite HexagonDown;

    public Sprite PlusUp;
    public Sprite PlusDown;

    public Sprite SquareUp;
    public Sprite SquareDown;

    public override void Awake()
    {
        base.Awake();
        Type = new NetworkVariable<ApeDoorType>(ApeDoorType.Crescent);
    }

    public override Door<ApeDoorType>[] GetAllOtherDoor()
    {
        return FindObjectsByType<ApeDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public override DoorButton<ApeDoorType>[] GetAllOtherDoorButtons()
    {
        return FindObjectsByType<ApeDoorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public override void UpdateSprite()
    {
        if (State.Value == ButtonState.Up)
        {
            switch (Type.Value)
            {
                case ApeDoorType.Crescent: m_spriteRenderer.sprite = CrescentUp; break;
                case ApeDoorType.Triangle: m_spriteRenderer.sprite = TriangleUp; break;
                case ApeDoorType.Hexagon: m_spriteRenderer.sprite = HexagonUp; break;
                case ApeDoorType.Plus: m_spriteRenderer.sprite = PlusUp; break;
                case ApeDoorType.Square: m_spriteRenderer.sprite = SquareUp; break;
                default: break;
            }
        }
        else
        {
            switch (Type.Value)
            {
                case ApeDoorType.Crescent: m_spriteRenderer.sprite = CrescentDown; break;
                case ApeDoorType.Triangle: m_spriteRenderer.sprite = TriangleDown; break;
                case ApeDoorType.Hexagon: m_spriteRenderer.sprite = HexagonDown; break;
                case ApeDoorType.Plus: m_spriteRenderer.sprite = PlusDown; break;
                case ApeDoorType.Square: m_spriteRenderer.sprite = SquareDown; break;
                default: break;
            }
        }
    }
}