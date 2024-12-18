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
        HpMax.text = "HpMax: " + m_networkCharacter.currentStaticStats.HpMax.ToString("F0");
        HpCurrent.text = "HpCurrent: " + m_networkCharacter.currentDynamicStats.HpCurrent.ToString("F0");
        HpBuffer.text = "HpBuffer: " + m_networkCharacter.currentStaticStats.HpBuffer.ToString("F0");
        AttackPower.text = "AttackPower: " + m_networkCharacter.currentStaticStats.AttackPower.ToString("F0");
        CriticalChance.text = "CriticalChance: " + m_networkCharacter.currentStaticStats.CriticalChance.ToString("F2");
        ApMax.text = "ApMax: " + m_networkCharacter.currentStaticStats.ApMax.ToString("F0");
        ApCurrent.text = "ApCurrent: " + m_networkCharacter.currentDynamicStats.ApCurrent.ToString("F0");
        ApBuffer.text = "ApBuffer: " + m_networkCharacter.currentStaticStats.ApBuffer.ToString("F0");
        DoubleStrikeChance.text = "DoubleStrikeChance: " + m_networkCharacter.currentStaticStats.DoubleStrikeChance.ToString("F2");
        CriticalDamage.text = "CriticalDamage: " + m_networkCharacter.currentStaticStats.CriticalDamage.ToString("F2");
        MoveSpeed.text = "MoveSpeed: " + m_networkCharacter.currentStaticStats.MoveSpeed.ToString("F2");
        Accuracy.text = "Accuracy: " + m_networkCharacter.currentStaticStats.Accuracy.ToString("F2");
        Evasion.text = "Evasion: " + m_networkCharacter.currentStaticStats.Evasion.ToString("F2");
        DamageReduction.text = "DamageReduction: " + m_networkCharacter.currentStaticStats.DamageReduction.ToString("F2");
        ApLeech.text = "ApLeech: " + m_networkCharacter.currentStaticStats.ApLeech.ToString("F2");
        ApRegen.text = "ApRegen: " + m_networkCharacter.currentStaticStats.ApRegen.ToString("F2");
    }
}
