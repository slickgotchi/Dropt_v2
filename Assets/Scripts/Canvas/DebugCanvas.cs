using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Reflection;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance { get; private set; }

    public GameObject Container;

    public TextMeshProUGUI localFpsText;
    public TextMeshProUGUI serverFpsText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI networkObjectCountText;
    public TextMeshProUGUI gameObjectCountText;
    public TextMeshProUGUI networkVariableCountText;

    private float m_fpsSampleTimer = 0;
    private List<float> m_fpsList = new List<float>();

    private int m_ping = 0;
    //private List<int> m_pingList = new List<int>();

    private int m_serverFps = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Container.SetActive(false);
    }

    private void Update()
    {
        // update fps
        UpdateFps();

        pingText.text = "Ping: " + m_ping.ToString();
        serverFpsText.text = "Server FPS: " + m_serverFps.ToString();

        UpdateNetworkObjects();
    }

    public void SetPing(int ping)
    {
        m_ping = ping;

    }

    public void SetServerFPS(int serverFPS)
    {
        m_serverFps = serverFPS;
    }

    void UpdateFps()
    {
        float dt = Time.deltaTime;
        m_fpsList.Add(dt);
        if (m_fpsList.Count > 200) m_fpsList.RemoveAt(0);
        float sum = 0;
        foreach (var fps in m_fpsList) sum += fps;
        sum /= m_fpsList.Count;
        m_fpsSampleTimer -= dt;
        if (m_fpsSampleTimer < 0)
        {
            localFpsText.text = "FPS: " + Mathf.Ceil(1 / sum).ToString();
            m_fpsSampleTimer += 0.5f;
        }
    }

    private float m_noTimer = 0f;

    void UpdateNetworkObjects()
    {
        m_noTimer -= Time.deltaTime;

        if (m_noTimer < 0)
        {
            m_noTimer = 1f;

            var networkObjects = FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            networkObjectCountText.text = "NetworkObjects: " + networkObjects.Length.ToString("F0");

            var gameObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            gameObjectCountText.text = "GameObjects: " + gameObjects.Length.ToString("F0");

            var networkVariables = GetAllNetworkVariables();
            networkVariableCountText.text = "Network Variables: " + networkVariables.Count.ToString("F0");

        }
    }


    List<object> GetAllNetworkVariables()
    {
        List<object> networkVariables = new List<object>();

        foreach (var networkBehaviour in FindObjectsOfType<NetworkBehaviour>(true))
        {
            var fields = networkBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(NetworkVariable<>))
                {
                    var value = field.GetValue(networkBehaviour);
                    if (value != null)
                    {
                        networkVariables.Add(value);
                    }
                }
            }
        }

        return networkVariables;
    }
}
