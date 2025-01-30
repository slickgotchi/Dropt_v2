using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;
using Unity.Netcode;
using System;
using UnityEngine.Networking;

public class Web3AuthCanvas : NetworkBehaviour
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


    [HideInInspector] public System.Numerics.BigInteger ChainId = 80002;

    [HideInInspector] public DroptContractAddresses Contracts;
    [HideInInspector] public DroptABIs ABIs;

    IThirdwebWallet m_wallet;
    private int k_walletPollInterval_ms = 2000;
    private string m_walletAddress = "";
    private float m_ghstBalance = 0;

    private string authUri = "https://db.playdropt.io/web3auth";

    public enum ConnectionState { NotConnected, ConnectedNotAuthenticated, ConnectedAndAuthenticated }
    private ConnectionState m_connectionState = ConnectionState.NotConnected;

    public ConnectionState GetConnectionState() { return m_connectionState; }

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


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_walletAddress = "";
        SetContractAddresses((int)ChainId);
        SetABIs();

        // only the client should handle sign ins and poll the user wallet
        if (IsClient)
        {
            m_signInButton.onClick.AddListener(HandleClick_ConnectAndSignIn);
            _ = PollWallet();
            PollWalletStatus().Forget();
        }
    }

    private void Update()
    {
        SetPanel(m_connectionState);
    }

    private async UniTaskVoid PollWalletStatus()
    {
        if (!IsClient) return;

        while (gameObject != null)
        {
            await UniTask.Delay(k_walletPollInterval_ms);

            _ = PollWallet();
        }
    }

    private async UniTaskVoid PollWallet()
    {
        if (!IsClient) return;

        m_wallet = ThirdwebManager.Instance.GetActiveWallet();

        if (m_wallet != null)
        {
            var newWalletAddress = await m_wallet.GetAddress();
            if (m_walletAddress != newWalletAddress)
            {
                m_walletAddress = newWalletAddress;
                m_connectionState = ConnectionState.ConnectedNotAuthenticated;
            }

            var walletBalance = await m_wallet.GetBalance(ChainId, Contracts.ghst);
            m_ghstBalance = (float)(walletBalance) / 1e18f;
        }
        else
        {
            m_connectionState = ConnectionState.NotConnected;
        }
    }

    private void SetPanel(ConnectionState connectionState)
    {
        if (connectionState == ConnectionState.NotConnected)
        {
            m_signInText.text = "Connect";
            m_signInText.fontSize = 16;
            m_signInImage.color = m_signInColor;

            m_ghstBalanceText.gameObject.SetActive(false);

            m_leftPanel.gameObject.SetActive(false);
        }
        else if (connectionState == ConnectionState.ConnectedNotAuthenticated)
        {
            m_signInText.text = "Sign In";
            m_signInText.fontSize = 16;
            m_signInImage.color = m_signInColor;

            m_ghstBalanceText.gameObject.SetActive(false);

            m_leftPanel.gameObject.SetActive(false);
        }
        else if (connectionState == ConnectionState.ConnectedAndAuthenticated)
        {
            m_signInText.text = ShortenString(m_walletAddress);
            m_signInText.fontSize = 12;

            m_ghstBalanceText.gameObject.SetActive(true);
            m_ghstBalanceText.text = m_ghstBalance.ToString("F2") + " GHST";

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
    }

    public void ConnectAndSignIn()
    {
        if (!IsClient) return;

        HandleClick_ConnectAndSignIn();
    }

    void HandleClick_ConnectAndSignIn()
    {
        if (!IsClient) return;

        _ = HandleClick_ConnectAndSignIn_ASYNC();
    }

    async UniTaskVoid HandleClick_ConnectAndSignIn_ASYNC()
    {
        if (!IsClient) return;

        try
        {
            if (m_connectionState == ConnectionState.NotConnected)
            {
                var existingWallet = ThirdwebManager.Instance.GetActiveWallet();
                if (existingWallet != null)
                {
                    await existingWallet.Disconnect();
                }

                    if (m_wallet == null)
                    {
    #if UNITY_WEBGL
                    var newProvider = WalletProvider.MetaMaskWallet;
    #else
                    var newProvider = WalletProvider.WalletConnectWallet;
    #endif
                    Debug.Log($"Set provider: {newProvider.ToString()}");

                    var walletOptions = new WalletOptions(provider: newProvider, chainId: ChainId);

                    IThirdwebWallet

                    m_wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);
                    if (m_wallet == null)
                    {
                        Debug.LogWarning("No active wallet found!");
                        return;
                    }

                    m_connectionState = ConnectionState.ConnectedNotAuthenticated;

                }

            }
            else if (m_connectionState == ConnectionState.ConnectedNotAuthenticated)
            {
                //if we now have a wallet the next step is to authenticate it with the server
                _ = AuthenticateUser(m_wallet);
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    async UniTaskVoid AuthenticateUser(IThirdwebWallet wallet)
    {
        if (!IsClient) return;

        try
        {
            var walletAddress = await wallet.GetAddress();
            if (string.IsNullOrEmpty(walletAddress))
            {
                Debug.LogError("Could not retrieve wallet address.");
                return;
            }

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            string message = "Welcome to Dropt! Please sign in to verify you are the true owner of " +
                walletAddress + ". \nTime: " + timestamp;

            string signature = await wallet.PersonalSign(message);
            if (string.IsNullOrEmpty(signature))
            {
                Debug.LogWarning("Failed to sign message");
                return;
            }

            // send to server to then confirm with backend
            var clientAuthNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
            AuthenticateUserServerRpc(walletAddress, message, signature, clientAuthNetworkObjectId);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }


    [Rpc(SendTo.Server)]
    void AuthenticateUserServerRpc(string address, string message, string signature, ulong clientAuthNetworkObjectId)
    {
        _ = AuthenticateUserServerRpc_ASYNC(address, message, signature, clientAuthNetworkObjectId);
    }

    async UniTaskVoid AuthenticateUserServerRpc_ASYNC(string address, string message, string signature, ulong clientAuthNetworkObjectId)
    {
        Debug.Log("Send message to web3auth.js");
        try
        {
            var url = authUri + "/verify";
            Debug.Log(url);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {

                var authRequest = new AuthRequest
                {
                    address = address,
                    message = message,
                    signature = signature
                };
                string jsonPayload = JsonUtility.ToJson(authRequest);

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = await request.SendWebRequest();

                while (!operation.isDone)
                {
                    await UniTask.Yield();
                }

                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        Debug.Log("Success: " + request.downloadHandler.text); // Return the response content
                        ConfirmAuthenticationClientRpc(request.downloadHandler.text, clientAuthNetworkObjectId);
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                    default:
                        Debug.LogError($"GetRequest() error: {request.error}");
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ConfirmAuthenticationClientRpc(string token, ulong clientAuthNetworkObjectId)
    {
        var networkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("ConfirmAuthenticationClientRpc: " + networkObjectId + " vs " + clientAuthNetworkObjectId);
        if (networkObjectId != clientAuthNetworkObjectId)
        {
            return;
        }

        Debug.Log("Client successfully received token: " + token);

        m_connectionState = ConnectionState.ConnectedAndAuthenticated;
    }

    public override void OnDestroy()
    {
        m_signInButton.onClick.RemoveAllListeners();

        base.OnDestroy();
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

    [System.Serializable]
    public struct AuthRequest
    {
        public string address;
        public string message;
        public string signature;
    }
}
