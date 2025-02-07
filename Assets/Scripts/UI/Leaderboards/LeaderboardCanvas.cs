using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;
using System;

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

    [SerializeField] private List<LeaderboardDataRow> m_adventureLeaderboardDataRows;
    [SerializeField] private List<LeaderboardDataRow> m_gauntletLeaderboardDataRows;

    private List<LeaderboardLogger.LeaderboardEntry> m_adventureLeaderboardEntries;
    private List<LeaderboardLogger.LeaderboardEntry> m_gauntletLeaderboardEntries;

    public enum Tab { Adventure, Gauntlet, SpotPrizes, Null }
    private Tab m_currentTab = Tab.Adventure;
    //private Tab m_previousTab = Tab.Null;

    private int m_pageNumber = 0;
    [SerializeField] private GameObject m_paginationPanel;
    [SerializeField] private TMPro.TextMeshProUGUI m_pageNumberText;
    [SerializeField] private Button m_pageLeftButton;
    [SerializeField] private Button m_pageRightButton;

    [Header("Set the End Date and Time in UTC")]
    [Tooltip("Enter the end date and time in UTC (YYYY-MM-DD HH:mm:ss)")]
    public string EndDateAndTimeUTC;

    [Header("Reference to the TextMeshPro Text")]
    [SerializeField] private TMPro.TextMeshProUGUI countdownText;
    private DateTime endDateTime;

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
        m_pageLeftButton.onClick.AddListener(HandlePageLeft);
        m_pageRightButton.onClick.AddListener(HandlePageRight);
    }

    public override void OnStart()
    {
        base.OnStart();

        // Parse the EndDateAndTimeUTC from the Inspector
        if (DateTime.TryParse(EndDateAndTimeUTC, out endDateTime))
        {
            // Ensure the entered date is in UTC format
            endDateTime = DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc);
        }
        else
        {
            Debug.LogError("Invalid EndDateAndTimeUTC format. Use 'YYYY-MM-DD HH:mm:ss'.");
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (isCanvasOpen) UpdateCountdown();
    }

    void HandlePageLeft()
    {
        m_pageNumber--;
        m_pageNumber = math.max(m_pageNumber, 0);

        SetLeaderboard(m_currentTab, m_pageNumber);

        m_pageNumberText.text = $"{m_pageNumber*100 + 1} - {(m_pageNumber+1) * 100}";
    }

    void HandlePageRight()
    {
        m_pageNumber++;

        SetLeaderboard(m_currentTab, m_pageNumber);

        m_pageNumberText.text = $"{m_pageNumber * 100 + 1} - {(m_pageNumber + 1) * 100}";
    }

    void SetNewTab(Tab newTab)
    {
        if (newTab == m_currentTab) return;
        m_currentTab = newTab;

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

    void HandleClickAdventure() {
        m_paginationPanel.SetActive(true);
        SetNewTab(Tab.Adventure);
        SetLeaderboard(Tab.Adventure, 0);
    }

    void HandleClickGauntlet() {
        m_paginationPanel.SetActive(true);
        SetNewTab(Tab.Gauntlet);
        SetLeaderboard(Tab.Gauntlet, 0);
    }

    void HandleClickSpotPrizes() {
        m_paginationPanel.SetActive(false);
        SetNewTab(Tab.SpotPrizes);
    }

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

    async UniTaskVoid PopulateLeaderboards()
    {
        try
        {
            // get local players leaderboard logger
            TryGetLocalPlayerInput();
            if (m_localPlayerInput == null)
            {
                Debug.Log("m_localPlayerInput = null... returning...");
                return;
            }

            // get adventure leaderboard entries
            m_adventureLeaderboardEntries = await LeaderboardLogger.GetAllLeaderboardEntries("adventure_leaderboard");
            if (m_adventureLeaderboardEntries == null)
            {
                Debug.Log("m_adventureLeaderboardEntries = null... returning...");
                return;
            }

            // get gauntlet leaderboard entries
            m_gauntletLeaderboardEntries = await LeaderboardLogger.GetAllLeaderboardEntries("gauntlet_leaderboard");
            if (m_gauntletLeaderboardEntries == null)
            {
                Debug.Log("m_gauntletLeaderboardEntries = null... returning...");
                return;
            }

            // populate the data rows
            SetLeaderboard(Tab.Adventure, m_pageNumber);
            SetLeaderboard(Tab.Gauntlet, m_pageNumber);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void SetLeaderboard(Tab boardName, int pageNumber)
    {
        m_pageNumber = math.max(0, pageNumber);

        if (boardName == Tab.Adventure)
        {
            for (int i = 0; i < m_adventureLeaderboardDataRows.Count; i++)
            {
                var dataRow = m_adventureLeaderboardDataRows[i];

                if (i + pageNumber*100 >= m_adventureLeaderboardEntries.Count)
                {
                    dataRow.SetActive(false);
                }
                else
                {
                    var entry = m_adventureLeaderboardEntries[i + pageNumber];
                    if (entry != null && dataRow != null)
                    {
                        var ghstPrize = (i + pageNumber >= m_adventurePrizes.Length) ? 0 : m_adventurePrizes[i + pageNumber];

                        dataRow.SetActive(true);
                        dataRow.Set(
                            i + 1 + pageNumber * 100, entry.gotchi_name, entry.gotchi_id, entry.wallet_address, ghstPrize, entry.formation, entry.dust_balance, entry.kills, entry.completion_time);
                    }
                }
            }
        }
        else if (boardName == Tab.Gauntlet)
        {
            for (int i = 0; i < m_gauntletLeaderboardDataRows.Count; i++)
            {
                var dataRow = m_gauntletLeaderboardDataRows[i];

                if (i + pageNumber*100 >= m_gauntletLeaderboardEntries.Count)
                {
                    dataRow.SetActive(false);
                }
                else
                {
                    var entry = m_gauntletLeaderboardEntries[i + pageNumber];
                    if (entry != null && dataRow != null)
                    {
                        var ghstPrize = (i + pageNumber >= m_gauntletPrizes.Length) ? 0 : m_gauntletPrizes[i + pageNumber];

                        dataRow.SetActive(true);
                        dataRow.Set(
                            i+1 + pageNumber*100, entry.gotchi_name, entry.gotchi_id, entry.wallet_address, ghstPrize, entry.formation, entry.dust_balance, entry.kills, entry.completion_time);
                    }
                }
            }
        }
    }

    private void UpdateCountdown()
    {
        // Calculate the remaining time
        TimeSpan remainingTime = endDateTime - DateTime.UtcNow;

        if (remainingTime.TotalSeconds > 0)
        {
            // Format as "XXh : XXm : XXs"
            countdownText.text = $"10 - 19th Jan  |  {remainingTime.Days:D2}d : {remainingTime.Hours:D2}h : {remainingTime.Minutes:D2}m : {remainingTime.Seconds:D2}s";
        }
        else
        {
            // Display "00h : 00m : 00s" if time is up
            countdownText.text = "00h : 00m : 00s";
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

    private int[] m_adventurePrizes = new int[] {
        200,    150,    100,    75,     50,
        35,     25,     15,     15,     15,
        10,     10,     10,     10,     10,
        10,     10,     10,     10,     10,
        10,     10,     10,     10,     10
    };

    private int[] m_gauntletPrizes = new int[]
    {
        200,    150,    100,    75,     50,
        35,     25,     15,     10,     10
    };
}
