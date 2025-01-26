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

#if UNITY_EDITOR
        m_isUnityEditor = true;
#endif
    }

    public enum Region { America, Europe, Asia };
    public Region region = Region.America;

    public NetworkRole NetworkRole = NetworkRole.Host;
    public ConnectionType ConnectionType = ConnectionType.Local;
    public bool AutoBoot = false;
    public bool AutoPlay = false;
    public bool UseServerManager = false;
    public bool ShowTutorialLevel = false;
    public string Version = "0.0.0";

    public string IpAddress = "178.128.22.77";
    public ushort GamePort = 9000;
    public ushort WorkerPort = 3000;
    public string GameId = "";
    public string HeartbeatSecret = "";
    public string DbSecret = "";
    public string DomainName = "";

    public int TestBlockChainGotchiId = 0;

    public string TestWalletAddress = "0xtest";

    private bool m_isUnityEditor = false;

    [Tooltip("Toggles player equipment debug canvas")]
    public bool isPlayerEquipmentSwapperActive = false;

    [HideInInspector] public bool isJoiningFromTitle = false;

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

            if (arg == "-gameport") GamePort = ushort.Parse(param);

            if (arg == "-workerport") WorkerPort = ushort.Parse(param);

            if (arg == "-gameid") GameId = param;

            if (arg == "-noservermanager") UseServerManager = false;

            if (arg == "-ipaddress") IpAddress = param;

            if (arg == "-heartbeatsecret") HeartbeatSecret = param;

            if (arg == "-dbsecret") DbSecret = param;

            if (arg == "-domainname") DomainName = param; 
        }
    }

    private void Start()
    {
        // check for autoboot
        if (AutoBoot)
        {
            if (NetworkRole == NetworkRole.Server)
            {
                SceneManager.LoadScene("Game");
            }
            else
            {
                if (AutoPlay)
                {
                    SceneManager.LoadScene("Game");
                } else
                {
                    SceneManager.LoadScene("Title");
                }
            }
        }
    }

    public static string RegionToString(Region region)
    {
        if (region == Region.America) return "america";
        else if (region == Region.Europe) return "europe";
        else return "asia";
    }

    public static string GetRegionString()
    {
        return RegionToString(Instance.region);
    }

    private void OnDestroy()
    {
        if (!IsServer())
        {
            //GameAudioManager.TryToDispose();
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

    public static bool IsUnityEditor()
    {
        return Instance.m_isUnityEditor;
    }

    public static bool IsUseServerManager()
    {
        return Instance.UseServerManager;
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