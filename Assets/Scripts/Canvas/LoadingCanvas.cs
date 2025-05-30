using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading;
using Cysharp;
using System.Threading;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            InstaClear();
        }
    }

    public void InstaBlack()
    {
        m_animator.Play("LoadingCanvas_InstaBlack");
        m_loadingCanvasState = LoadingCanvasState.BlackedOut;
    }

    public void InstaClear()
    {
        if (m_loadingCanvasState != LoadingCanvasState.Clear)
        {
            m_animator.Play("LoadingCanvas_InstaClear");
            m_loadingCanvasState = LoadingCanvasState.Clear;
        }
    }

    public void WipeIn()
    {
        if (m_loadingCanvasState == LoadingCanvasState.Clear)
        {
            m_animator.Play("LoadingCanvas_WipeIn");
            m_loadingCanvasState = LoadingCanvasState.BlackedOut;
        }

        StartClearScreenFailSafeTimer();
        
    }

    public void WipeOut()
    {
        if (m_loadingCanvasState == LoadingCanvasState.BlackedOut)
        {
            m_animator.Play("LoadingCanvas_WipeOut");
            m_loadingCanvasState = LoadingCanvasState.Clear;
        }

    }

    private CancellationTokenSource failSafeTokenSource;

    private async void StartClearScreenFailSafeTimer()
    {
        // Cancel any existing fail-safe timer before starting a new one
        failSafeTokenSource?.Cancel();
        failSafeTokenSource?.Dispose();
        failSafeTokenSource = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(5000, cancellationToken: failSafeTokenSource.Token);

            m_animator.Play("LoadingCanvas_InstaClear");
            m_loadingCanvasState = LoadingCanvasState.Clear;
        }
        catch (System.Exception ex)
        {
            // Timer was reset, do nothing
        }
    }

    private void OnDestroy()
    {
        failSafeTokenSource?.Cancel();
        failSafeTokenSource?.Dispose();
    }
}
