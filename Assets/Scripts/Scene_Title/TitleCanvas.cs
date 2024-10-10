using System.Collections;
using System.Collections.Generic;
using Assets.Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleCanvas : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private TMPro.TMP_Dropdown regionDropdown;

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
            //LoadingCanvas.Instance.Animator.Play("LoadingCanvas_Blackout");
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
}
