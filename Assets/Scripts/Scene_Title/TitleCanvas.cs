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

    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
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
