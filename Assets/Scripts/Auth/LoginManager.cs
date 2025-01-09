using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LoginManager : NetworkBehaviour
{
    public static LoginManager Instance { get; private set; }

    private string m_walletAddress = "";
    private int m_selectedGotchiId = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


}
