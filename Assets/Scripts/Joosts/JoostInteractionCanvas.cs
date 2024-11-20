using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoostInteractionCanvas : MonoBehaviour
{
    public static JoostInteractionCanvas Instance { get; private set; }

    public GameObject Container;

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI PurchasedText;

    private void Awake()
    {
        Instance = this;
        Container.SetActive(false);
        PurchasedText.gameObject.SetActive(false);
    }

    public void Init(string name, string description, string cost)
    {
        NameText.text = name;
        DescriptionText.text = description;
        CostText.text = cost;
    }
}
