using Assets.Plugins;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
            GoToSceneWithWipeDelay(isSkipTitle ? "Game" : "Title");
        });

        serverButton.onClick.AddListener(() =>
        {
            Bootstrap.Instance.NetworkRole = NetworkRole.Server;
            SetConnectionTypeFromDropDown();
            GoToSceneWithWipeDelay(isSkipTitle ? "Game" : "Title");
        });

        clientButton.onClick.AddListener(() =>
        {
            Bootstrap.Instance.NetworkRole = NetworkRole.Client;
            SetConnectionTypeFromDropDown();
            GoToSceneWithWipeDelay(isSkipTitle ? "Game" : "Title");
        });

        connectionDropdown.onValueChanged.AddListener(OnConnectionDropdownValueChanged);

        if (Defines.FAST_START)
        {
            hostButton.onClick.Invoke();
        }
    }

    async UniTaskVoid GoToSceneWithWipeDelay(string scene)
    {
        LoadingCanvas.Instance.WipeIn();
        await UniTask.Delay(500);
        SceneManager.LoadScene(scene);
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
