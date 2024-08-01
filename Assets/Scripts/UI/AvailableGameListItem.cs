using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.Netcode;

public class AvailableGameListItem : MonoBehaviour
{
    public TextMeshProUGUI GameIdText;
    public TextMeshProUGUI PlayerCountText;
    public Button JoinButton;

    private bool m_isNetworkManagerShuttingDown = false;

    private void Awake()
    {
        JoinButton.onClick.AddListener(HandleClickJoinButton);
    }

    public void Init(string gameId, int playerCount)
    {
        GameIdText.text = gameId;
        PlayerCountText.text = playerCount.ToString() + "/3";
    }

    void HandleClickJoinButton()
    {
        Debug.Log("AvailableGameListItem: Join clicked. Shutting down server");
        m_isNetworkManagerShuttingDown = true;
        NetworkManager.Singleton.Shutdown();

    }

    private void Update()
    {
        if (!NetworkManager.Singleton.ShutdownInProgress && m_isNetworkManagerShuttingDown)
        {
            Debug.Log("AvailableGameListItem: Server shutdown, joining new gameID: " + GameIdText.text);
            m_isNetworkManagerShuttingDown = false;
            Game.Instance.JoinGameViaServerManager(GameIdText.text);

        }
    }
}
