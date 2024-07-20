using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GltrOrb : MonoBehaviour
{
    public Sprite tinySprite;
    public Sprite smallSprite;
    public Sprite mediumSprite;
    public Sprite largeSprite;

    public SpriteRenderer m_spriteRenderer;
    
    private int m_value = 1;

    public void Init(PickupItemManager.Size size)
    {
        switch (size)
        {
            case PickupItemManager.Size.Tiny: 
                m_spriteRenderer.sprite = tinySprite;
                m_value = 1;
                break;
            case PickupItemManager.Size.Small:
                m_spriteRenderer.sprite = smallSprite;
                m_value = 5;
                break;
            case PickupItemManager.Size.Medium:
                m_spriteRenderer.sprite = mediumSprite;
                m_value = 25;
                break;
            case PickupItemManager.Size.Large:
                m_spriteRenderer.sprite = largeSprite;
                m_value = 100;
                break;
            default: break;
        }
    }

    public int GetValue() {  return m_value; }
}
