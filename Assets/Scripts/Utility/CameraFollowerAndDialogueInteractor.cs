using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowerAndDialogueInteractor : MonoBehaviour
{
    public static CameraFollowerAndDialogueInteractor Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void FocusDialogue()
    {

    }

    void UnfocusDialogue()
    {

    }
}
