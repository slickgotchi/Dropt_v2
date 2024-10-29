using UnityEngine;

public class PetView : MonoBehaviour
{
    public SpriteRenderer m_body;
    public SpriteRenderer m_shadow;

    public Sprite m_leftSprite;
    public Sprite m_rightSprite;
    public Sprite m_upSprite;
    public Sprite m_downSprite;

    public void Show()
    {
        m_body.enabled = true;
        m_shadow.enabled = true;
    }

    public void Hide()
    {
        m_body.enabled = false;
        m_shadow.enabled = false;
    }

    public void SetSprite(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                m_body.sprite = m_leftSprite;
                break;
            case Direction.Right:
                m_body.sprite = m_rightSprite;
                break;
            case Direction.Up:
                m_body.sprite = m_upSprite;
                break;
            case Direction.Down:
                m_body.sprite = m_downSprite;
                break;
        }
    }

    public bool IsActivated()
    {
        return m_body.enabled;
    }
}