using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuCanvas : MonoBehaviour
{
    private static OptionsMenuCanvas _instance;
    public static OptionsMenuCanvas Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<OptionsMenuCanvas>() ?? new GameObject("OptionsMenuCanvas").AddComponent<OptionsMenuCanvas>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        Container.SetActive(false);

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

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
    public Button exitToTitleButton;

    private List<ResItem> resolutions = new List<ResItem> {
            new ResItem { width = 720, height = 450 },
            new ResItem { width = 960, height = 600 },
            new ResItem { width = 1920, height = 1080 },
            new ResItem { width = 1920, height = 1200 },
    };
    private ResItem currentResolution;



    // Start is called before the first frame update
    void Setup()
    {
        // Fetch available screen resolutions and remove duplicates
        resolutions = Screen.resolutions
            .Select(res => new ResItem { width = res.width, height = res.height })
            .Where(res =>
            {
                int gcd = GreatestCommonDivisor(res.width, res.height);
                float aspectRatio = (float)res.width / gcd / ((float)res.height / gcd);
                return aspectRatio == 16f / 9f || aspectRatio == 16f / 10f;
            })
            .Distinct(new ResolutionComparer())  // Use a custom comparer for distinct operation
            .ToList();

        // Load saved settings or use default values
        int defaultWidth = 960;
        int defaultHeight = 600;
        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
        bool savedFullscreen = PlayerPrefs.GetInt("FullscreenMode", 0) == 1;  // default to windowed mode if not set

        // Find the saved resolution in the available resolutions or use the default
        currentResolution = resolutions.FirstOrDefault(res => res.width == savedWidth && res.height == savedHeight);
        if (currentResolution.width == 0 && currentResolution.height == 0)
        {
            currentResolution = new ResItem { width = defaultWidth, height = defaultHeight };  // Set to default if not found
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

        // add all listeners
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        exitButton.onClick.AddListener(() => { Container.SetActive(false); });
        saveButton.onClick.AddListener(() => { Container.SetActive(false); });
        exitToTitleButton.onClick.AddListener(OnClickExitToTitleButton);

        // check for player prefs for slider volume
        CheckPlayerPrefs();

        musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    public void ShowMenu()
    {
        Container.SetActive(true);
        UpdateResolutionOptions();
    }

    private void CheckPlayerPrefs()
    {
        // Check and load volume settings from PlayerPrefs
        float musicVolume = PlayerPrefs.HasKey("musicVolume") ? PlayerPrefs.GetFloat("musicVolume") : 0.5f;
        musicVolumeSlider.value = musicVolume;

        float sfxVolume = PlayerPrefs.HasKey("sfxVolume") ? PlayerPrefs.GetFloat("sfxVolume") : 0.5f;
        sfxVolumeSlider.value = sfxVolume;
    }

    void OnResolutionDropdownChanged(int selectedIndex)
    {
        ResItem selectedResolution = resolutions[selectedIndex];
        var fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullScreenMode);

        PlayerPrefs.SetInt("ResolutionWidth", selectedResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", selectedResolution.height);
        PlayerPrefs.Save();  // Save the preferences

        currentResolution = selectedResolution;
    }




    void OnFullscreenToggle(bool isOn)
    {
        isFullscreen = isOn;
        Screen.SetResolution(currentResolution.width, currentResolution.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        // Save the fullscreen mode to PlayerPrefs
        PlayerPrefs.SetInt("FullscreenMode", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();  // Make sure to save PlayerPrefs changes
    }

    public void UpdateResolutionOptions()
    {
        resolutions = Screen.resolutions
            .Select(res => new ResItem { width = res.width, height = res.height })
            .Where(res =>
            {
                int gcd = GreatestCommonDivisor(res.width, res.height);
                float aspectRatio = (float)res.width / gcd / ((float)res.height / gcd);
                return aspectRatio == 16f / 9f || aspectRatio == 16f / 10f;
            })
            .Distinct(new ResolutionComparer())
            .ToList();

        resolutionDropdown.ClearOptions();
        List<string> resolutionStrings = resolutions.Select(res => $"{res.width} x {res.height}").ToList();
        resolutionDropdown.AddOptions(resolutionStrings);

        // Optionally reset the selected index or try to find and set the current resolution
        int currentResolutionIndex = resolutions.FindIndex(res => res.width == Screen.width && res.height == Screen.height);
        if (currentResolutionIndex != -1)
            resolutionDropdown.value = currentResolutionIndex;
        else
            resolutionDropdown.value = 0;  // Default to first resolution or handle appropriately
        resolutionDropdown.RefreshShownValue();
    }


    void OnMusicSliderChanged(float value)
    {
    }

    void OnSfxSliderChanged(float value)
    {
    }

    void Update()
    {
        //var currScene = Loader.GetCurrentScene();
        //if (currScene == Loader.Scene.Title)
        //{
        //    exitToTitleButton.gameObject.SetActive(false);
        //}
        //else
        //{
        //    exitToTitleButton.gameObject.SetActive(true);
        //}

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Container.SetActive(!Container.gameObject.activeSelf);
            // update res options if on another screen
            if (Container.activeSelf)
            {
                UpdateResolutionOptions();
            }
        }

    }

    public void OnClickExitToTitleButton()
    {
        Container.SetActive(false);

    }

    int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}

public struct ResItem
{
    public int width;
    public int height;
    //public int refreshRate; // Optional: Store refresh rate
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

