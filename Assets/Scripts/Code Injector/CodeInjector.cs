using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CodeInjector : NetworkBehaviour
{
    public enum Variable
    {
        Multiplier,
        EnemyHP,
        EnemyDamage,
        EnemySpeed,
        EliteEnemies,
        EnemyShield,
        TrapDamage,
        LimitedStock,
        UnderPressure
    }

    public static CodeInjector Instance { get; private set; }

    [SerializeField] private CodeInjectorFloat m_multiplier;
    [SerializeField] private CodeInjectorFloat m_enemyHp;
    [SerializeField] private CodeInjectorFloat m_enemyDamage;
    [SerializeField] private CodeInjectorFloat m_enemySpeed;
    [SerializeField] private CodeInjectorFloat m_eliteEnemies;
    [SerializeField] private CodeInjectorInt m_enemyShield;
    [SerializeField] private CodeInjectorFloat m_trapDamage;
    [SerializeField] private CodeInjectorInt m_limitedStock;
    [SerializeField] private CodeInjectorFloat m_underPressure;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnect;
        }
    }

    private void OnClientConnect(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            if (IsHost)
            {
                InitializeVariables();
            }
            else
            {
                RequestForVariableDataClientRpc();
            }
        }
        else
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }  // Specify the target client ID(s)
                }
            };
            SetFloatVariablesClientRpc(GetFloatVariables(), clientRpcParams);
            SetIntVariablesClientRpc(GetIntVariables(), clientRpcParams);
        }
    }

    private void InitializeVariables()
    {
        m_multiplier.Initialize();
        m_enemyHp.Initialize();
        m_enemyDamage.Initialize();
        m_enemySpeed.Initialize();
        m_eliteEnemies.Initialize();
        m_enemyShield.Initialize();
        m_trapDamage.Initialize();
        m_limitedStock.Initialize();
        m_underPressure.Initialize();
        CodeInjectorCanvas.Instance.InitializeVariableValues();
    }

    public void AddVariable(Variable type)
    {
        switch (type)
        {
            case Variable.Multiplier:
                m_multiplier.Add();
                break;
            case Variable.EnemyHP:
                m_enemyHp.Add();
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.Add();
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.Add();
                break;
            case Variable.EliteEnemies:
                m_eliteEnemies.Add();
                break;
            case Variable.EnemyShield:
                m_enemyShield.Add();
                break;
            case Variable.TrapDamage:
                m_trapDamage.Add();
                break;
            case Variable.LimitedStock:
                m_limitedStock.Add();
                break;
            case Variable.UnderPressure:
                m_underPressure.Add();
                break;
            default:
                break;
        }
    }

    public void SubtractVariable(Variable type)
    {
        switch (type)
        {
            case Variable.Multiplier:
                m_multiplier.Subtract();
                break;
            case Variable.EnemyHP:
                m_enemyHp.Subtract();
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.Subtract();
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.Subtract();
                break;
            case Variable.EliteEnemies:
                m_eliteEnemies.Subtract();
                break;
            case Variable.EnemyShield:
                m_enemyShield.Subtract();
                break;
            case Variable.TrapDamage:
                m_trapDamage.Subtract();
                break;
            case Variable.LimitedStock:
                m_limitedStock.Subtract();
                break;
            case Variable.UnderPressure:
                m_underPressure.Subtract();
                break;
        }
    }

    public string GetVariableString(Variable type)
    {
        return type switch
        {
            Variable.Multiplier => m_multiplier.ToString(),
            Variable.EnemyHP => m_enemyHp.ToString(),
            Variable.EnemyDamage => m_enemyDamage.ToString(),
            Variable.EnemySpeed => m_enemySpeed.ToString(),
            Variable.EliteEnemies => m_eliteEnemies.ToString(),
            Variable.EnemyShield => m_enemyShield.ToString(),
            Variable.TrapDamage => m_trapDamage.ToString(),
            Variable.LimitedStock => m_limitedStock.ToString(),
            Variable.UnderPressure => m_underPressure.ToString(),
            _ => "",
        };
    }

    public void ResetVariable(Variable type)
    {
        switch (type)
        {
            case Variable.Multiplier:
                m_multiplier.Reset();
                break;
            case Variable.EnemyHP:
                m_enemyHp.Reset();
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.Reset();
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.Reset();
                break;
            case Variable.EliteEnemies:
                m_eliteEnemies.Reset();
                break;
            case Variable.EnemyShield:
                m_enemyShield.Reset();
                break;
            case Variable.TrapDamage:
                m_trapDamage.Reset();
                break;
            case Variable.LimitedStock:
                m_limitedStock.Reset();
                break;
            case Variable.UnderPressure:
                m_underPressure.Reset();
                break;
            default:
                break;
        }
    }

    public void UpdateVariablesData()
    {
        if (IsHost)
        {
            SetFloatVariablesClientRpc(GetUpdatedFloatVariables());
            SetIntVariablesClientRpc(GetUpdatedIntVariables());
        }
        else
        {
            SetFloatVariablesServerRpc(GetUpdatedFloatVariables());
            SetIntVariablesServerRpc(GetUpdatedIntVariables());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetFloatVariablesServerRpc(string floatVariablesJson)
    {
        if (!IsHost)
        {
            Dictionary<Variable, float> floatVariables = JsonConvert.DeserializeObject<Dictionary<Variable, float>>(floatVariablesJson);

            foreach (var item in floatVariables)
            {
                SetFloatVariable(item.Key, item.Value);
            }
        }
        SetFloatVariablesClientRpc(floatVariablesJson);
    }

    [ClientRpc]
    public void SetFloatVariablesClientRpc(string floatVariablesJson, ClientRpcParams clientRpcParams = default)
    {
        Dictionary<Variable, float> floatVariables = JsonConvert.DeserializeObject<Dictionary<Variable, float>>(floatVariablesJson);
        foreach (var item in floatVariables)
        {
            SetFloatVariable(item.Key, item.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetIntVariablesServerRpc(string intVariablesJson)
    {
        if (!IsHost)
        {
            Dictionary<Variable, int> intVariables = JsonConvert.DeserializeObject<Dictionary<Variable, int>>(intVariablesJson);
            foreach (var item in intVariables)
            {
                SetIntVariable(item.Key, item.Value);
            }
        }
        SetIntVariablesClientRpc(intVariablesJson);
    }

    [ClientRpc]
    public void SetIntVariablesClientRpc(string intVariablesJson, ClientRpcParams clientRpcParams = default)
    {
        Dictionary<Variable, int> intVariables = JsonConvert.DeserializeObject<Dictionary<Variable, int>>(intVariablesJson);
        foreach (var item in intVariables)
        {
            SetIntVariable(item.Key, item.Value);
        }
    }

    private void SetFloatVariable(Variable type, float value)
    {
        switch (type)
        {
            case Variable.Multiplier:
                m_multiplier.SetValue(value);
                break;
            case Variable.EnemyHP:
                m_enemyHp.SetValue(value);
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.SetValue(value);
                break;
            case Variable.EnemySpeed:
                m_multiplier.SetValue(value);
                break;
            case Variable.EliteEnemies:
                m_eliteEnemies.SetValue(value);
                break;
            case Variable.TrapDamage:
                m_trapDamage.SetValue(value);
                break;
            case Variable.UnderPressure:
                m_underPressure.SetValue(value);
                break;
        }
        CodeInjectorCanvas.Instance.UpdateVariableText(type);
    }

    private void SetIntVariable(Variable type, int value)
    {
        switch (type)
        {
            case Variable.EnemyShield:
                m_enemyShield.SetValue(value);
                break;
            case Variable.LimitedStock:
                m_limitedStock.SetValue(value);
                break;
        }
        CodeInjectorCanvas.Instance.UpdateVariableText(type);
    }

    [ClientRpc]
    private void RequestForVariableDataClientRpc()
    {
        InitializeVariables();
        SendFloatVariableDataServerRpc(GetFloatVariables());
        SendIntVariableDataServerRpc(GetIntVariables());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendFloatVariableDataServerRpc(string floatVariablesJson)
    {
        Dictionary<Variable, float> floatVariables = JsonConvert.DeserializeObject<Dictionary<Variable, float>>(floatVariablesJson);
        foreach (var item in floatVariables)
        {
            SetFloatVariable(item.Key, item.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendIntVariableDataServerRpc(string intVariablesJson)
    {
        Dictionary<Variable, int> intVariables = JsonConvert.DeserializeObject<Dictionary<Variable, int>>(intVariablesJson);
        foreach (var item in intVariables)
        {
            SetIntVariable(item.Key, item.Value);
        }
    }

    private string GetFloatVariables()
    {
        Dictionary<Variable, float> floatVariables = new Dictionary<Variable, float>();
        floatVariables.Add(Variable.Multiplier, m_multiplier.UpdatedValue);
        floatVariables.Add(Variable.EnemyHP, m_enemyHp.UpdatedValue);
        floatVariables.Add(Variable.EnemyDamage, m_enemyDamage.UpdatedValue);
        floatVariables.Add(Variable.EnemySpeed, m_enemySpeed.UpdatedValue);
        floatVariables.Add(Variable.EliteEnemies, m_eliteEnemies.UpdatedValue);
        floatVariables.Add(Variable.TrapDamage, m_trapDamage.UpdatedValue);
        floatVariables.Add(Variable.UnderPressure, m_underPressure.UpdatedValue);
        return JsonConvert.SerializeObject(floatVariables);
    }

    private string GetIntVariables()
    {
        Dictionary<Variable, int> intVariables = new Dictionary<Variable, int>();
        intVariables.Add(Variable.EnemyShield, m_enemyShield.UpdatedValue);
        intVariables.Add(Variable.LimitedStock, m_limitedStock.UpdatedValue);
        return JsonConvert.SerializeObject(intVariables);
    }

    private string GetUpdatedFloatVariables()
    {
        Dictionary<Variable, float> floatVariables = new Dictionary<Variable, float>();
        if (m_multiplier.IsChanged()) floatVariables.Add(Variable.Multiplier, m_multiplier.UpdatedValue);
        if (m_enemyHp.IsChanged()) floatVariables.Add(Variable.EnemyHP, m_enemyHp.UpdatedValue);
        if (m_enemyDamage.IsChanged()) floatVariables.Add(Variable.EnemyDamage, m_enemyDamage.UpdatedValue);
        if (m_enemySpeed.IsChanged()) floatVariables.Add(Variable.EnemySpeed, m_enemySpeed.UpdatedValue);
        if (m_eliteEnemies.IsChanged()) floatVariables.Add(Variable.EliteEnemies, m_eliteEnemies.UpdatedValue);
        if (m_trapDamage.IsChanged()) floatVariables.Add(Variable.TrapDamage, m_trapDamage.UpdatedValue);
        if (m_underPressure.IsChanged()) floatVariables.Add(Variable.UnderPressure, m_underPressure.UpdatedValue);
        return JsonConvert.SerializeObject(floatVariables);
    }

    private string GetUpdatedIntVariables()
    {
        Dictionary<Variable, int> intVariables = new Dictionary<Variable, int>();
        if (m_enemyShield.IsChanged()) intVariables.Add(Variable.EnemyShield, m_enemyShield.UpdatedValue);
        if (m_limitedStock.IsChanged()) intVariables.Add(Variable.LimitedStock, m_limitedStock.UpdatedValue);
        return JsonConvert.SerializeObject(intVariables);
    }
}
