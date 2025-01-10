using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

// WebGLFocusManager
// - this manager just tries to provide players some feedback of why they might get
//   disconnected when they tab away from the screen

public class WebGLFocusManager : MonoBehaviour
{
    private System.DateTime? m_focusLostTimestamp = null;

    public static WebGLFocusManager Instance { get; private set; }

    public bool isClientNetworkManagerConnected = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Application.focusChanged += OnApplicationFocusChanged;

    }

    private void OnDestroy()
    {
        Application.focusChanged -= OnApplicationFocusChanged;

    }

    void OnApplicationFocusChanged(bool hasFocus)
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (!hasFocus)
        {
            // Record the timestamp when focus is lost
            m_focusLostTimestamp = System.DateTime.UtcNow;
            Debug.Log("Lost focus, taking a timestamp");
        }
        else
        {
            // When focus is regained, check the elapsed time
            if (m_focusLostTimestamp.HasValue)
            {
                double elapsedMilliseconds = (System.DateTime.UtcNow - m_focusLostTimestamp.Value).TotalMilliseconds;

                if (elapsedMilliseconds > unityTransport.DisconnectTimeoutMS && !isClientNetworkManagerConnected)
                {
                    Debug.Log("You were disconnected due to inactivity while the game was unfocused.");
                    ErrorDialogCanvas.Instance.Show("Disconnected from the server due to being out of focus (tabbed out) longer than " + unityTransport.DisconnectTimeoutMS + "s. Please refresh for a new game!");
                    SceneManager.LoadScene("Title");
                }
                else
                {
                    Debug.Log("Welcome back! No disconnection occurred.");
                }

                // Reset the timestamp
                m_focusLostTimestamp = null;
            }
        }
    }
}
