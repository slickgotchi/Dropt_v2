using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyAI_DebugCanvas : MonoBehaviour
{
    public GameObject Container;
    public TextMeshProUGUI stateTMP;
    public Slider slider;

    private bool m_isVisible = false;

    private void Update()
    {
        if (GeneralDebugCanvas.Instance == null) return;

        if (GeneralDebugCanvas.Instance.enemyAIToggle.isOn && !m_isVisible)
        {
            m_isVisible = true;
            Container.SetActive(true);
        }
        else if (!GeneralDebugCanvas.Instance.enemyAIToggle.isOn && m_isVisible)
        {
            m_isVisible = false;
            Container.SetActive(false);
        }

    }
}
