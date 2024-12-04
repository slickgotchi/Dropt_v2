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

    private void Awake()
    {
        JoinButton.onClick.AddListener(HandleClick_JoinButton);
    }

    public void Init(string gameId, int playerCount)
    {
        GameIdText.text = gameId;
        PlayerCountText.text = playerCount.ToString() + "/3";
    }

    void HandleClick_JoinButton()
    {
        Game.Instance.ConnectClientGame(GameIdText.text);
    }
}
