using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas Instance { get; private set; }
    
    [SerializeField] private TMPro.TextMeshProUGUI m_droppingText;

    [SerializeField] private Animator m_animator;

    private enum LoadingCanvasState { Clear, BlackedOut }
    private LoadingCanvasState m_loadingCanvasState = LoadingCanvasState.Clear;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InstaBlack()
    {
        m_animator.Play("LoadingCanvas_InstaBlack");

        m_loadingCanvasState = LoadingCanvasState.BlackedOut;
    }

    public void InstaClear()
    {
        m_animator.Play("LoadingCanvas_InstaClear");

        m_loadingCanvasState = LoadingCanvasState.Clear;
    }

    public void WipeIn()
    {
        if (m_loadingCanvasState == LoadingCanvasState.Clear)
        {
            m_animator.Play("LoadingCanvas_WipeIn");
            m_loadingCanvasState = LoadingCanvasState.BlackedOut;
            Debug.Log("Wipe In");
        }

    }

    public void WipeOut()
    {
        if (m_loadingCanvasState == LoadingCanvasState.BlackedOut)
        {
            m_animator.Play("LoadingCanvas_WipeOut");

            m_loadingCanvasState = LoadingCanvasState.Clear;

            Debug.Log("Wipe Out");
        }

    }
}
