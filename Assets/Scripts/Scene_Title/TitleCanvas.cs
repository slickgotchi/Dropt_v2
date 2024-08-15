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

        playButton.onClick.AddListener(() =>
        {
            Debug.Log("Load Game");
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
    }
}
