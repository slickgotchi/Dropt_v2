using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Game : MonoBehaviour
{
    private void Start()
    {
        //test Nike
        var transport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;

        if (transport == null)
        {
            Debug.Log("Could not get UnityTransport");
            return;
        }

        if (Bootstrap.IsRemoteConnection())
        {
            transport.UseEncryption = true;

            transport.SetConnectionData("178.128.22.77", Bootstrap.Instance.Port, "0.0.0.0");

            if (Bootstrap.IsServer() || Bootstrap.IsHost())
            {
                transport.SetServerSecrets(SecureParameters.ServerCertificate, SecureParameters.ServerPrivateKey);
            } 
            
            if (Bootstrap.IsClient() || Bootstrap.IsHost())
            {
                transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.ClientCA);
            }
        }

        else
        {
            transport.UseEncryption = false;
            transport.SetConnectionData("127.0.0.1", Bootstrap.Instance.Port, "0.0.0.0");
        }

        // startup network 
        if (Bootstrap.IsHost())
        {
            Debug.Log("StartHost()");
            NetworkManager.Singleton.StartHost();
        }
        else if (Bootstrap.IsServer())
        {
            Debug.Log("StartServer()");
            NetworkManager.Singleton.StartServer();
        }
        else if (Bootstrap.IsClient())
        {
            Debug.Log("StartClient");
            NetworkManager.Singleton.StartClient();
        }
    }
}
