using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class REKTCanvas : NetworkBehaviour
{
    public static REKTCanvas Instance { get; private set; }

    public GameObject Container;
    public Button DegenapeButton;

    private void Awake()
    {
        Instance = this;

        Container.SetActive(false);

        DegenapeButton.onClick.AddListener(HandleClickDegenapeButton);
    }

    void HandleClickDegenapeButton()
    {
        GoToDegenapeServerRpc();

        // get the local player and renable their input
        var players = GameObject.FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                player.IsInputDisabled = false;
            }
        }
    }

    [Rpc(SendTo.Server)]
    void GoToDegenapeServerRpc()
    {
        LevelManager.Instance.GoToDegenapeVillageLevel();
    }
}
