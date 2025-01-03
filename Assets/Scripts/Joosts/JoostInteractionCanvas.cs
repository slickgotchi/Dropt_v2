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

    [SerializeField] private Color m_availableColor;
    [SerializeField] private Color m_consumedColor;
    [SerializeField] private Color m_insufficientEctoColor;

    public enum PurchaseState { Available, Consumed, InsufficientEcto }
    [HideInInspector] public PurchaseState purchaseState = PurchaseState.Available;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Container.SetActive(false);
        PurchasedText.text = "Purchase?";
        PurchasedText.color = m_availableColor;
    }

    public void Set(string name, string description, string cost, PurchaseState purchaseState)
    {
        NameText.text = name;
        DescriptionText.text = description;
        CostText.text = cost;

        switch (purchaseState)
        {
            case PurchaseState.Available:
                PurchasedText.text = "Purchase?";
                PurchasedText.color = m_availableColor;
                break;
            case PurchaseState.Consumed:
                PurchasedText.text = "Consuumed";
                PurchasedText.color = m_consumedColor;
                break;
            case PurchaseState.InsufficientEcto:
                PurchasedText.text = "Insufficient Ecto";
                PurchasedText.color = m_insufficientEctoColor;
                break;
            default:
                break;
        }
    }
}
