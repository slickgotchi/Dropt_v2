using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class OptionsMenuCanvas : MonoBehaviour
{
    public static OptionsMenuCanvas Instance { get; private set; }

    void Awake()
    {
        Container.SetActive(false);

        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Setup();
    }

    [SerializeField] private GameObject Container;

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private bool isFullscreen = false;

    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button saveButton;
    public Button exitButton;
    public Button exitToVillageButton;

    public TMPro.TextMeshProUGUI exitToVillageNote;

    // Predefined 16:9 and 16:10 resolutions
    private List<ResItem> resolutions = new List<ResItem> {
        // 16:9 Resolutions
        new ResItem { width = 1920, height = 1080 },
        new ResItem { width = 1600, height = 900 },
        new ResItem { width = 1280, height = 720 },

        // 16:10 Resolutions
        new ResItem { width = 1920, height = 1200 },
        new ResItem { width = 1680, height = 1050 },
        new ResItem { width = 1440, height = 900 },
        new ResItem { width = 960, height = 600 }
    };
    private ResItem currentResolution;

    void Setup()
    {
        // Load saved settings or use default values
        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", 1920);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", 1080);
        bool savedFullscreen = PlayerPrefs.GetInt("FullscreenMode", 0) == 1;  // default to windowed mode if not set

        // Find the saved resolution in the predefined resolutions or use the default
        currentResolution = resolutions.FirstOrDefault(res => res.width == savedWidth && res.height == savedHeight);
        if (currentResolution.width == 0 && currentResolution.height == 0)
        {
            currentResolution = new ResItem { width = savedWidth, height = savedHeight };  // Set to default if not found
        }

        // Populate and set the resolution dropdown
        resolutionDropdown.ClearOptions();
        List<string> resolutionStrings = resolutions.Select(res => $"{res.width} x {res.height}").ToList();
        resolutionDropdown.AddOptions(resolutionStrings);
        resolutionDropdown.value = resolutions.FindIndex(res => res.width == savedWidth && res.height == savedHeight);
        resolutionDropdown.RefreshShownValue();

        // Set the fullscreen toggle and apply the resolution and fullscreen settings
        fullscreenToggle.isOn = savedFullscreen;
        OnFullscreenToggle(savedFullscreen);  // Apply the loaded fullscreen setting
        OnResolutionDropdownChanged(resolutionDropdown.value); // Apply the loaded resolution setting

        // Add all listeners
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        exitButton.onClick.AddListener(() => { Container.SetActive(false); });
        saveButton.onClick.AddListener(() => { Container.SetActive(false); });
        exitToVillageButton.onClick.AddListener(OnClickExitToVillageButton);

        // Check for player prefs for slider volume
        SetupAudioPrefs();

        musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    public void ShowMenu()
    {
        Container.SetActive(true);
    }

    private void SetupAudioPrefs()
    {
        // Check and load volume settings from PlayerPrefs
        float musicVolume = PlayerPrefs.HasKey("musicVolume") ? PlayerPrefs.GetFloat("musicVolume") : 0.5f;
        musicVolumeSlider.value = musicVolume;
        AudioManager.Instance.MasterMusicVolume = musicVolume;

        float sfxVolume = PlayerPrefs.HasKey("sfxVolume") ? PlayerPrefs.GetFloat("sfxVolume") : 0.5f;
        sfxVolumeSlider.value = sfxVolume;
        AudioManager.Instance.MasterSFXVolume = sfxVolume;
    }

    void OnResolutionDropdownChanged(int selectedIndex)
    {
        ResItem selectedResolution = resolutions[selectedIndex];
        var fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

#if !UNITY_WEBGL
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullScreenMode);
#endif

        PlayerPrefs.SetInt("ResolutionWidth", selectedResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", selectedResolution.height);
        PlayerPrefs.Save();  // Save the preference

        currentResolution = selectedResolution;
    }

    void OnFullscreenToggle(bool isOn)
    {
        isFullscreen = isOn;

#if !UNITY_WEBGL
        Screen.SetResolution(currentResolution.width, currentResolution.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
#endif

        // Save the fullscreen mode to PlayerPrefs
        PlayerPrefs.SetInt("FullscreenMode", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();  // Make sure to save PlayerPrefs changes
    }

    void OnMusicSliderChanged(float value)
    {
        // Handle music volume changes here
        AudioManager.Instance.MasterMusicVolume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    void OnSfxSliderChanged(float value)
    {
        // Handle sfx volume changes here
        AudioManager.Instance.MasterSFXVolume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Container.SetActive(!Container.gameObject.activeSelf);
        }

        // if the containers active, we need to set if the exit to village button is visible
        if (Container.activeSelf)
        {
            if (Game.Instance != null && LevelManager.Instance != null && !LevelManager.Instance.IsDegenapeVillage())
            {
                exitToVillageButton.gameObject.SetActive(true);
                exitToVillageNote.gameObject.SetActive(true);
            }
            else
            {
                exitToVillageButton.gameObject.SetActive(false);
                exitToVillageNote.gameObject.SetActive(false);
            }
        }
    }

    public void OnClickExitToVillageButton()
    {
        ExitGoToVillage_ASYNC();
    }

    async UniTask ExitGoToVillage_ASYNC()
    {
        LoadingCanvas.Instance.WipeIn();

        await UniTask.Delay(500);

        NetworkManager.Singleton.Shutdown();
        Container.SetActive(false);
        Bootstrap.Instance.GameId = "";
        SceneManager.LoadScene("Game");
    }
}

public struct ResItem
{
    public int width;
    public int height;
}

class ResolutionComparer : IEqualityComparer<ResItem>
{
    public bool Equals(ResItem x, ResItem y)
    {
        // Direct comparison of width and height, no null checks needed for structs
        return x.width == y.width && x.height == y.height;
    }

    public int GetHashCode(ResItem obj)
    {
        // Combine the hash codes for the width and height using XOR
        int hashWidth = obj.width.GetHashCode();
        int hashHeight = obj.height.GetHashCode();
        return hashWidth ^ hashHeight;
    }
}
