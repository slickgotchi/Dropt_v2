using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDCanvas : MonoBehaviour
{
    private static PlayerHUDCanvas _singleton;

    public static PlayerHUDCanvas Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<PlayerHUDCanvas>();
                if (_singleton == null)
                {
                    Debug.LogError("There needs to be one active PlayerHUDCanvas script on a GameObject in your scene.");
                }
            }
            return _singleton;
        }
    }

    void Awake()
    {
        if (_singleton == null)
        {
            _singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_singleton != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private GameObject m_container;
    [SerializeField] private Slider m_healthSlider;
    [SerializeField] private TextMeshProUGUI m_healthText;
    [SerializeField] private Slider m_abilitySlider;
    [SerializeField] private TextMeshProUGUI m_abilityText;

    private NetworkCharacter m_localPlayerCharacter;

    public void SetLocalPlayerCharacter(NetworkCharacter localPlayerCharacter)
    {
        m_localPlayerCharacter = localPlayerCharacter;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_localPlayerCharacter == null)
        {
            m_container.SetActive(false);
            return;
        }

        m_container.SetActive(true);

        // HP
        var maxHp = m_localPlayerCharacter.HpMax.Value + m_localPlayerCharacter.HpBuffer.Value;
        var currHp = m_localPlayerCharacter.HpCurrent.Value;

        m_healthSlider.maxValue = maxHp;
        m_healthSlider.value = currHp;

        m_healthText.text = currHp + " / " + maxHp;

        // AP
        var maxAp = m_localPlayerCharacter.ApMax.Value + m_localPlayerCharacter.ApBuffer.Value;
        var currAp = m_localPlayerCharacter.ApCurrent.Value;

        m_abilitySlider.maxValue = maxAp;
        m_abilitySlider.value = currAp;

        m_abilityText.text = currAp + " / " + maxAp;
    }
}
