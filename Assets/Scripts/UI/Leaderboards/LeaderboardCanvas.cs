using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardCanvas : DroptCanvas
{
    public static LeaderboardCanvas Instance { get; private set; }

    public Interactable interactable;

    [SerializeField] private Color m_buttonSelectedColor;
    [SerializeField] private Color m_buttonUnselectedColor;

    [SerializeField] private GameObject m_adventureBoard;
    [SerializeField] private GameObject m_gauntletBoard;
    [SerializeField] private GameObject m_spotPrizesBoard;

    [SerializeField] private Button m_adventureButton;
    [SerializeField] private Button m_gauntletButton;
    [SerializeField] private Button m_spotPrizesButton;
    [SerializeField] private Button m_exitButton;

    private List<LeaderboardDataRow> m_leaderboardDataRows;

    public enum Tab { Adventure, Gauntlet, SpotPrizes, Null }
    private Tab m_currentTab = Tab.Adventure;
    private Tab m_previousTab = Tab.Null;

    private void Awake()
    {
        // Singleton pattern 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InstaHideCanvas();

        // setup buttons
        m_adventureButton.onClick.AddListener(HandleClickAdventure);
        m_gauntletButton.onClick.AddListener(HandleClickGauntlet);
        m_spotPrizesButton.onClick.AddListener(HandleClickSpotPrizes);
        m_exitButton.onClick.AddListener(HandleClickExit);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (m_currentTab != m_previousTab)
        {
            m_previousTab = m_currentTab;
            SetNewTab(m_currentTab);
            Debug.Log("Set new tab");
        }
    }

    void SetNewTab(Tab newTab)
    {
        m_adventureBoard.SetActive(false);
        m_gauntletBoard.SetActive(false);
        m_spotPrizesBoard.SetActive(false);

        m_adventureButton.GetComponent<Image>().color = m_buttonUnselectedColor;
        m_gauntletButton.GetComponent<Image>().color = m_buttonUnselectedColor;
        m_spotPrizesButton.GetComponent<Image>().color = m_buttonUnselectedColor;

        if (newTab == Tab.Adventure)
        {
            m_adventureBoard.SetActive(true);
            m_adventureButton.GetComponent<Image>().color = m_buttonSelectedColor;
        }
        else if (newTab == Tab.Gauntlet)
        {
            m_gauntletBoard.SetActive(true);
            m_gauntletButton.GetComponent<Image>().color = m_buttonSelectedColor;
        }
        else
        {
            m_spotPrizesBoard.SetActive(true);
            m_spotPrizesButton.GetComponent<Image>().color = m_buttonSelectedColor;
        }
    }

    void HandleClickAdventure() { m_currentTab = Tab.Adventure; }
    void HandleClickGauntlet() { m_currentTab = Tab.Gauntlet; }
    void HandleClickSpotPrizes() { m_currentTab = Tab.SpotPrizes; }

    void HandleClickExit()
    {
        LeaderboardCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();

        PopulateLeaderboards();
    }

    async void PopulateLeaderboards()
    {
        try
        {
            // get local players leaderboard logger
            TryGetLocalPlayerInput();
            if (m_localPlayerInput == null) return;
            var playerLeaderboardLogger = m_localPlayerInput.GetComponent<PlayerLeaderboardLogger>();
            if (playerLeaderboardLogger == null) return;

            // get leaderboard entries
            var entries = await playerLeaderboardLogger.GetAllLeaderboardEntries("adventure_leaderboard");

            // populate the data rows
            for (int i = 0; i < m_leaderboardDataRows.Count; i++)
            {
                var entry = entries[i];
                var dataRow = m_leaderboardDataRows[i];
                if (entry != null && dataRow != null)
                {
                    dataRow.Set(
                        i, entry.gotchi_name, entry.gotchi_id, entry.wallet_address, 0, entry.formation, entry.dust_balance, entry.kills, entry.completion_time);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    struct LeaderboardRowEntry
    {
        public int rank;
        public string gotchi;
        public int id;
        public string address;
        public int ghst;
        public string formation;
        public int dust;
        public int kills;
        public int time;
    }
}
