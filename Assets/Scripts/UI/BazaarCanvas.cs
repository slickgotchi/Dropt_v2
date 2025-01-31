using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb.Unity;
using Cysharp.Threading.Tasks;
using Thirdweb;
using System.Numerics;
using System;

public class BazaarCanvas : DroptCanvas
{
    public static BazaarCanvas Instance { get; private set; }

    [HideInInspector] public Interactable interactable;

    [Header("Buttons")]
    [SerializeField] private Button m_exitButton;
    [SerializeField] private Button m_signInButton;
    [SerializeField] private TMPro.TextMeshProUGUI m_signInButtonText;
    [SerializeField] private Button m_approveGhstButton;

    [Header("Input Field")]
    [SerializeField] private TMPro.TMP_InputField m_approveGhstInputField;

    [Header("Approved GHST Text")]
    [SerializeField] private TMPro.TextMeshProUGUI m_approvedGhstText;

    private int m_approvedGhst = 0;

    [Header("Panels")]
    [SerializeField] private GameObject m_signInPanel;
    [SerializeField] private GameObject m_approveGhstPanel;
    [SerializeField] private GameObject m_purchaseItemsPanel;
    [SerializeField] private GameObject m_purchaseSuccessModal;

    ThirdwebContract m_ghstContract;
    
    private void Awake()
    {
        // Singleton pattern 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_exitButton.onClick.AddListener(HandleClickExit);
        m_approveGhstButton.onClick.AddListener(HandleClickApprove);
        m_signInButton.onClick.AddListener(HandleClickSignIn);

        m_purchaseSuccessModal.SetActive(false);

        InstaHideCanvas();

    }

    public void ShowPurchaseSuccessModal()
    {
        m_purchaseSuccessModal.SetActive(true);
    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();


        _ = PollUpdates();
    }

    void ConfigureMainPanel()
    {
        switch (Web3AuthCanvas.Instance.GetConnectionState())
        {
            case Web3AuthCanvas.ConnectionState.NotConnected:
                m_signInPanel.SetActive(true);
                m_approveGhstPanel.SetActive(false);
                m_purchaseItemsPanel.SetActive(false);

                m_signInButtonText.text = "Connect";
                break;
            case Web3AuthCanvas.ConnectionState.ConnectedNotAuthenticated:
                m_signInPanel.SetActive(true);
                m_approveGhstPanel.SetActive(false);
                m_purchaseItemsPanel.SetActive(false);

                m_signInButtonText.text = "Sign In";
                break;
            case Web3AuthCanvas.ConnectionState.ConnectedAndAuthenticated:
                if (m_approvedGhst <= 3)
                {
                    m_signInPanel.SetActive(false);
                    m_approveGhstPanel.SetActive(true);
                    m_purchaseItemsPanel.SetActive(false);
                }
                else
                {
                    m_signInPanel.SetActive(false);
                    m_approveGhstPanel.SetActive(false);
                    m_purchaseItemsPanel.SetActive(true);
                }
                break;
            default: break;
        }
    }

    public override void OnHideCanvas()
    {
        base.OnHideCanvas();
    }



    public override void OnUpdate()
    {
        ConfigureMainPanel();
    }

    private async UniTaskVoid PollUpdates()
    {
        while (isCanvasOpen)
        {
            await UniTask.Delay(3000);

            _ = CheckGhstApproval();
        }
    }

