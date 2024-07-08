using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacterDebugCanvasToggler : MonoBehaviour
{
    private bool m_isVisible = false;

    private void Start()
    {
        SetVisibile(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetVisibile(!m_isVisible);
        }
    }

    void SetVisibile(bool visible)
    {
        var canvii = FindObjectsByType<NetworkCharacterDebugCanvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvii)
        {
            canvas.Container.SetActive(visible);
        }

        m_isVisible = visible;
    }
}
