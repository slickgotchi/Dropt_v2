using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance { get; private set; }

    public GameObject Container;

    public TextMeshProUGUI localFpsText;
    public TextMeshProUGUI serverFpsText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI networkObjectCountText;
    public TextMeshProUGUI gameObjectCountText;

    private float m_fpsSampleTimer = 0;
    private List<float> m_fpsList = new List<float>();

    private int m_ping = 0;
    //private List<int> m_pingList = new List<int>();

    private int m_serverFps = 0;

    private void Awake()
    {
        Instance = this;
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
        //m_pingList.Add(ping);
        //if (m_pingList.Count > 20) m_pingList.RemoveAt(0);
        //var sum = 0;
        //foreach (var p in m_pingList) sum += p;

        //m_ping = sum / m_pingList.Count;

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
        }
    }

}
