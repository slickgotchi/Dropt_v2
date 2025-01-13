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

    public bool isVisible = false;

    private void Start()
    {
        if (Bootstrap.IsServer())
        {
            GeneralDebugCanvas.Instance.enemyAIToggle.isOn = false;
            isVisible = false;
            Container.SetActive(false);
        }
    }

    private void Update()
    {
        if (GeneralDebugCanvas.Instance == null) return;

        if (GeneralDebugCanvas.Instance.enemyAIToggle.isOn && !isVisible)
        {
            isVisible = true;
            Container.SetActive(true);
        }
        else if (!GeneralDebugCanvas.Instance.enemyAIToggle.isOn && isVisible)
        {
            isVisible = false;
            Container.SetActive(false);
        }

    }
}
