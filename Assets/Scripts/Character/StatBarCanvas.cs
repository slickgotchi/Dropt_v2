using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class StatBarCanvas : MonoBehaviour
{
    public Slider hpSlider;
    public Slider apSlider;
    public Slider shieldSlider;

    private NetworkCharacter m_character;

    public Image hpBg;
    public Image hpFill;
    public Image apBg;
    public Image apFill;
    public Image shieldBg;
    public Image shieldFill;

    private float m_fadeTimer = 0;
    private float m_fadeDuration = 2f;
    private float m_fadeStartPoint = 2f * 0.3f;

    private bool m_isDamaged = false;

    private void Awake()
    {
        m_character = transform.parent.GetComponent<NetworkCharacter>();
    }

    private void Update()
    {
        UpdateStatBarsShowIfBelow100();
    }

    private void UpdateStatBarsShowIfBelow100()
    {
        // update sliders
        hpSlider.maxValue = m_character.currentStaticStats.HpMax;
        hpSlider.value = m_character.currentDynamicStats.HpCurrent;
        apSlider.maxValue = m_character.currentStaticStats.ApMax;
        apSlider.value = m_character.currentDynamicStats.ApCurrent;
        shieldSlider.maxValue = m_character.currentStaticStats.MaxEnemyShield;
        shieldSlider.value = m_character.currentDynamicStats.EnemyShield;

        bool isShieldActive = m_character.currentDynamicStats.EnemyShield > 0;

        shieldSlider.gameObject.SetActive(isShieldActive);
        hpSlider.gameObject.SetActive(!isShieldActive);

        // hide ap bar if char has no AP stat
        if (m_character.currentStaticStats.ApMax <= 0)
        {
            apSlider.gameObject.SetActive(false);
        }

        // show damage
        if (m_isDamaged)
        {
            SetAlpha(1);
        }
        else
        {
            SetAlpha(0);
            if (m_character.currentDynamicStats.HpCurrent - m_character.currentStaticStats.HpMax < 0)
            {
                m_isDamaged = true;
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        alpha = math.min(alpha, 1f);
        alpha = math.max(0, alpha);

        SetImageAlpha(hpBg, 0.7f * alpha);
        SetImageAlpha(hpFill, alpha);
        SetImageAlpha(apBg, 0.7f * alpha);
        SetImageAlpha(apFill, alpha);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}