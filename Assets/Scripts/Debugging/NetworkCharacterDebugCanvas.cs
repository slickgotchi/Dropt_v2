using TMPro;
using UnityEngine;

public class NetworkCharacterDebugCanvas : MonoBehaviour
{
    //[Header("Network Character")]
    private NetworkCharacter m_networkCharacter;
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
    public TextMeshProUGUI ApRegen;

    private float prevHpMax;
    private float prevHpCurrent;
    private float prevHpBuffer;
    private float prevAttackPower;
    private float prevCriticalChance;
    private float prevApMax;
    private float prevApCurrent;
    private float prevApBuffer;
    private float prevDoubleStrikeChance;
    private float prevCriticalDamage;
    private float prevMoveSpeed;
    private float prevAccuracy;
    private float prevEvasion;
    private float prevDamageReduction;
    private float prevApLeech;
    private float prevApRegen;

    private void Awake()
    {
        m_networkCharacter = transform.parent.GetComponent<NetworkCharacter>();
        Container.SetActive(false);
    }

    private void Start()
    {
        Container.SetActive(false);
    }

    void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (prevHpMax != m_networkCharacter.currentStaticStats.HpMax)
        {
            prevHpMax = m_networkCharacter.currentStaticStats.HpMax;
            HpMax.text = "HpMax: " + prevHpMax.ToString("F0");
        }

        if (prevHpCurrent != m_networkCharacter.currentDynamicStats.HpCurrent)
        {
            prevHpCurrent = m_networkCharacter.currentDynamicStats.HpCurrent;
            HpCurrent.text = "HpCurrent: " + prevHpCurrent.ToString("F0");
        }

        if (prevHpBuffer != m_networkCharacter.currentStaticStats.HpBuffer)
        {
            prevHpBuffer = m_networkCharacter.currentStaticStats.HpBuffer;
            HpBuffer.text = "HpBuffer: " + prevHpBuffer.ToString("F0");
        }

        if (prevAttackPower != m_networkCharacter.currentStaticStats.AttackPower)
        {
            prevAttackPower = m_networkCharacter.currentStaticStats.AttackPower;
            AttackPower.text = "AttackPower: " + prevAttackPower.ToString("F0");
        }

        if (prevCriticalChance != m_networkCharacter.currentStaticStats.CriticalChance)
        {
            prevCriticalChance = m_networkCharacter.currentStaticStats.CriticalChance;
            CriticalChance.text = "CriticalChance: " + prevCriticalChance.ToString("F2");
        }

        if (prevApMax != m_networkCharacter.currentStaticStats.ApMax)
        {
            prevApMax = m_networkCharacter.currentStaticStats.ApMax;
            ApMax.text = "ApMax: " + prevApMax.ToString("F0");
        }

        if (prevApCurrent != m_networkCharacter.currentDynamicStats.ApCurrent)
        {
            prevApCurrent = m_networkCharacter.currentDynamicStats.ApCurrent;
            ApCurrent.text = "ApCurrent: " + prevApCurrent.ToString("F0");
        }

        if (prevApBuffer != m_networkCharacter.currentStaticStats.ApBuffer)
        {
            prevApBuffer = m_networkCharacter.currentStaticStats.ApBuffer;
            ApBuffer.text = "ApBuffer: " + prevApBuffer.ToString("F0");
        }

        if (prevDoubleStrikeChance != m_networkCharacter.currentStaticStats.DoubleStrikeChance)
        {
            prevDoubleStrikeChance = m_networkCharacter.currentStaticStats.DoubleStrikeChance;
            DoubleStrikeChance.text = "DoubleStrikeChance: " + prevDoubleStrikeChance.ToString("F2");
        }

        if (prevCriticalDamage != m_networkCharacter.currentStaticStats.CriticalDamage)
        {
            prevCriticalDamage = m_networkCharacter.currentStaticStats.CriticalDamage;
            CriticalDamage.text = "CriticalDamage: " + prevCriticalDamage.ToString("F2");
        }

        if (prevMoveSpeed != m_networkCharacter.currentStaticStats.MoveSpeed)
        {
            prevMoveSpeed = m_networkCharacter.currentStaticStats.MoveSpeed;
            MoveSpeed.text = "MoveSpeed: " + prevMoveSpeed.ToString("F2");
        }

        if (prevAccuracy != m_networkCharacter.currentStaticStats.Accuracy)
        {
            prevAccuracy = m_networkCharacter.currentStaticStats.Accuracy;
            Accuracy.text = "Accuracy: " + prevAccuracy.ToString("F2");
        }

        if (prevEvasion != m_networkCharacter.currentStaticStats.Evasion)
        {
            prevEvasion = m_networkCharacter.currentStaticStats.Evasion;
            Evasion.text = "Evasion: " + prevEvasion.ToString("F2");
        }

        if (prevDamageReduction != m_networkCharacter.currentStaticStats.DamageReduction)
        {
            prevDamageReduction = m_networkCharacter.currentStaticStats.DamageReduction;
            DamageReduction.text = "DamageReduction: " + prevDamageReduction.ToString("F2");
        }

        if (prevApLeech != m_networkCharacter.currentStaticStats.ApLeech)
        {
            prevApLeech = m_networkCharacter.currentStaticStats.ApLeech;
            ApLeech.text = "ApLeech: " + prevApLeech.ToString("F2");
        }

        if (prevApRegen != m_networkCharacter.currentStaticStats.ApRegen)
        {
            prevApRegen = m_networkCharacter.currentStaticStats.ApRegen;
            ApRegen.text = "ApRegen: " + prevApRegen.ToString("F2");
        }
    }
}
