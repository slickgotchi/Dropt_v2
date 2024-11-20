using UnityEngine;
using Unity.Netcode;

public class ApeDoor : Door<ApeDoorType>
{
    [Header("Emblems")]
    public SpriteRenderer LeftEmblem;
    public SpriteRenderer RightEmblem;

    [Header("Sprites")]
    public Sprite CrescentLeft;
    public Sprite CrescentRight;

    public Sprite TriangleLeft;
    public Sprite TriangleRight;

    public Sprite HexagonLeft;
    public Sprite HexagonRight;

    public Sprite PlusLeft;
    public Sprite PlusRight;

    public Sprite SquareLeft;
    public Sprite SquareRight;

    private Animator m_animator;

    public override void Awake()
    {
        m_animator = GetComponent<Animator>();
        Type = new NetworkVariable<ApeDoorType>(ApeDoorType.Crescent);
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
        UpdateSprite();
    }

    public override void OpenDoorAnimation()
    {
        m_animator.Play("ApeDoor_Open");
    }

    public void UpdateSprite()
    {
        if (!IsClient) return;

        switch (Type.Value)
        {
            case ApeDoorType.Crescent:
                LeftEmblem.sprite = CrescentLeft;
                RightEmblem.sprite = CrescentRight;
                break;
            case ApeDoorType.Triangle:
                LeftEmblem.sprite = TriangleLeft;
                RightEmblem.sprite = TriangleRight;
                break;
            case ApeDoorType.Hexagon:
                LeftEmblem.sprite = HexagonLeft;
                RightEmblem.sprite = HexagonRight;
                break;
            case ApeDoorType.Plus:
                LeftEmblem.sprite = PlusLeft;
                RightEmblem.sprite = PlusRight;
                break;
            case ApeDoorType.Square:
                LeftEmblem.sprite = SquareLeft;
                RightEmblem.sprite = SquareRight;
                break;
            default: break;
        }
    }
}