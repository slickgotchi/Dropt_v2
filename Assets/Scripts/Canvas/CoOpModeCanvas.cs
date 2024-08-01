using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class CoOpModeCanvas : MonoBehaviour
{
    public GameObject AvailableGameListContent;
    public GameObject PrefabAvailableGameListItem;

    public Button CopyMyGameIdButton;
    public Button JoinPrivateButton;
    public TMP_InputField JoinPrivateInput;
    public Toggle IsPublicToggle;

    private TextMeshProUGUI m_copyButtonText;

    private float k_updateInterval = 3f;
    private float m_updateTimer = 0f;

    private void Awake()
    {
        CopyMyGameIdButton.onClick.AddListener(HandleClick_CopyMyGameIdButton);
        JoinPrivateButton.onClick.AddListener(HandleClick_JoinPrivateButton);

        m_copyButtonText = CopyMyGameIdButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
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
    }

    void HandleClick_CopyMyGameIdButton()
    {
        GUIUtility.systemCopyBuffer = m_copyButtonText.text;
    }

    void HandleClick_JoinPrivateButton()
    {
        Debug.Log("CoOpModeCanvas.cs - Join private gameId: " + JoinPrivateInput.text);
        Game.Instance.TryJoinGame(JoinPrivateInput.text);
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
            // Instantiate new game item and add to the AvailableGameListContent
            var newGameListItem = Instantiate(PrefabAvailableGameListItem, AvailableGameListContent.transform);
            newGameListItem.GetComponent<AvailableGameListItem>().Init(
                game.gameId, game.numberPlayers);

            // Remove join button if this is our game
            if (Bootstrap.Instance.GameId == game.gameId)
            {
                Destroy(newGameListItem.GetComponent<AvailableGameListItem>().JoinButton.gameObject);
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
