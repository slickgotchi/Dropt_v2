using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BazaarItemCard : MonoBehaviour
{
    [Header("Assign in Scene")]
    [SerializeField] private BazaarItem_SO m_bazaarItem_SO;

    [Header("Assign in Prefab")]
    [SerializeField] private Image m_image;
    [SerializeField] private TextMeshProUGUI m_titleText;
    [SerializeField] private TextMeshProUGUI m_descriptionText;
    [SerializeField] private TextMeshProUGUI m_ghstCost;
    [SerializeField] private Button buyButton;

    private void Awake()
    {
        buyButton.onClick.AddListener(HandleClickBuy);
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveAllListeners();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_bazaarItem_SO == null)
        {
            Debug.LogWarning("No BazaarItem_SO assigned to BazaarItemCard!");
            return;
        }

        m_image.sprite = m_bazaarItem_SO.sprite;
        m_titleText.text = m_bazaarItem_SO.titleText + " x" + m_bazaarItem_SO.purchaseQty.ToString();
        m_descriptionText.text = m_bazaarItem_SO.descriptionText;
        m_ghstCost.text = m_bazaarItem_SO.ghstCost.ToString();
    }

    void HandleClickBuy()
    {
        Debug.Log("Buy " + m_titleText.text + " for " + m_ghstCost.text + " GHST");
    }
}
