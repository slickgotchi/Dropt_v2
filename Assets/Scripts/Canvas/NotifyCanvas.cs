using UnityEngine;
using TMPro;

public class NotifyCanvas : MonoBehaviour
{
    public static NotifyCanvas Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI m_messageText;
    [SerializeField] private GameObject m_container;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        m_container.SetActive(false);
    }

    public void SetVisible(string message)
    {
        m_messageText.text = message;
        m_container.SetActive(true);
    }

    public void ClickOnCloseButton()
    {
        m_container.SetActive(false);
    }
}
