using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldBarCanvas : MonoBehaviour
{
    [SerializeField] Slider holdSlider;
    [SerializeField] GameObject holdBar;
    [SerializeField] PlayerPrediction playerPrediction;

    private void Awake()
    {
        UpdateStatBars();
    }

    void UpdateStatBars()
    {
        var holdPercentage = playerPrediction.GetHoldPercentage();
        if (holdPercentage <= 0)
        {
            holdBar.SetActive(false);
        } else
        {
            holdBar.SetActive(true);
            holdSlider.value = holdPercentage;
        }
    }

    private void Update()
    {
        UpdateStatBars();
    }
}
