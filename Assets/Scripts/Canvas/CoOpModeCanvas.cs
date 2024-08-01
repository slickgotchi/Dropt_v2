using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoOpModeCanvas : MonoBehaviour
{
    public GameObject AvailableGameListContent;
    public GameObject PrefabAvailableGameListItem;
    public TextMeshProUGUI MyGameIdText;

    private float k_updateInterval = 3f;
    private float m_updateTimer = 0f;

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
            MyGameIdText.text = "My Game ID: " + Bootstrap.Instance.GameId;
        }
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
            if (Bootstrap.Instance.GameId != game.gameId)
            {
                var newGameListItem = Instantiate(PrefabAvailableGameListItem, AvailableGameListContent.transform);
                newGameListItem.GetComponent<AvailableGameListItem>().Init(
                    game.gameId, game.numberPlayers);
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
