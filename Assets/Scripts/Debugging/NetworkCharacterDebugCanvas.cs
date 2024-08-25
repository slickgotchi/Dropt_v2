using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCharacterDebugCanvas : MonoBehaviour
{
    [Header("Network Character")]
    public NetworkCharacter networkCharacter;
    public GameObject Container;

    [Header("Stat TextMesh")]
    public TextMeshProUGUI HpMax;
    public TextMeshProUGUI HpCurrent;
    public TextMeshProUGUI HpBuffer;
    public TextMeshProUGUI AttackPower;
    public TextMeshProUGUI CriticalChance;
    public TextMeshProUGUI ApMax;
    public TextMeshProUGUI ApCurrent;
    public TextMeshProUGUI ApBuffer;
    public TextMeshProUGUI DoubleStrikeChance;
    public TextMeshProUGUI CriticalDamage;
    public TextMeshProUGUI MoveSpeed;
    public TextMeshProUGUI Accuracy;
    public TextMeshProUGUI Evasion;
    public TextMeshProUGUI DamageReduction;
    public TextMeshProUGUI ApLeech;

    private void Awake()
    {
        Container.SetActive(false);
    }

    void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        HpMax.text = "HpMax: " + networkCharacter.HpMax.Value.ToString("F0");
        HpCurrent.text = "HpCurrent: " + networkCharacter.HpCurrent.Value.ToString("F0");
        HpBuffer.text = "HpBuffer: " + networkCharacter.HpBuffer.Value.ToString("F0");
        AttackPower.text = "AttackPower: " + networkCharacter.AttackPower.Value.ToString("F0");
        CriticalChance.text = "CriticalChance: " + networkCharacter.CriticalChance.Value.ToString("F2");
        ApMax.text = "ApMax: " + networkCharacter.ApMax.Value.ToString("F0");
        ApCurrent.text = "ApCurrent: " + networkCharacter.ApCurrent.Value.ToString("F0");
        ApBuffer.text = "ApBuffer: " + networkCharacter.ApBuffer.Value.ToString("F0");
        DoubleStrikeChance.text = "DoubleStrikeChance: " + networkCharacter.DoubleStrikeChance.Value.ToString("F2");
        CriticalDamage.text = "CriticalDamage: " + networkCharacter.CriticalDamage.Value.ToString("F2");
        MoveSpeed.text = "MoveSpeed: " + networkCharacter.MoveSpeed.Value.ToString("F2");
        Accuracy.text = "Accuracy: " + networkCharacter.Accuracy.Value.ToString("F2");
        Evasion.text = "Evasion: " + networkCharacter.Evasion.Value.ToString("F2");
        DamageReduction.text = "DamageReduction: " + networkCharacter.DamageReduction.Value.ToString("F2");
        ApLeech.text = "ApLeech: " + networkCharacter.ApLeech.Value.ToString("F2");
    }
}
