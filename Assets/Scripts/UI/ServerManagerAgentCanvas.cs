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
    public Button LeaveExistingButton;
    public TMPro.TMP_InputField GameIdInput;

    private void Awake()
    {
        GetEmptyButton.onClick.AddListener(async () => await handleClick_GetEmptyButton());
        GetExistingButton.onClick.AddListener(async () => await handleClick_GetExistingButton());
        LeaveExistingButton.onClick.AddListener(async () => await handleClick_LeaveExistingButton());
    }

    async UniTask handleClick_GetEmptyButton()
    {
        await ServerManagerAgent.Instance.GetEmpty();
    }

    async UniTask handleClick_GetExistingButton()
    {
        string gameId = GameIdInput.text;

        await ServerManagerAgent.Instance.GetExisting(gameId);
    }

    async UniTask handleClick_LeaveExistingButton()
    {
        string gameId = GameIdInput.text;

        await ServerManagerAgent.Instance.LeaveExisting(gameId);
    }
}
