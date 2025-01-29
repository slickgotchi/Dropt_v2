using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;

public class Web3AuthCanvas : MonoBehaviour
{
    public static Web3AuthCanvas Instance { get; private set; }

    [Header("Sign In Button")]
    [SerializeField] private Button m_signInButton;
    [SerializeField] private Image m_signInImage;
    [SerializeField] private Color m_signInColor;
    [SerializeField] private Color m_connectedColor;
    [SerializeField] private TMPro.TextMeshProUGUI m_signInText;
    [SerializeField] private TMPro.TextMeshProUGUI m_ghstBalanceText;

    [Header("Left Panel Detals")]
    [SerializeField] private GameObject m_leftPanel;
    [SerializeField] private Color m_maticChainColor;
    [SerializeField] private Color m_amoyChainColor;
    [SerializeField] private Image m_chainIconImage;

    private int k_walletPollInterval_ms = 3000;

    [HideInInspector] public System.Numerics.BigInteger ChainId = 80002;

    [HideInInspector] public DroptContractAddresses Contracts;
    [HideInInspector] public DroptABIs ABIs;

    IThirdwebWallet m_wallet;
    private string m_walletAddress = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IThirdwebWallet GetActiveWallet() { return m_wallet; }
    public string GetActiveWalletAddress() { return m_walletAddress; }

    // Start is called before the first frame update
    void Start()
    {
        m_signInButton.onClick.AddListener(HandleClick_SignIn);

        SetContractAddresses((int)ChainId);
        SetABIs();

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

            var walletBalance = await m_wallet.GetBalance(ChainId, Contracts.ghst);
            m_ghstBalanceText.gameObject.SetActive(true);
            m_ghstBalanceText.text = ((float)(walletBalance) / 1e18).ToString("F2") + " GHST";


            m_signInImage.color = m_connectedColor;

            m_leftPanel.gameObject.SetActive(true);

            if (ChainId == 137)
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

            m_ghstBalanceText.gameObject.SetActive(false);

            m_leftPanel.gameObject.SetActive(false);
        }
    }

    public void SignIn()
    {
        HandleClick_SignIn();
    }

    void HandleClick_SignIn()
    {
        _ = HandleClick_SignIn_Async();
    }

    async UniTaskVoid HandleClick_SignIn_Async()
    {
        var existingWallet = ThirdwebManager.Instance.GetActiveWallet();
        if (existingWallet != null)
        {
            await existingWallet.Disconnect();
        }

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

                var walletOptions = new WalletOptions(provider: newProvider, chainId: ChainId);

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

    void SetContractAddresses(int chainId)
    {
        switch (chainId)
        {
            case 80002: // amoy
                Contracts.droptPaymentProcessor = "0x32EFD2fBb0a43eE1918621D0544EFbd2c7F77beE";
                Contracts.ghst = "0xc05bD0a668119Fc0113572c057e5Fee2Da85d5bf";
                Contracts.essence = "0x98ddfDD9a7f121263c28543F417b2E13aBa11896";
                break;
            case 137:   // polygon
                Contracts.droptPaymentProcessor = "0xc9e2ce71F51c9dcc5E0C6142Df72A38024b5A382";
                Contracts.ghst = "0x385eeac5cb85a38a9a07a70c73e0a3271cfb54a7";
                Contracts.essence = "0x4fDfc1B53Fd1D80d969C984ba7a8CE4c7bAaD442";
                break;
            case 631571:    // polter
                Contracts.droptPaymentProcessor = "0x37086AA423cE1048047B955dfd31b2DDee949368";
                Contracts.ghst = "";
                Contracts.essence = "0xbbF58eb4846aa5ffD06C69Cbe23114B1ec196069";
                break;
            case 63157: // geist
                Contracts.droptPaymentProcessor = "";
                Contracts.ghst = "";
                Contracts.essence = "";
                break;
            default: break;
        }
    }

    void SetABIs()
    {
        TextAsset abiFile = Resources.Load<TextAsset>("DroptPaymentProcessorV1-abi");
        if (abiFile == null)
        {
            Debug.LogError("DroptPaymentProcessorV1-abi.json file not found in Resources folder.");
            return;
        }

        ABIs.paymentProcessor = abiFile.text;

        abiFile = Resources.Load<TextAsset>("GHST-abi");
        if (abiFile == null)
        {
            Debug.LogError("GHST-abi.json file not found in Resources folder.");
            return;
        }

        ABIs.ghst = abiFile.text;

        abiFile = Resources.Load<TextAsset>("Essence-abi");
        if (abiFile == null)
        {
            Debug.LogError("Essence-abi.json file not found in Resources folder.");
            return;
        }

        ABIs.essence = abiFile.text;
    }

    public struct DroptContractAddresses
    {
        public string droptPaymentProcessor;
        public string ghst;
        public string essence;
    }

    public struct DroptABIs
    {
        public string paymentProcessor;
        public string ghst;
        public string essence;
    }
}
