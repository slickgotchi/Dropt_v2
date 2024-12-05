using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas Instance { get; private set; }
    
    [SerializeField] private TMPro.TextMeshProUGUI m_droppingText;

    [SerializeField] private Animator m_animator;

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
        m_animator.Play("LoadingCanvas_Default");
    }

    public void WipeIn()
    {
        m_animator.Play("LoadingCanvas_WipeIn");
    }

    public void WipeOut()
    {
        m_animator.Play("LoadingCanvas_WipeOut");
    }
}
