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

    // Cached values
    private float cachedHpMax = -1;
    private float cachedHpCurrent = -1;
    private float cachedApMax = -1;
    private float cachedApCurrent = -1;
    private float cachedShieldMax = -1;
    private float cachedShieldCurrent = -1;

    private bool cachedShieldActive = false;

    private void Awake()
    {
        m_character = transform.parent.GetComponent<NetworkCharacter>();
    }

    public void Init()
    {
        m_isDamaged = false;
    }

    private void Update()
    {
        UpdateStatBarsShowIfBelow100();
    }

    private void UpdateStatBarsShowIfBelow100()
    {
        bool valuesChanged = false;

        // Update HP slider only if the values change
        if (cachedHpMax != m_character.currentStaticStats.HpMax || cachedHpCurrent != m_character.currentDynamicStats.HpCurrent)
        {
            cachedHpMax = m_character.currentStaticStats.HpMax;
            cachedHpCurrent = m_character.currentDynamicStats.HpCurrent;
            hpSlider.maxValue = cachedHpMax;
            hpSlider.value = cachedHpCurrent;
            valuesChanged = true;
        }

        // Update AP slider only if the values change
        if (cachedApMax != m_character.currentStaticStats.ApMax || cachedApCurrent != m_character.currentDynamicStats.ApCurrent)
        {
            cachedApMax = m_character.currentStaticStats.ApMax;
            cachedApCurrent = m_character.currentDynamicStats.ApCurrent;
            apSlider.maxValue = cachedApMax;
            apSlider.value = cachedApCurrent;

            // Hide AP bar if character has no AP stat
            apSlider.gameObject.SetActive(cachedApMax > 0);
            valuesChanged = true;
        }

        // Update Shield slider only if the values change
        if (cachedShieldMax != m_character.currentStaticStats.MaxEnemyShield || cachedShieldCurrent != m_character.currentDynamicStats.EnemyShield)
        {
            cachedShieldMax = m_character.currentStaticStats.MaxEnemyShield;
            cachedShieldCurrent = m_character.currentDynamicStats.EnemyShield;
            shieldSlider.maxValue = cachedShieldMax;
            shieldSlider.value = cachedShieldCurrent;

            bool isShieldActive = cachedShieldCurrent > 0;
            if (isShieldActive != cachedShieldActive)
            {
                cachedShieldActive = isShieldActive;
                shieldSlider.gameObject.SetActive(isShieldActive);
                hpSlider.gameObject.SetActive(!isShieldActive);
                valuesChanged = true;
            }
        }

        // Update damage alpha only if it changes
        if (m_isDamaged || m_character.currentDynamicStats.HpCurrent < m_character.currentStaticStats.HpMax)
        {
            SetAlpha(1);
            m_isDamaged = true;
        }
        else
        {
            SetAlpha(0);
        }

        // Redraw the layout only if necessary
        if (valuesChanged)
        {
            RefreshLayout();
        }
    }

    private void SetAlpha(float alpha)
    {
        alpha = math.clamp(alpha, 0f, 1f);

        SetImageAlpha(hpBg, 0.7f * alpha);
        SetImageAlpha(hpFill, alpha);
        SetImageAlpha(apBg, 0.7f * alpha);
        SetImageAlpha(apFill, alpha);
        SetImageAlpha(shieldBg, 0.7f * alpha);
        SetImageAlpha(shieldFill, alpha);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void RefreshLayout()
    {
        // Add logic to force layout updates if necessary
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
