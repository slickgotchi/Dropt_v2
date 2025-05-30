using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class ServerManagerAgentCanvas : MonoBehaviour
{
    public Button GetEmptyButton;
    public Button GetExistingButton;
    public TMP_InputField GameIdInput;
    public TMP_Dropdown RegionDropdown;

    private void Awake()
    {
        GetEmptyButton.onClick.AddListener(async () => await handleClick_GetEmptyButton());
        GetExistingButton.onClick.AddListener(async () => await handleClick_GetExistingButton());
    }

    async UniTask handleClick_GetEmptyButton()
    {
        var region = RegionDropdown.options[RegionDropdown.value].text;
        //await ServerManagerAgent.Instance.GetEmpty(region);
    }

    async UniTask handleClick_GetExistingButton()
    {
        string gameId = GameIdInput.text;

        //await ServerManagerAgent.Instance.GetExisting(gameId);
    }
}
