using System.Collections;
using System.Collections.Generic;
using Assets.Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class TitleCanvas : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private TMPro.TMP_Dropdown regionDropdown;
    [SerializeField] private CanvasGroup m_fadeOutCanvasGroup;
    [SerializeField] private float m_fadeOutDuration = 1f;

    private Tweener m_fadeTween;

    private const string ServerRegionKey = "ServerRegion";

    private void Start()
    {
        // preset our dropdown if we have a player pref
        var playerPrefRegion = PlayerPrefs.GetString(ServerRegionKey).ToUpper();
        int regionIndex = regionDropdown.options.FindIndex(option => option.text.Equals(playerPrefRegion, System.StringComparison.OrdinalIgnoreCase));
        if (regionIndex >= 0)
        {
            regionDropdown.value = regionIndex;
            SetRegion(regionIndex);
        }

        // play button listener
        playButton.onClick.AddListener(() =>
        {
            //LoadingCanvas.Instance.InstaBlack();
            LoadingCanvas.Instance.WipeIn();
            SceneManager.LoadScene("Game");
        });

        optionsButton.onClick.AddListener(() =>
        {
            OptionsMenuCanvas.Instance.ShowMenu();
        });

        if (Defines.FAST_START)
        {
            playButton.onClick.Invoke();
        }

        if (Bootstrap.Instance.AutoPlay)
        {
            playButton.onClick.Invoke();
        }

        regionDropdown.onValueChanged.AddListener(Handle_DropdownChange);

        if (m_fadeOutCanvasGroup != null)
        {
            m_fadeOutCanvasGroup.alpha = 1;
            m_fadeTween = m_fadeOutCanvasGroup.DOFade(0, m_fadeOutDuration).SetEase(Ease.InOutQuad);
        }
    }

    private void Handle_DropdownChange(int index)
    {
        SetRegion(index);
    }

    void SetRegion(int index)
    {
        var selectedRegion = regionDropdown.options[index].text.ToUpper();
        switch (selectedRegion)
        {
            case "AMERICA":
                Bootstrap.Instance.region = Bootstrap.Region.America;
                break;
            case "EUROPE":
                Bootstrap.Instance.region = Bootstrap.Region.Europe;
                break;
            case "ASIA":
                Bootstrap.Instance.region = Bootstrap.Region.Asia;
                break;
            default:
                break;
        }

        PlayerPrefs.SetString(ServerRegionKey, selectedRegion.ToUpper());
    }

    private void OnDestroy()
    {
        if (m_fadeTween != null && m_fadeTween.IsActive())
        {
            m_fadeTween.Kill();
        }
    }
}
