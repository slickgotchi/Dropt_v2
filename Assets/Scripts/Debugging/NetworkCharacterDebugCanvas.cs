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
        HpMax.text = "HpMax: " + networkCharacter.HpMax.Value;
        HpCurrent.text = "HpCurrent: " + networkCharacter.HpCurrent.Value;
        HpBuffer.text = "HpBuffer: " + networkCharacter.HpBuffer.Value;
        AttackPower.text = "AttackPower: " + networkCharacter.AttackPower.Value;
        CriticalChance.text = "CriticalChance: " + networkCharacter.CriticalChance.Value;
        ApMax.text = "ApMax: " + networkCharacter.ApMax.Value;
        ApCurrent.text = "ApCurrent: " + networkCharacter.ApCurrent.Value;
        ApBuffer.text = "ApBuffer: " + networkCharacter.ApBuffer.Value;
        DoubleStrikeChance.text = "DoubleStrikeChance: " + networkCharacter.DoubleStrikeChance.Value;
        CriticalDamage.text = "CriticalDamage: " + networkCharacter.CriticalDamage.Value;
        MoveSpeed.text = "MoveSpeed: " + networkCharacter.MoveSpeed.Value;
        Accuracy.text = "Accuracy: " + networkCharacter.Accuracy.Value;
        Evasion.text = "Evasion: " + networkCharacter.Evasion.Value;
        DamageReduction.text = "DamageReduction: " + networkCharacter.DamageReduction.Value;
        ApLeech.text = "ApLeech: " + networkCharacter.ApLeech.Value;
    }
}
