using Unity.Netcode;
using UnityEngine;

public class NetworkCharacter : NetworkBehaviour
{
    [Header("Initial Base Stats")]
    public int initialHpMax = 100;
    public int initialHpCurrent = 100;
    public int initialHpBuffer = 0;
    public int initialAttackPower = 10;
    public float initialCriticalChance = 0.1f;
    public int initialApMax = 50;
    public int initialApCurrent = 50;
    public int initialApBuffer = 0;
    public float initialDoubleStrikeChance = 0.05f;
    public float initialCriticalStrikeDamage = 1.5f;
    public float initialMoveSpeed = 6.22f;
    public float initialAccuracy = 1f;
    public float initialEvasion = 0f;

    // NetworkVariables
    [HideInInspector] public NetworkVariable<int> HpMax = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> HpCurrent = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> HpBuffer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> AttackPower = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<float> CriticalChance = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<int> ApMax = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> ApCurrent = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> ApBuffer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<float> DoubleStrikeChance = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> CriticalDamage = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> Accuracy = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> Evasion = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize default values on the server
            InitializeStats();
        }
    }

    protected virtual void InitializeStats()
    {
        HpMax.Value = initialHpMax;
        HpCurrent.Value = initialHpCurrent;
        HpBuffer.Value = initialHpBuffer;
        AttackPower.Value = initialAttackPower;
        CriticalChance.Value = initialCriticalChance;
        ApMax.Value = initialApMax;
        ApCurrent.Value = initialApCurrent;
        ApBuffer.Value = initialApBuffer;
        DoubleStrikeChance.Value = initialDoubleStrikeChance;
        CriticalDamage.Value = initialCriticalStrikeDamage;
        MoveSpeed.Value = initialMoveSpeed;
        Accuracy.Value = initialAccuracy;
        Evasion.Value = initialEvasion;
    }


}
