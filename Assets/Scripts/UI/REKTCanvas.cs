using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class REKTCanvas : NetworkBehaviour
{
    public static REKTCanvas Instance { get; private set; }

    public GameObject Container;
    public Button DegenapeButton;

    public enum TypeOfREKT { HP, Essence }
    public TypeOfREKT Type = TypeOfREKT.HP;

    public TextMeshProUGUI REKTReasonText;

    private void Awake()
    {
        Instance = this;

        Container.SetActive(false);

        DegenapeButton.onClick.AddListener(HandleClickDegenapeButton);
    }

    public void Show(TypeOfREKT type)
    {
        Type = type;
        Container.SetActive(true);

        if (type == TypeOfREKT.HP)
        {
            REKTReasonText.text = "You ran out of HP... dungeons can be tough huh?";
        } else
        {
            REKTReasonText.text = "You ran out of Essence... maybe catch a lil essence once in a while?";
        }
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
