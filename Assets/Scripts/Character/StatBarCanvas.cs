using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBarCanvas : MonoBehaviour
{
    public Slider hpSlider;
    public Slider apSlider;

    public NetworkCharacter character;

    private void Awake()
    {
        UpdateStatBars();
    }

    void UpdateStatBars()
    {
        hpSlider.maxValue = character.HpMax.Value;
        hpSlider.value = character.HpMax.Value;
        apSlider.maxValue = character.ApMax.Value;
        apSlider.value = character.ApMax.Value;

        if (character.ApMax.Value <= 0)
        {
            apSlider.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateStatBars();
    }
}
