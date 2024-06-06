using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            Bootstrap.Singleton.NetworkRole = NetworkRole.Host;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene("Game");
            //SceneManager.LoadScene("Title");
        });

        serverButton.onClick.AddListener(() =>
        {
            Bootstrap.Singleton.NetworkRole = NetworkRole.Server;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene("Game");
            //SceneManager.LoadScene("Title");
        });

        clientButton.onClick.AddListener(() =>
        {
            Bootstrap.Singleton.NetworkRole = NetworkRole.Client;
            SetConnectionTypeFromDropDown();
            SceneManager.LoadScene("Game");
            //SceneManager.LoadScene("Title");
        });

        connectionDropdown.onValueChanged.AddListener(OnConnectionDropdownValueChanged);

    }

    private void Start()
    {
        // set initial drop down value
        connectionDropdown.value = (int)Bootstrap.Singleton.ConnectionType;
    }

    private void OnConnectionDropdownValueChanged(int value)
    {
        if (value == 0) Bootstrap.Singleton.ConnectionType = ConnectionType.Local;
        if (value == 1) Bootstrap.Singleton.ConnectionType = ConnectionType.Remote;

    }

    void SetConnectionTypeFromDropDown()
    {
        Bootstrap.Singleton.ConnectionType = connectionDropdown.value == 0 ? ConnectionType.Local : ConnectionType.Remote;
    }
}
