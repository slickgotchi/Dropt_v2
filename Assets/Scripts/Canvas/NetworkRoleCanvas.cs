using System.Collections;
using System.Collections.Generic;
using Assets.Plugins;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoleCanvas : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_Dropdown connectionDropdown;

    public bool isSkipTitle = true;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            Bootstrap.Instance.NetworkRole = NetworkRole.Host;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene(isSkipTitle ? "Game" : "Title");
        });

        serverButton.onClick.AddListener(() =>
        {
            Bootstrap.Instance.NetworkRole = NetworkRole.Server;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene(isSkipTitle ? "Game" : "Title");
        });

        clientButton.onClick.AddListener(() =>
        {
            Bootstrap.Instance.NetworkRole = NetworkRole.Client;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene(isSkipTitle ? "Game" : "Title");
        });

        connectionDropdown.onValueChanged.AddListener(OnConnectionDropdownValueChanged);

        if (Defines.FAST_START)
        {
            hostButton.onClick.Invoke();
        }
    }

    private void Start()
    {
        isSkipTitle = Bootstrap.Instance.AutoPlay;
        // set initial drop down value
        connectionDropdown.value = (int)Bootstrap.Instance.ConnectionType;
    }

    private void OnConnectionDropdownValueChanged(int value)
    {
        if (value == 0) Bootstrap.Instance.ConnectionType = ConnectionType.Local;
        if (value == 1) Bootstrap.Instance.ConnectionType = ConnectionType.Remote;

    }

    void SetConnectionTypeFromDropDown()
    {
        Bootstrap.Instance.ConnectionType = connectionDropdown.value == 0 ? ConnectionType.Local : ConnectionType.Remote;
    }
}
