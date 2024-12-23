using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class REKTCanvas : MonoBehaviour
{
    public static REKTCanvas Instance { get; private set; }

    public GameObject Container;
    public Button DegenapeButton;

    public enum TypeOfREKT { HP, Essence, InActive, Escaped }
    public TypeOfREKT Type = TypeOfREKT.HP;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI ReasonText;

    public Color EscapeTitleTextColor;
    public Color EscapeReasonTextColor;
    public Color REKTTitleTextColor;
    public Color REKTReasonTextColor;

    [SerializeField] private TextMeshProUGUI m_BankEctoDeltaText;
    [SerializeField] private TextMeshProUGUI m_GotchiDustCollectedText;
    [SerializeField] private TextMeshProUGUI m_BombsUsedText;
    [SerializeField] private TextMeshProUGUI m_EnemiesSlainText;
    [SerializeField] private TextMeshProUGUI m_DestructiblesSmashedText;
    [SerializeField] private TextMeshProUGUI m_dungeonTimerText;

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

        DegenapeButton.onClick.AddListener(HandleClickDegenapeButton);
    }

    //private bool m_isDisconnectTimerActive = false;
    //private float m_disconnectTimer = 0f;
    //private float k_disconnectTime = 10f;

    public void Update()
    {
        if (!Bootstrap.IsClient()) return;

        //m_disconnectTimer -= Time.deltaTime;
        //if (m_isDisconnectTimerActive && m_disconnectTimer < 0)
        //{
        //    m_isDisconnectTimerActive = false;
        //    NetworkManager.Singleton.Shutdown();
        //}

        //if (Container.activeSelf)
        //{
        //    InitializePlayerInfo();
        //}
    }

    public void Show(TypeOfREKT type)
    {
        //m_isDisconnectTimerActive = true;
        //m_disconnectTimer = k_disconnectTime;

        InitializePlayerInfo();

        Type = type;
        Container.SetActive(true);

        // set text colors
        if (type == TypeOfREKT.Escaped)
        {
            TitleText.text = "ESCAPED";
            TitleText.color = EscapeTitleTextColor;
            ReasonText.color = EscapeReasonTextColor;
        }
        else
        {
            TitleText.text = "REKT";
            TitleText.color = REKTTitleTextColor;
            ReasonText.color = REKTReasonTextColor;
        }

        // display text based on how we game over'd
        switch (type)
        {
            case TypeOfREKT.HP:
                ReasonText.text = "You ran out of HP... dungeons can be tough huh?";
                break;
            case TypeOfREKT.Essence:
                ReasonText.text = "You ran out of Essence... maybe catch a lil essence once in a while?";
                break;
            case TypeOfREKT.Escaped:
                ReasonText.text = "You successfully escaped with your collected treasures. Maybe a little deeper next time?";
                break;
            case TypeOfREKT.InActive:
                ReasonText.text = "You have been inactive for longer than " + PlayerController.InactiveTimerDuration.ToString("F0") + "s so... got the boot!";
                break;
        }

        DegenapeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return to Degenape Village";

        //InitializeTime();

        NetworkManager.Singleton.Shutdown();
    }

    private void HandleClickDegenapeButton()
    {
        //m_isDisconnectTimerActive = false;
        //NetworkManager.Singleton.Shutdown();
        Container.SetActive(false);
        Bootstrap.Instance.GameId = "";
        SceneManager.LoadScene("Game");
    }

    private void InitializePlayerInfo()
    {
        GameObject player = GetLocalPlayer();
        if (player == null)
        {
            Debug.LogWarning("No local player found");
            return;
        }

        PlayerOffchainData playerOffchainData = player.GetComponent<PlayerOffchainData>();

        int ectoDelta = playerOffchainData.GetEctoDeltaValue();
        m_BankEctoDeltaText.text = ectoDelta.ToString();
        m_BankEctoDeltaText.color = ectoDelta < 0 ? new Color32(245, 85, 93, 255) : new Color32(153, 230, 95, 255);

        int dustDelta = playerOffchainData.GetDustDeltaValue();
        m_GotchiDustCollectedText.text = dustDelta.ToString() + " x" + CodeInjector.Instance.GetOutputMultiplier();

        int bombDelta = playerOffchainData.GetBombDeltaValue();
        m_BombsUsedText.text = bombDelta.ToString();

        PlayerController playerController = player.GetComponent<PlayerController>();
        m_EnemiesSlainText.text = playerController.GetTotalKilledEnemies().ToString();
        m_DestructiblesSmashedText.text = playerController.GetTotalDestroyedDestructibles().ToString();

        PlayerDungeonTime playerDungeonTime = player.GetComponent<PlayerDungeonTime>();
        m_dungeonTimerText.text = playerDungeonTime.ToString();
    }

    private GameObject GetLocalPlayer()
    {
        PlayerController[] playerControllers = Game.Instance.playerControllers.ToArray();
        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController.IsLocalPlayer)
            {
                return playerController.gameObject;
            }
        }
        return null;
    }
}