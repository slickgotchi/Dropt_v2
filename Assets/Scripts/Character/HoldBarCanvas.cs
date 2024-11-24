using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HoldBarCanvas : MonoBehaviour
{
    [SerializeField] Slider holdSlider;
    [SerializeField] GameObject holdBar;
    [SerializeField] PlayerPrediction playerPrediction;

    private void Awake()
    {
        holdBar.SetActive(false);
        //UpdateStatBars();
    }

    void UpdateStatBars()
    {
        //var holdPercentage = playerPrediction.GetHoldPercentage();
        //if (holdPercentage <= 0 || !playerPrediction.GetComponent<NetworkObject>().IsLocalPlayer)
        //{
        //    holdBar.SetActive(false);
        //} else
        //{
        //    holdBar.SetActive(true);
        //    holdSlider.value = holdPercentage;
        //}
    }

    private void Update()
    {
        //UpdateStatBars();
    }
}
