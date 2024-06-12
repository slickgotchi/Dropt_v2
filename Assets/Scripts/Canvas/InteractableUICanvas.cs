using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableUICanvas : MonoBehaviour
{
    public static InteractableUICanvas Instance {  get; private set; }

    public GameObject InteractTextbox;

    private void Awake()
    {
        Instance = this;
        InteractTextbox.SetActive(false);
    }
}