    private async UniTaskVoid CheckGhstApproval()
    {
        if (m_ghstContract == null)
        {
            m_ghstContract = await ThirdwebManager.Instance.GetContract(
                address: Web3AuthCanvas.Instance.Contracts.ghst,
                chainId: Web3AuthCanvas.Instance.ChainId,
                abi: Web3AuthCanvas.Instance.ABIs.ghst
                );

            if (m_ghstContract == null) { Debug.LogWarning("Can not get GHST contract"); return; }
        }

        // Get the user's wallet
        IThirdwebWallet wallet = Web3AuthCanvas.Instance.GetActiveWallet();
        if (wallet == null)
        {
            return;
        }

        BigInteger weiAllowance = await ThirdwebContract.Read<BigInteger>(
            m_ghstContract,
            "allowance",
            Web3AuthCanvas.Instance.GetActiveWalletAddress(),
            Web3AuthCanvas.Instance.Contracts.droptPaymentProcessor
            );

        float amount = (float)(weiAllowance) / 1e18f;

        m_approvedGhst = (int)((float)weiAllowance / 1e18f);
        m_approvedGhstText.text = amount.ToString("F0") + " Approved";
    }

    void HandleClickExit()
    {
        BazaarCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }

    void HandleClickSignIn()
    {
        switch (Web3AuthCanvas.Instance.GetConnectionState())
        {
            case Web3AuthCanvas.ConnectionState.NotConnected:
                Web3AuthCanvas.Instance.Connect();
                break;
            case Web3AuthCanvas.ConnectionState.ConnectedNotAuthenticated:
                Web3AuthCanvas.Instance.SignIn();
                break;
            default: break;
        }
    }

    void HandleClickApprove()
    {
        bool isFloat = float.TryParse(m_approveGhstInputField.text, out float parsedValue);
        if (!isFloat)
        {
            Debug.LogWarning("Approve quantity should be a number");
            return;
        } 

        _ = HandleClickApproveAsync((int)parsedValue);
    }

    async UniTaskVoid HandleClickApproveAsync(int approveAmount)
    {
        Debug.Log("Try approve: " + approveAmount + " GHST");
        try
        {
            // convert int amount into big int
            BigInteger weiAmount = new BigInteger(approveAmount) * BigInteger.Pow(10, 18);

            // Get the GHST contract instance
            ThirdwebContract contract = await ThirdwebManager.Instance.GetContract(
                address: Web3AuthCanvas.Instance.Contracts.ghst,
                chainId: Web3AuthCanvas.Instance.ChainId,
                abi: Web3AuthCanvas.Instance.ABIs.ghst
            );

            if (contract == null)
            {
                Debug.LogError("Failed to retrieve the GHST contract.");
                return;
            }

            // Get the user's wallet
            IThirdwebWallet wallet = Web3AuthCanvas.Instance.GetActiveWallet();
            if (wallet == null)
            {
                Debug.LogError("No active wallet found!");
                return;
            }

            var prepareTxn = await ThirdwebContract.Prepare(
                wallet,
                contract,
                "approve",
                0,
                Web3AuthCanvas.Instance.Contracts.droptPaymentProcessor,
                weiAmount
            );

            Debug.Log("Prepared transaction");

            BigInteger estimateGas = await ThirdwebTransaction.EstimateGasLimit(prepareTxn);
            prepareTxn.SetGasLimit(estimateGas);
            Debug.Log("Estimated and set gas limit: " + estimateGas);

            // Estimate Max Fee per Gas & Max Priority Fee per Gas
            (BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas) = await ThirdwebTransaction.EstimateGasFees(prepareTxn);

            // we need to set gas higher for amoy
            if (Web3AuthCanvas.Instance.ChainId == 80002)
            {
                BigInteger gwei = BigInteger.Pow(10, 9);
                maxFeePerGas = 49 * gwei;
                maxPriorityFeePerGas = 49 * gwei;
            }

            prepareTxn.SetMaxFeePerGas(maxFeePerGas);
            prepareTxn.SetMaxPriorityFeePerGas(maxPriorityFeePerGas);
            Debug.Log($"Estimated & Set Gas Fees - Max Fee: {maxFeePerGas}, Max Priority Fee: {maxPriorityFeePerGas}");

            var receipt = await ThirdwebTransaction.Send(prepareTxn);

            Debug.Log($"Approval transaction completed. Tx Hash: {receipt.GetType()}");
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during approval: {ex.Message}");
        }
    }

}
