using Cysharp.Threading.Tasks;
using System;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

// FLOWS
// Server
// 1. -server has been passed so we go directly into


public class Bootstrap : MonoBehaviour
{
    private static Bootstrap _singleton;
    public static Bootstrap Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindAnyObjectByType<Bootstrap>() ?? new GameObject("Bootstrap").AddComponent<Bootstrap>();
            }
            return _singleton;
        }
    }

    void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        _singleton = this;
        DontDestroyOnLoad(gameObject);

        ProcessCommandLineArgs();

        // Hide network role canvas if we are autobooting
        if (AutoBoot)
        {
            StartupButtons.gameObject.SetActive(false);
        }
    }

    public NetworkRole NetworkRole = NetworkRole.Host;
    public ConnectionType ConnectionType = ConnectionType.Local;
    public bool AutoBoot = false;

    [SerializeField] private GameObject StartupButtons;

    void ProcessCommandLineArgs()
    {
        // get command line args
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var param = i < args.Length - 1 ? args[i + 1] : "";

            if (arg == "-server")
            {
                NetworkRole = NetworkRole.Server;
                AutoBoot = true;
                ConnectionType = ConnectionType.Remote;
            }
        }
        Debug.Log("Bootstrap.Awake(): Command line arguments processed");
    }

    private void Start()
    {
        // check for autoboot
        if (AutoBoot)
        {
            if (NetworkRole == NetworkRole.Server)
            {
                SceneManager.LoadScene("Game");
            } else
            {
                SceneManager.LoadScene("Title");
            }
        }
    }

    public static bool IsServer()
    {
        return Singleton.NetworkRole == NetworkRole.Server;
    }

    public static bool IsHost()
    {
        return Singleton.NetworkRole == NetworkRole.Host;
    }

    public static bool IsClient()
    {
        return Singleton.NetworkRole == NetworkRole.Client;
    }

    public static bool IsLocalConnection()
    {
        return Singleton.ConnectionType == ConnectionType.Local;
    }

    public static bool IsRemoteConnection()
    {
        return Singleton.ConnectionType == ConnectionType.Remote;
    }
}

public enum NetworkRole
{
    Host, Server, Client,
}

public enum ConnectionType
{
    Local, Remote,
}