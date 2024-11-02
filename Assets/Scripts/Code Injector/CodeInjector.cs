using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CodeInjector : NetworkBehaviour
{
    public enum Variable
    {
        EnemyHP,
        EnemyDamage,
        EnemySpeed,
        //EliteEnemies,
        EnemyShield,
        //TrapDamage,
        //LimitedStock,
        EssencePressure
    }

    public static CodeInjector Instance { get; private set; }

    [SerializeField] private CodeInjectorFloat m_enemyHp;
    [SerializeField] private CodeInjectorFloat m_enemyDamage;
    [SerializeField] private CodeInjectorFloat m_enemySpeed;
    //[SerializeField] private CodeInjectorFloat m_eliteEnemies;
    [SerializeField] private CodeInjectorInt m_enemyShield;
    //[SerializeField] private CodeInjectorFloat m_trapDamage;
    //[SerializeField] private CodeInjectorInt m_limitedStock;
    //[SerializeField] private CodeInjectorFloat m_underPressure;
    [SerializeField] private CodeInjectorFloat m_essencePressure;

    public CodeInjectorFloat EnemyHp => m_enemyHp;
    public CodeInjectorFloat EnemyDamage => m_enemyDamage;
    public CodeInjectorFloat EnemySpeed => m_enemySpeed;
    //public CodeInjectorFloat EliteEnemies => m_eliteEnemies;
    public CodeInjectorInt EnemyShield => m_enemyShield;
    //public CodeInjectorFloat TrapDamage => m_trapDamage;
    //public CodeInjectorInt LimitedStock => m_limitedStock;
    //public CodeInjectorFloat UnderPressure => m_underPressure;
    public CodeInjectorFloat EssencePressure => m_essencePressure;

    private float m_outputMultiplier;

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
        m_enemyHp.Initialize();
        m_enemyDamage.Initialize();
        m_enemySpeed.Initialize();
        //m_eliteEnemies.Initialize();
        m_enemyShield.Initialize();
        //m_trapDamage.Initialize();
        //m_limitedStock.Initialize();
        m_essencePressure.Initialize();
        UpdateOutputMultiplier();
        CodeInjectorCanvas.Instance.UpdateVariables();
    }

    public void AddVariable(Variable type)
    {
        switch (type)
        {
            case Variable.EnemyHP:
                m_enemyHp.Add();
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.Add();
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.Add();
                break;
            //case Variable.EliteEnemies:
            //    m_eliteEnemies.Add();
            //    break;
            case Variable.EnemyShield:
                m_enemyShield.Add();
                break;
            //case Variable.TrapDamage:
            //    m_trapDamage.Add();
            //    break;
            //case Variable.LimitedStock:
            //    m_limitedStock.Add();
            //    break;
            case Variable.EssencePressure:
                m_essencePressure.Add();
                break;
            default:
                break;
        }
        UpdateOutputMultiplier();
    }

    public void SubtractVariable(Variable type)
    {
        switch (type)
        {
            case Variable.EnemyHP:
                m_enemyHp.Subtract();
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.Subtract();
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.Subtract();
                break;
            //case Variable.EliteEnemies:
            //    m_eliteEnemies.Subtract();
            //    break;
            case Variable.EnemyShield:
                m_enemyShield.Subtract();
                break;
            //case Variable.TrapDamage:
            //    m_trapDamage.Subtract();
            //    break;
            //case Variable.LimitedStock:
            //    m_limitedStock.Subtract();
            //    break;
            case Variable.EssencePressure:
                m_essencePressure.Subtract();
                break;
        }
        UpdateOutputMultiplier();
    }

    public string GetVariableString(Variable type)
    {
        return type switch
        {
            Variable.EnemyHP => m_enemyHp.ToString(),
            Variable.EnemyDamage => m_enemyDamage.ToString(),
            Variable.EnemySpeed => m_enemySpeed.ToString(),
            //Variable.EliteEnemies => m_eliteEnemies.ToString(),
            Variable.EnemyShield => m_enemyShield.ToString(),
            //Variable.TrapDamage => m_trapDamage.ToString(),
            //Variable.LimitedStock => m_limitedStock.ToString(),
            Variable.EssencePressure => m_essencePressure.ToString(),
            _ => "",
        };
    }

    public void ResetUpdatedVariablesValue()
    {
        m_enemyHp.ResetUpdatedValue();
        m_enemyDamage.ResetUpdatedValue();
        m_enemySpeed.ResetUpdatedValue();
        //m_eliteEnemies.ResetUpdatedValue();
        m_enemyShield.ResetUpdatedValue();
        //m_trapDamage.ResetUpdatedValue();
        //m_limitedStock.ResetUpdatedValue();
        m_essencePressure.ResetUpdatedValue();
        UpdateOutputMultiplier();
    }

    public void ResetUpdatedVariablesValueToDefalut()
    {
        m_enemyHp.ResetToDefault();
        m_enemyDamage.ResetToDefault();
        m_enemySpeed.ResetToDefault();
        //m_eliteEnemies.ResetToDefault();
        m_enemyShield.ResetToDefault();
        //m_trapDamage.ResetToDefault();
        //m_limitedStock.ResetToDefault();
        m_essencePressure.ResetToDefault();
        UpdateOutputMultiplier();
    }

    public void UpdateVariablesData()
    {
        if (IsHost)
        {
            SetFloatVariablesClientRpc(GetUpdatedFloatVariables());
            SetIntVariablesClientRpc(GetUpdatedIntVariables());
            NotifyNewCodeInjectedClientRpc(NetworkManager.SpawnManager.GetLocalPlayerObject().NetworkObjectId);
        }
        else
        {
            SetFloatVariablesServerRpc(GetUpdatedFloatVariables());
            SetIntVariablesServerRpc(GetUpdatedIntVariables());
            NotifyNewCodeInjectedServerRpc(NetworkManager.SpawnManager.GetLocalPlayerObject().NetworkObjectId);
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
            UpdateOutputMultiplier();
            CodeInjectorCanvas.Instance.UpdateVariables();
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
        UpdateOutputMultiplier();
        CodeInjectorCanvas.Instance.UpdateVariables();
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
            UpdateOutputMultiplier();
            CodeInjectorCanvas.Instance.UpdateVariables();
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
        UpdateOutputMultiplier();
        CodeInjectorCanvas.Instance.UpdateVariables();
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyNewCodeInjectedServerRpc(ulong networkObjectId)
    {
        NotifyNewCodeInjectedClientRpc(networkObjectId);
    }

    [ClientRpc]
    public void NotifyNewCodeInjectedClientRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.GetLocalPlayerObject().NetworkObjectId.Equals(networkObjectId))
        {
            NotifyCanvas.Instance.SetVisible($"Player - {networkObjectId} team mate injected new code and adjusted the difficulty of the dungeon!");
        }
    }

    private void SetFloatVariable(Variable type, float value)
    {
        switch (type)
        {
            case Variable.EnemyHP:
                m_enemyHp.SetValue(value);
                break;
            case Variable.EnemyDamage:
                m_enemyDamage.SetValue(value);
                break;
            case Variable.EnemySpeed:
                m_enemySpeed.SetValue(value);
                break;
            //case Variable.EliteEnemies:
            //    //m_eliteEnemies.SetValue(value);
            //    break;
            //case Variable.TrapDamage:
            //    m_trapDamage.SetValue(value);
            //    break;
            case Variable.EssencePressure:
                m_essencePressure.SetValue(value);
                break;
        }
    }

    private void SetIntVariable(Variable type, int value)
    {
        switch (type)
        {
            case Variable.EnemyShield:
                m_enemyShield.SetValue(value);
                break;
                //case Variable.LimitedStock:
                //    m_limitedStock.SetValue(value);
                //    break;
        }
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
        UpdateOutputMultiplier();
        CodeInjectorCanvas.Instance.UpdateVariables();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendIntVariableDataServerRpc(string intVariablesJson)
    {
        Dictionary<Variable, int> intVariables = JsonConvert.DeserializeObject<Dictionary<Variable, int>>(intVariablesJson);
        foreach (var item in intVariables)
        {
            SetIntVariable(item.Key, item.Value);
        }
        UpdateOutputMultiplier();
        CodeInjectorCanvas.Instance.UpdateVariables();
    }

    private string GetFloatVariables()
    {
        Dictionary<Variable, float> floatVariables = new Dictionary<Variable, float>();
        floatVariables.Add(Variable.EnemyHP, m_enemyHp.GetUpdatedValue());
        floatVariables.Add(Variable.EnemyDamage, m_enemyDamage.GetUpdatedValue());
        floatVariables.Add(Variable.EnemySpeed, m_enemySpeed.GetUpdatedValue());
        //floatVariables.Add(Variable.EliteEnemies, m_eliteEnemies.GetUpdatedValue());
        //floatVariables.Add(Variable.TrapDamage, m_trapDamage.GetUpdatedValue());
        floatVariables.Add(Variable.EssencePressure, m_essencePressure.GetUpdatedValue());
        return JsonConvert.SerializeObject(floatVariables);
    }

    private string GetIntVariables()
    {
        Dictionary<Variable, int> intVariables = new Dictionary<Variable, int>();
        intVariables.Add(Variable.EnemyShield, m_enemyShield.GetUpdatedValue());
        //intVariables.Add(Variable.LimitedStock, m_limitedStock.GetUpdatedValue());
        return JsonConvert.SerializeObject(intVariables);
    }

    private string GetUpdatedFloatVariables()
    {
        Dictionary<Variable, float> floatVariables = new Dictionary<Variable, float>();
        if (m_enemyHp.IsChanged())
        {
            floatVariables.Add(Variable.EnemyHP, m_enemyHp.GetUpdatedValue());
        }

        if (m_enemyDamage.IsChanged())
        {
            floatVariables.Add(Variable.EnemyDamage, m_enemyDamage.GetUpdatedValue());
        }

        if (m_enemySpeed.IsChanged())
        {
            floatVariables.Add(Variable.EnemySpeed, m_enemySpeed.GetUpdatedValue());
        }

        //if (m_eliteEnemies.IsChanged())
        //{
        //    floatVariables.Add(Variable.EliteEnemies, m_eliteEnemies.GetUpdatedValue());
        //}

        //if (m_trapDamage.IsChanged())
        //{
        //    floatVariables.Add(Variable.TrapDamage, m_trapDamage.GetUpdatedValue());
        //}

        if (m_essencePressure.IsChanged())
        {
            floatVariables.Add(Variable.EssencePressure, m_essencePressure.GetUpdatedValue());
        }

        return JsonConvert.SerializeObject(floatVariables);
    }

    private string GetUpdatedIntVariables()
    {
        Dictionary<Variable, int> intVariables = new Dictionary<Variable, int>();
        if (m_enemyShield.IsChanged())
        {
            intVariables.Add(Variable.EnemyShield, m_enemyShield.GetUpdatedValue());
        }

        //if (m_limitedStock.IsChanged())
        //{
        //    intVariables.Add(Variable.LimitedStock, m_limitedStock.GetUpdatedValue());
        //}

        return JsonConvert.SerializeObject(intVariables);
    }

    private void UpdateOutputMultiplier()
    {
        //Debug.Log("UpdateOutputMultiplier");
        m_outputMultiplier = m_enemyHp.GetMultiplier()
                             * m_enemyDamage.GetMultiplier()
                             * m_enemySpeed.GetMultiplier()
                             //* m_eliteEnemies.GetMultiplier()
                             * m_enemyShield.GetMultiplier()
                             //* m_trapDamage.GetMultiplier()
                             //* m_limitedStock.GetMultiplier()
                             * m_essencePressure.GetMultiplier();
        CodeInjectorCanvas.Instance.UpdateOutputMultiplier();
        //Debug.Log("UpdateOutputMultiplier - " + m_outputMultiplier);
    }

    public float GetOutputMultiplier()
    {
        return m_outputMultiplier;
    }
}