using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    private CinemachineVirtualCamera m_virtualCamera;
    private CinemachineBasicMultiChannelPerlin m_perlin;
    private float m_startingIntensity;
    private float m_shakeTimer_s;
    private float m_shakeTimerTotal_s;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        return;

        var virtualCameraGameObject = GameObject.FindGameObjectWithTag("VirtualCamera");
        if (virtualCameraGameObject == null)
        {
            Debug.LogWarning("No virtual camera exists in the scene");
            return;
        }

        m_virtualCamera = virtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
        m_perlin = m_virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity = 1.5f, float time = 0.3f)
    {
        // lets get rid of camera shake for now
        return;

        if (!IsLocalPlayer || !IsSpawned) return;

        m_perlin.m_AmplitudeGain = intensity;
        m_shakeTimer_s = time;
        m_shakeTimerTotal_s = time;
        m_startingIntensity = intensity;
    }

    // Update is called once per frame
    void Update()
    {
        return;

        if (!IsLocalPlayer || !IsSpawned) return;

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
