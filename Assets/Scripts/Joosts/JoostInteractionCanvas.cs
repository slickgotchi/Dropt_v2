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

    [SerializeField] private Color toPurchaseColor;
    [SerializeField] private Color isPurchasedColor;

    private void Awake()
    {
        Instance = this;
        Container.SetActive(false);
        PurchasedText.text = "Purchase?";
        PurchasedText.color = toPurchaseColor;
    }

    public void Init(string name, string description, string cost, bool isPurchased)
    {
        NameText.text = name;
        DescriptionText.text = description;
        CostText.text = cost;

        if (isPurchased)
        {
            PurchasedText.text = "[ Purchased ]";
            PurchasedText.color = isPurchasedColor;
        }
        else
        {
            PurchasedText.text = "Purchase?";
            PurchasedText.color = toPurchaseColor;
        }
    }
}
