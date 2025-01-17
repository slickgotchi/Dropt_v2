using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private Color m_flashColor = Color.white;
    private float m_flashTime = 0.5f;
    //private int m_flashCount = 3; // Number of times the sprite should flash
    //private float m_flashInterval = 0.5f / 3;

    [SerializeField] private List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();

    private Material[] m_materials;
    private bool m_isPlaying = false;
    private float m_timer = 0;
    private int m_currentFlashCount = 0;

    public enum FlashStyle { TripleStandard, Rapid5, HeavyDamage, Single }
    public struct FlashSetting
    {
        public float whiteTime_s;
        public float normalTime_s;
        public int flashCount;
    }
    private FlashSetting m_flashSetting;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        m_materials = new Material[m_spriteRenderers.Count];

        for (int i = 0; i < m_spriteRenderers.Count; i++)
        {
            m_materials[i] = m_spriteRenderers[i].material;
        }

        SetFlashColor();
        SetFlashAmount(0);
    }

    private bool isOn = false;

    public void DamageFlash(FlashStyle flashStyle = FlashStyle.TripleStandard)
    {
        SetupFlashSetting(flashStyle);

        m_isPlaying = true;
        m_timer = m_flashSetting.whiteTime_s;
        SetFlashColor();

        m_currentFlashCount = 0; // Reset the flash count
        SetFlashAmount(1);
        isOn = true;
    }

    private void Update()
    {
        if (!m_isPlaying) return;

        UpdateFlickerFlash();
    }

    void UpdateFlickerFlash()
    {

        if (m_isPlaying)
        {
            m_timer -= Time.deltaTime;

            if (m_timer <= 0)
            {
                if (isOn)
                {
                    SetFlashAmount(0);
                    isOn = false;
                    m_currentFlashCount++;
                    m_timer+= m_flashSetting.normalTime_s;
                }
                else
                {
                    SetFlashAmount(1);
                    isOn = true;
                    m_timer += m_flashSetting.whiteTime_s;
                }


                if (m_currentFlashCount >= m_flashSetting.flashCount)
                {
                    m_isPlaying = false;
                }
            }
        }

    }

    //void UpdateFadeInOutFlash()
    //{
    //    m_timer -= Time.deltaTime;

    //    float alpha = m_timer > 0 ? m_timer / m_flashTime : 0;

    //    SetFlashAmount(alpha);

    //    if (m_timer <= 0)
    //    {
    //        m_currentFlashCount++;

    //        if (m_currentFlashCount < m_flashCount)
    //        {
    //            // Start the next flash
    //            m_timer = m_flashTime;
    //        }
    //        else
    //        {
    //            // End the flashing
    //            m_isPlaying = false;
    //        }
    //    }
    //}

    void SetupFlashSetting(FlashStyle flashStyle)
    {
        if (flashStyle == FlashStyle.TripleStandard)
        {
            m_flashSetting.whiteTime_s = 0.1f;
            m_flashSetting.normalTime_s = 0.05f;
            m_flashSetting.flashCount = 3;
        }
        else if (flashStyle == FlashStyle.Rapid5)
        {
            m_flashSetting.whiteTime_s = 0.05f;
            m_flashSetting.normalTime_s = 0.05f;
            m_flashSetting.flashCount = 5;
        }
        else if (flashStyle == FlashStyle.HeavyDamage)
        {
            m_flashSetting.whiteTime_s = 0.15f;
            m_flashSetting.normalTime_s = 0.1f;
            m_flashSetting.flashCount = 2;
        }
        else if (flashStyle == FlashStyle.Single)
        {
            m_flashSetting.whiteTime_s = 0.2f;
            m_flashSetting.normalTime_s = 0.0f;
            m_flashSetting.flashCount = 1;
        }
    }

    private void SetFlashColor()
    {
        for (int i = 0; i < m_materials.Length; i++)
        {
            m_materials[i].SetColor("_FlashColor", m_flashColor);
        }
    }

    private void SetFlashAmount(float amount)
    {
        for (int i = 0; i < m_materials.Length; i++)
        {
            m_materials[i].SetFloat("_FlashAmount", amount);
        }
    }
}
