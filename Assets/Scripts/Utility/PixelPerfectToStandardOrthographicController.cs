using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public class PixelPerfectToStandardOrthographicController : MonoBehaviour
{
    private Camera m_camera;
    private PixelPerfectCamera m_pixelPerfectCamera;
    public CinemachineVirtualCamera m_cinemachineVirtualCamera;

    private float m_lowRange = 16 / 11;
    private float m_highRange = 16 / 8;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
        m_pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
    }

    private void Start()
    {
        //m_pixelPerfectCamera.gameObject.SetActive(false);
        //m_camera.orthographicSize = 8.33f;
    }

    private void Update()
    {
        var width = Screen.width;
        var height = Screen.height;
        var ratio = width / height;

        if (ratio < m_highRange && ratio > m_lowRange)
        {
            m_pixelPerfectCamera.gameObject.SetActive(false);
            //m_camera.orthographicSize = 8.33f;
            m_cinemachineVirtualCamera.m_Lens.OrthographicSize = 8.33f;
        }
        else
        {
            m_pixelPerfectCamera.gameObject.SetActive(true);
        }
    }
}
