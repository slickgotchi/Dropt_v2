using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUICanvas : MonoBehaviour
{
    public static InteractableUICanvas Instance {  get; private set; }

    public GameObject InteractTextbox;
    public Slider InteractSlider;

    private void Awake()
    {
        Instance = this;
        InteractTextbox.SetActive(false);
    }
}
