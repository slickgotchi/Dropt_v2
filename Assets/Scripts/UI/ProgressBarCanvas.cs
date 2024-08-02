using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarCanvas : MonoBehaviour
{
    public static ProgressBarCanvas Instance { get; private set; }

    public GameObject Container;
    public TextMeshProUGUI Text;
    public Slider Slider;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        //if (Game.Instance == null) return;

        //Text.text = "Connection Status: " + Game.Instance.statusString;
    }

    public void Show(string statusText, float sliderProgress)
    {
        Container.SetActive(true);
        Text.text = statusText;
        Slider.value = sliderProgress;
    }

    public void Hide()
    {
        Container.SetActive(false);
    }
}
