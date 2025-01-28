using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;

public class Web3AuthCanvas : MonoBehaviour
{
    [Header("Sign In Button")]
    [SerializeField] private Button m_signInButton;
    [SerializeField] private Image m_signInImage;
    [SerializeField] private Color m_signInColor;
    [SerializeField] private Color m_connectedColor;
    [SerializeField] private TMPro.TextMeshProUGUI m_signInText;
    [SerializeField] private TMPro.TextMeshProUGUI m_balanceText;

    [Header("Left Panel Detals")]
    [SerializeField] private GameObject m_leftPanel;
    [SerializeField] private Color m_maticChainColor;
    [SerializeField] private Color m_amoyChainColor;
    [SerializeField] private Image m_chainIconImage;

    private int k_walletPollInterval_ms = 3000;

    System.Numerics.BigInteger m_chainId = 80002;

    IThirdwebWallet m_wallet;
    private string m_walletAddress = "";

    // Start is called before the first frame update
    void Start()
    {
        m_signInButton.onClick.AddListener(HandleClick_SignIn);

        PollWalletStatus().Forget();

        m_walletAddress = "";

        _ = PollWallet();
    }

    private void Update()
    {
        
    }

    private async UniTaskVoid PollWalletStatus()
    {
        while (gameObject != null)
        {
            await UniTask.Delay(k_walletPollInterval_ms);

            _ = PollWallet();
        }
    }

    private async UniTaskVoid PollWallet()
    {
        m_wallet = ThirdwebManager.Instance.GetActiveWallet();

        if (m_wallet != null)
        {
            m_walletAddress = await m_wallet.GetAddress();
            m_signInText.text = ShortenString(m_walletAddress);
            m_signInText.fontSize = 12;

            var walletBalance = await m_wallet.GetBalance(m_chainId);
            m_balanceText.gameObject.SetActive(true);
            m_balanceText.text = ((float)(walletBalance) / 1e18).ToString("F2") + " POL";


            m_signInImage.color = m_connectedColor;

            m_leftPanel.gameObject.SetActive(true);

            if (m_chainId == 137)
            {
                m_chainIconImage.color = m_maticChainColor;
            }
            else
            {
                m_chainIconImage.color = m_amoyChainColor;
            }
        }
        else
        {
            m_signInText.text = "Sign In";
            m_signInText.fontSize = 16;
            m_signInImage.color = m_signInColor;

            m_balanceText.gameObject.SetActive(false);

            m_leftPanel.gameObject.SetActive(false);
        }
    }

    void HandleClick_SignIn()
    {
        _ = HandleClick_SignIn_Async();
    }

    async UniTaskVoid HandleClick_SignIn_Async()
    {
        try
        {
            if (m_wallet == null)
            {
#if UNITY_WEBGL
                var newProvider = WalletProvider.MetaMaskWallet;
#else
                var newProvider = WalletProvider.WalletConnectWallet;
#endif
                Debug.Log($"Set provider: {newProvider.ToString()}");

                var walletOptions = new WalletOptions(provider: newProvider, chainId: m_chainId);

                m_wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void OnDestroy()
    {
        m_signInButton.onClick.RemoveAllListeners();
    }

    public static string ShortenString(string input)
    {
        if (input.Length <= 10) // If the string is too short, return it as is
            return input;

        string firstPart = input.Substring(0, 6);
        string lastPart = input.Substring(input.Length - 4);

        return firstPart + "..." + lastPart;
    }
}
