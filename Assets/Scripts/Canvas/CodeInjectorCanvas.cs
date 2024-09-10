using UnityEngine;
using TMPro;

public class CodeInjectorCanvas : MonoBehaviour
{
    public static CodeInjectorCanvas Instance { get; private set; }

    [SerializeField] private GameObject m_container;

    [SerializeField] private TextMeshProUGUI m_enemyHpText;
    [SerializeField] private TextMeshProUGUI m_enemyDamageText;
    [SerializeField] private TextMeshProUGUI m_enemySpeedText;
    [SerializeField] private TextMeshProUGUI m_eliteEnemiesText;
    [SerializeField] private TextMeshProUGUI m_enemyShieldText;
    [SerializeField] private TextMeshProUGUI m_trapDamageText;
    [SerializeField] private TextMeshProUGUI m_limitedStockText;
    [SerializeField] private TextMeshProUGUI m_underPressureText;

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

    public void SetVisible(bool isVisible)
    {
        m_container.SetActive(isVisible);
    }

    public void InitializeVariableValues()
    {

    }
}
