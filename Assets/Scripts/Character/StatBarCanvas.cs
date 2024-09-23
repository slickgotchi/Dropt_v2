using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class StatBarCanvas : MonoBehaviour
{
    public Slider hpSlider;
    public Slider apSlider;

    public NetworkCharacter character;

    public Image hpBg;
    public Image hpFill;
    public Image apBg;
    public Image apFill;

    private float m_fadeTimer = 0;
    private float m_fadeDuration = 2f;
    private float m_fadeStartPoint = 2f * 0.3f;

    private void Awake()
    {
        UpdateStatBars();
    }

    void UpdateStatBars()
    {
        // check hp differences
        if (math.abs(hpSlider.value - character.HpCurrent.Value) > 1)
        {
            ShowFade(2f);
        }

        hpSlider.maxValue = character.HpMax.Value;
        hpSlider.value = character.HpCurrent.Value;
        apSlider.maxValue = character.ApMax.Value;
        apSlider.value = character.ApCurrent.Value;

        if (character.ApMax.Value <= 0)
        {
            apSlider.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateStatBars();
        UpdateAlpha();
    }

    void UpdateAlpha()
    {
        m_fadeTimer -= Time.deltaTime;
        if (m_fadeTimer > m_fadeStartPoint)
        {
            SetAlpha(1);
        }
        else
        {
            SetAlpha(m_fadeTimer / m_fadeStartPoint);
        }
    }

    void ShowFade(float duration)
    {
        m_fadeDuration = duration;
        m_fadeTimer = duration;
        m_fadeStartPoint = duration * 0.3f;
    }

    void SetAlpha(float alpha)
    {
        alpha = math.min(alpha, 1f);
        alpha = math.max(0, alpha);

        SetImageAlpha(hpBg, 0.7f * alpha);
        SetImageAlpha(hpFill, alpha);
        SetImageAlpha(apBg, 0.7f * alpha);
        SetImageAlpha(apFill, alpha);
    }

    void SetImageAlpha(Image image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
