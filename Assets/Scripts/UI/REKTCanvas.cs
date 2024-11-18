using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class REKTCanvas : MonoBehaviour
{
    public static REKTCanvas Instance { get; private set; }

    public GameObject Container;
    public Button DegenapeButton;

    public enum TypeOfREKT { HP, Essence, InActive, Escaped }
    public TypeOfREKT Type = TypeOfREKT.HP;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI ReasonText;

    public Color EscapeTitleTextColor;
    public Color EscapeReasonTextColor;
    public Color REKTTitleTextColor;
    public Color REKTReasonTextColor;


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

        // set text colors
        if (type == TypeOfREKT.Escaped)
        {
            TitleText.text = "ESCAPED";
            TitleText.color = EscapeTitleTextColor;
            ReasonText.color = EscapeReasonTextColor;
        } else
        {
            TitleText.text = "REKT";
            TitleText.color = REKTTitleTextColor;
            ReasonText.color = REKTReasonTextColor;
        }

        // display text based on how we game over'd
        if (type == TypeOfREKT.HP)
        {
            ReasonText.text = "You ran out of HP... dungeons can be tough huh?";
        }
        else if (type == TypeOfREKT.Essence)
        {
            ReasonText.text = "You ran out of Essence... maybe catch a lil essence once in a while?";
        }
        else if (type == TypeOfREKT.Escaped)
        {
            ReasonText.text = "You successfully escaped with your collected treasures. Maybe a little deeper next time?";
        }
        else if (type == TypeOfREKT.InActive)
        {
            ReasonText.text = "You have been inactive for longer than " + PlayerController.InactiveTimerDuration.ToString("F0") + "s so... got the boot!";
        }

        DegenapeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return to Degenape Village";
    }

    void HandleClickDegenapeButton()
    {
        Container.SetActive(false);

        // reload the game scene
        SceneManager.LoadScene("Game");

        //Game.Instance.TryConnectClientOrHostGame();
    }
}
