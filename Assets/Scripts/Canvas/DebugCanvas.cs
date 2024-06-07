using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance { get; private set; }

    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI playerCountText;

    private float m_fpsSampleTimer = 0;
    private List<float> m_fpsList = new List<float>();

    private int m_ping = 0;
    private List<int> m_pingList = new List<int>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // update fps
        UpdateFps();

        pingText.text = "Ping: " + m_ping.ToString();

        //playerCountText.text = "Players: " + NetworkStats.Instance.ConnectedPlayers.ToString();
    }

    public void SetPing(int ping)
    {
        m_pingList.Add(ping);
        if (m_pingList.Count > 20) m_pingList.RemoveAt(0);
        var sum = 0;
        foreach (var p in m_pingList) sum += p;

        m_ping = sum / m_pingList.Count;
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
            fpsText.text = "FPS: " + Mathf.Ceil(1 / sum).ToString();
            m_fpsSampleTimer += 0.5f;
        }
    }

}
