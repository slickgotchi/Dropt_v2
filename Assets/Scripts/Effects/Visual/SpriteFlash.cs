using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private Color m_flashColor = Color.white;
    [SerializeField] private float m_flashTime = 0.15f;
    [SerializeField] private int m_flashCount = 4; // Number of times the sprite should flash

    [SerializeField] private List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();

    private Material[] m_materials;
    private bool m_isPlaying = false;
    private float m_timer = 0;
    private int m_currentFlashCount = 0;

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

    public void DamageFlash()
    {
        m_isPlaying = true;
        m_timer = m_flashTime;
        m_currentFlashCount = 0; // Reset the flash count
        SetFlashColor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            DamageFlash();
            Debug.Log("DamageFlash");
        }

        if (!m_isPlaying) return;

        m_timer -= Time.deltaTime;

        float alpha = m_timer > 0 ? m_timer / m_flashTime : 0;

        SetFlashAmount(alpha);

        if (m_timer <= 0)
        {
            m_currentFlashCount++;

            if (m_currentFlashCount < m_flashCount)
            {
                // Start the next flash
                m_timer = m_flashTime;
            }
            else
            {
                // End the flashing
                m_isPlaying = false;
            }
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
