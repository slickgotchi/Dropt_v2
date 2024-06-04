using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Game : MonoBehaviour
{
    private void Awake()
    {
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;

        if (Bootstrap.IsRemoteConnection())
        {
            transport.UseEncryption = true;

            transport.SetConnectionData("178.128.22.77", 8484, "0.0.0.0");
            transport.SetServerSecrets(SecureParameters.ServerCertificate, SecureParameters.ServerPrivateKey);
            transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.ClientCA);
        }

        else
        {
            transport.UseEncryption = false;
            transport.SetConnectionData("127.0.0.1", 8484, "0.0.0.0");
        }
    }

    void Start()
    {
        // startup network 
        if (Bootstrap.IsHost()) NetworkManager.Singleton.StartHost();
        else if (Bootstrap.IsServer()) NetworkManager.Singleton.StartServer();
        else if (Bootstrap.IsClient()) NetworkManager.Singleton.StartClient();
    }
}
