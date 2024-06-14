using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_virtualCamera;
    private CinemachineBasicMultiChannelPerlin m_perlin;
    private float m_startingIntensity;
    private float m_shakeTimer_s;
    private float m_shakeTimerTotal_s;

    private void Awake()
    {
        m_perlin = m_virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity, float time)
    {
        if (!IsLocalPlayer) return;

        m_perlin.m_AmplitudeGain = intensity;
        m_shakeTimer_s = time;
        m_shakeTimerTotal_s = time;
        m_startingIntensity = intensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer) return;

        m_shakeTimer_s -= Time.deltaTime;
        if (m_shakeTimer_s > 0)
        {
            math.lerp(m_startingIntensity, 0, m_shakeTimer_s / m_shakeTimerTotal_s);
        }
        else if (m_shakeTimer_s <= 0)
        {
            m_perlin.m_AmplitudeGain = 0f;
        }
    }
}
