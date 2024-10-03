using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class ServerManagerAgentCanvas : MonoBehaviour
{
    public Button JoinEmptyButton;
    public Button JoinExistingButton;
    public Button LeaveExistingButton;
    public TMPro.TMP_InputField GameIdInput;

    private void Awake()
    {
        JoinEmptyButton.onClick.AddListener(async () => await handleClick_JoinEmptyButton());
        JoinExistingButton.onClick.AddListener(async () => await handleClick_JoinExistingButton());
        LeaveExistingButton.onClick.AddListener(async () => await handleClick_LeaveExistingButton());
    }

    async UniTask handleClick_JoinEmptyButton()
    {
        await ServerManagerAgent.Instance.JoinEmpty();
    }

    async UniTask handleClick_JoinExistingButton()
    {
        string gameId = GameIdInput.text;

        await ServerManagerAgent.Instance.JoinExisting(gameId);
    }

    async UniTask handleClick_LeaveExistingButton()
    {
        string gameId = GameIdInput.text;

        await ServerManagerAgent.Instance.LeaveExisting(gameId);
    }
}
