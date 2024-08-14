using System.Collections;
using System.Collections.Generic;
using Assets.Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Audio.Game;

public class TitleCanvas : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;

    private void Start()
    {
        // disable duplicate audio
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (audioListeners.Length > 1 && GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.GetComponent<AudioListener>().enabled = false;
        }


        playButton.onClick.AddListener(() =>
        {
            //SceneManager.LoadScene("Game");
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
    }
}
