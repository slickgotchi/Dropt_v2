using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseSuccessfulModal : MonoBehaviour
{
    [SerializeField] private Button m_continueButton;

    private void Awake()
    {
        m_continueButton.onClick.AddListener(HandleClick_Continue);
    }

    void HandleClick_Continue()
    {
        gameObject.SetActive(false);
    }
}
