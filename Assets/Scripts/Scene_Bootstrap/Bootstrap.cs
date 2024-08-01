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
    public static Bootstrap Instance
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
    public bool UseServerManager = false;

    public string IpAddress = "178.128.22.77";
    public ushort Port = 9000;
    public string GameId = "default";

    [SerializeField] private GameObject StartupButtons;

    void ProcessCommandLineArgs()
    {
        // get command line args
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var param = i < args.Length - 1 ? args[i + 1] : "";

            if (arg == "-server") NetworkRole = NetworkRole.Server;

            if (arg == "-autoboot") AutoBoot = true;

            if (arg == "-local") ConnectionType = ConnectionType.Local;

            if (arg == "-remote") ConnectionType = ConnectionType.Remote;

            if (arg == "-port") Port = ushort.Parse(param);

            if (arg == "-gameid") GameId = param;

        }
        Debug.Log("Bootstrap.Awake(): CL arguments processed. Connection type: " + ConnectionType
            + ", Network: " + NetworkRole + ", Port: " + Port);
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
        return Instance.NetworkRole == NetworkRole.Server;
    }

    public static bool IsHost()
    {
        return Instance.NetworkRole == NetworkRole.Host;
    }

    public static bool IsClient()
    {
        return Instance.NetworkRole == NetworkRole.Client;
    }

    public static bool IsLocalConnection()
    {
        return Instance.ConnectionType == ConnectionType.Local;
    }

    public static bool IsRemoteConnection()
    {
        return Instance.ConnectionType == ConnectionType.Remote;
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