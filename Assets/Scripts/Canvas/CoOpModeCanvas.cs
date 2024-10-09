using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System.ComponentModel;

public class CoOpModeCanvas : DroptCanvas
{
    public GameObject AvailableGameListContent;
    public GameObject PrefabAvailableGameListItem;

    public Button CopyMyGameIdButton;
    public Button JoinPrivateButton;
    public TMP_InputField JoinPrivateInput;
    public Toggle IsPublicToggle;

    private TextMeshProUGUI m_copyButtonText;

    public GameObject MenuCard;

    private float k_updateInterval = 2f;
    private float m_updateTimer = 0f;

    private void Awake()
    {
        CopyMyGameIdButton.onClick.AddListener(HandleClick_CopyMyGameIdButton);
        JoinPrivateButton.onClick.AddListener(HandleClick_JoinPrivateButton);
        IsPublicToggle.onValueChanged.AddListener(HandleChange_IsPublicToggle);

        m_copyButtonText = CopyMyGameIdButton.GetComponentInChildren<TextMeshProUGUI>();
        MenuCard.SetActive(false);

        HideCanvas();
    }

    protected override void Update()
    {
        m_updateTimer -= Time.deltaTime;

        if (m_updateTimer <= 0)
        {
            UpdateAvailableGamesList();
            m_updateTimer = k_updateInterval; // Reset the timer
        }

        if (Bootstrap.Instance.GameId != null)
        {
            m_copyButtonText.text = Bootstrap.Instance.GameId.ToString();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            MenuCard.SetActive(!MenuCard.activeSelf);
        }

        if (LevelManager.Instance.IsDegenapeVillage())
        {
            ShowCanvas();
        }
        else
        {
            HideCanvas();
        }
    }

    void HandleClick_CopyMyGameIdButton()
    {
        GUIUtility.systemCopyBuffer = m_copyButtonText.text;
    }

    void HandleClick_JoinPrivateButton()
    {
        //Debug.Log("CoOpModeCanvas.cs - Join private gameId: " + JoinPrivateInput.text);
        //Game.Instance.TryJoinGame(JoinPrivateInput.text);
    }

    void HandleChange_IsPublicToggle(bool isOn)
    {
        //Debug.Log("Game.Instance.SetIsPublic to " + isOn);
        NetworkMessenger.Instance.SetGameIsPublic(isOn);
    }

    void UpdateAvailableGamesList()
    {
        // Clear and destroy existing children
        ClearAvailableGameListContent();

        // don't proceed if we don't have available games
        if (AvailableGamesHeartbeat.Instance == null) return;
        if (AvailableGamesHeartbeat.Instance.AvailableGames.Count <= 0) return;

        // Add new items to the list (example logic)
        foreach (var game in AvailableGamesHeartbeat.Instance.AvailableGames)
        {
            if (game.isPublic)
            {
                // Instantiate new game item and add to the AvailableGameListContent
                var availableGameListItem = Instantiate(PrefabAvailableGameListItem, AvailableGameListContent.transform).GetComponent<AvailableGameListItem>();
                availableGameListItem.Init(game.gameId, game.numberPlayers);

                // Change join button if its ours
                if (Bootstrap.Instance.GameId == game.gameId)
                {
                    Color color;
                    ColorUtility.TryParseHtmlString("#D3FC7E", out color);
                    availableGameListItem.JoinButton.GetComponent<Image>().color = color;
                    availableGameListItem.JoinButton.GetComponent<Button>().enabled = false;
                    availableGameListItem.JoinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Mine";
                }
            }
        }
    }

    void ClearAvailableGameListContent()
    {
        foreach (Transform child in AvailableGameListContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
