using UnityEngine;

public class ShieldBlockEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_effect;

    private float m_bubbleMaxRadius = 2.5f;
    private float m_bubbleMinRadius = 0.25f;
    private float m_bubbleMaxAlpha = 0.75f;
    private float m_bubbleMinAlpha = 0.25f;

    public void SetVisible(bool visible)
    {
        m_effect.gameObject.SetActive(visible);
    }

    public void UpdateEffect(float remainingShield)
    {
        float currentRadius = ((m_bubbleMaxRadius - m_bubbleMinRadius) * remainingShield) + m_bubbleMinRadius;
        float currentAlpha = ((m_bubbleMaxAlpha - m_bubbleMinAlpha) * remainingShield) + m_bubbleMinAlpha;
        var mainModule = m_effect.main;
        mainModule.startSize = currentRadius;
        Color color = mainModule.startColor.color;
        color.a = currentAlpha;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);
    }
}