using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionStatusCanvas : MonoBehaviour
{
    public TextMeshProUGUI StatusText;

    void Update()
    {
        if (Game.Instance == null) return;

        StatusText.text = "Connection Status: " + Game.Instance.statusString;
    }
}
