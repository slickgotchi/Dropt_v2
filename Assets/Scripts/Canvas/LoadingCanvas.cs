using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas Instance { get; private set; }

    public Animator Animator;

    private void Awake()
    {
        Instance = this;
        Animator = GetComponent<Animator>();
    }
}
