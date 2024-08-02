using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ErrorDialogCanvas : MonoBehaviour
{
    public static ErrorDialogCanvas Instance;

    public GameObject Container;
    public TextMeshProUGUI Text;
    public Button ExitButton;

    void Awake()
    {
        Instance = this;
        ExitButton.onClick.AddListener(HandleClick_ExitButton);
        Hide();
    }

    void HandleClick_ExitButton()
    {
        Hide();
    }

    public void Show(string message)
    {
        Container.SetActive(true);
        Text.text = message;
    }

    public void Hide()
    {
        Container.SetActive(false);
    }
}
