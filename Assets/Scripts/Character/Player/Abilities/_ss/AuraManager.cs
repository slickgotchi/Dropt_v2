using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraManager : MonoBehaviour
{
    public static AuraManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public bool IsAaveFlagActive = false;
    public bool IsJamaicanFlagActive = false;
    public bool IsL2SignActive = false;
    public bool IsOKExSignActive = false;
    public bool IsRektSignActive = false;
    public bool IsUpArrowActive = false;
    public bool IsVoteSignActive = false;
}
