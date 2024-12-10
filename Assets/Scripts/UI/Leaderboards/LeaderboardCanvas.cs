using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public enum Tab { Adventure, Gauntlet, SpotPrizes}
    private Tab m_tab = Tab.Adventure;

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
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        m_adventureBoard.SetActive(false);
        m_gauntletBoard.SetActive(false);
        m_spotPrizesBoard.SetActive(false);

        m_adventureButton.GetComponent<Image>().color = m_buttonUnselectedColor;
        m_gauntletButton.GetComponent<Image>().color = m_buttonUnselectedColor;
        m_spotPrizesButton.GetComponent<Image>().color = m_buttonUnselectedColor;

        if (m_tab == Tab.Adventure)
        {
            m_adventureBoard.SetActive(true);
            m_adventureButton.GetComponent<Image>().color = m_buttonSelectedColor;
        }
        else if (m_tab == Tab.Gauntlet)
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

    void HandleClickAdventure() { m_tab = Tab.Adventure; }
    void HandleClickGauntlet() { m_tab = Tab.Gauntlet; }
    void HandleClickSpotPrizes() { m_tab = Tab.SpotPrizes; }

    void HandleClickExit()
    {
        LeaderboardCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }
}
