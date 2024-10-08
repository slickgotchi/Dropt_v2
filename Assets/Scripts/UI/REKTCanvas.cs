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

    public enum TypeOfREKT { HP, Essence, InActive, Escaped }
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

        // display text based on how we game over'd
        if (type == TypeOfREKT.HP)
        {
            REKTReasonText.text = "You ran out of HP... dungeons can be tough huh?";
        }
        else if (type == TypeOfREKT.Essence)
        {
            REKTReasonText.text = "You ran out of Essence... maybe catch a lil essence once in a while?";
        }
        else if (type == TypeOfREKT.Escaped)
        {
            REKTReasonText.text = "You successfully escaped with your collected treasures. Maybe a little deeper next time?";
        }
        else
        {
            REKTReasonText.text = "You have been inactive for longer than " + PlayerController.InactiveTimerDuration.ToString("F0") + "s so... got the boot!";
        }

        DegenapeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return to Degenape Village";
    }

    void HandleClickDegenapeButton()
    {
        ProgressBarCanvas.Instance.ResetProgress();

        Container.SetActive(false);

        if (Bootstrap.IsHost())
        {
            Game.Instance.ConnectHostGame();
        }
    }
}
