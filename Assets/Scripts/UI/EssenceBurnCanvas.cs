using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GotchiHub;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using Thirdweb;
using Thirdweb.Unity;
using Cysharp.Threading.Tasks;
using System.Numerics;
using Unity.VectorGraphics;

public class EssenceBurnCanvas : DroptCanvas
{
    public static EssenceBurnCanvas Instance { get; private set; }

    [HideInInspector] public Interactable interactable;

    [SerializeField] private Button m_approveButton;
    [SerializeField] private TMPro.TextMeshProUGUI m_approvedEssenceText;
    [SerializeField] private Button m_burnButton;

    [SerializeField] private Button m_exitButton;
    [SerializeField] private SVGImage m_gotchiImage;
    [SerializeField] private TMPro.TextMeshProUGUI m_gotchiName;

    ThirdwebContract m_essenceContract;
    //private int m_approvedEssence = 0;
    private bool m_isApprovedEssence = false;

    ThirdwebContract m_paymentProcessorContract;

    private void Awake()
    {
        // Singleton pattern 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_exitButton.onClick.AddListener(HandleClick_Exit);
        m_approveButton.onClick.AddListener(HandleClick_Approve);
        m_burnButton.onClick.AddListener(HandleClick_Burn);

        InstaHideCanvas();

        m_burnButton.gameObject.SetActive(false);


    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();

        var selectedGotchiId = GotchiDataManager.Instance.GetSelectedGotchiId();
        InitById(selectedGotchiId);

        _ = CheckEssenceApproval();
        _ = PollUpdates();
    }

    public override void OnHideCanvas()
    {
        base.OnHideCanvas();
    }

    public override void OnUpdate()
    {
        // check if gotchi already has essence infused
        var localPlayerController = Game.Instance.LocalPlayerController;
        if (localPlayerController == null) return;

        var localPlayerOffchainData = localPlayerController.GetComponent<PlayerOffchainData>();
        if (localPlayerOffchainData == null) return;

        if (localPlayerOffchainData.m_isEssenceInfused_gotchi.Value)
        {
            m_approveButton.gameObject.SetActive(false);
            m_burnButton.gameObject.SetActive(false);
            m_approvedEssenceText.text = "Gotchi essence already infused";

        }
        else if (!m_isApprovedEssence)
        {
            m_approveButton.gameObject.SetActive(true);
            m_burnButton.gameObject.SetActive(false);
            m_approvedEssenceText.text = "Not Approved";

        }
        else
        {
            m_approveButton.gameObject.SetActive(false);
            m_burnButton.gameObject.SetActive(true);
            m_approvedEssenceText.text = "Approved";

        }
    }

    private async UniTaskVoid PollUpdates()
    {
        while (isCanvasOpen)
        {
            await UniTask.Delay(3000);

            _ = CheckEssenceApproval();
        }
    }

    private async UniTaskVoid CheckEssenceApproval()
    {
        if (m_essenceContract == null)
        {
            m_essenceContract = await ThirdwebManager.Instance.GetContract(
                address: Web3AuthCanvas.Instance.Contracts.essence,
                chainId: Web3AuthCanvas.Instance.ChainId,
                abi: Web3AuthCanvas.Instance.ABIs.essence
                );

            if (m_essenceContract == null) { Debug.LogWarning("Can not get Essence contract"); return; }
        }

        // Get the user's wallet
        IThirdwebWallet wallet = Web3AuthCanvas.Instance.GetActiveWallet();
        if (wallet == null)
        {
            return;
        }

        m_isApprovedEssence = await ThirdwebContract.Read<bool>(
            m_essenceContract,
            "isApprovedForAll",
            Web3AuthCanvas.Instance.GetActiveWalletAddress(),
            Web3AuthCanvas.Instance.Contracts.droptPaymentProcessor
            );
    }

    void HandleClick_Approve()
    {
        _ = HandleClick_Approve_ASYNC();
    }

    async UniTaskVoid HandleClick_Approve_ASYNC()
    {
        Debug.Log("Try approve: " + 250 + " Essence");
        try
        {
            // convert int amount into big int
            BigInteger weiAmount = new BigInteger(250) * BigInteger.Pow(10, 18);
            
            // Get the essence/forge contract instance
            ThirdwebContract essenceContract = await ThirdwebManager.Instance.GetContract(
                address: Web3AuthCanvas.Instance.Contracts.essence,
                chainId: Web3AuthCanvas.Instance.ChainId,
                abi: Web3AuthCanvas.Instance.ABIs.essence
            );

            if (essenceContract == null)
            {
                Debug.LogError("Failed to retrieve the essence contract.");
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
                essenceContract,
                "setApprovalForAll",
                0,
                Web3AuthCanvas.Instance.Contracts.droptPaymentProcessor,
                true
            );

            Debug.Log("Prepared essence approve transaction");

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

    void HandleClick_Burn()
    {
        int gotchiId = GotchiDataManager.Instance.GetSelectedGotchiId();
        _ = HandleClick_Burn_ASYNC(gotchiId);
    }

    async UniTaskVoid HandleClick_Burn_ASYNC(int gotchiId)
    {

        Debug.Log("Try burn: " + 250 + " Essence for gotchiId: " + gotchiId);

        if (m_paymentProcessorContract == null)
        {
            m_paymentProcessorContract = await ThirdwebManager.Instance.GetContract(
                address: Web3AuthCanvas.Instance.Contracts.droptPaymentProcessor,
                chainId: Web3AuthCanvas.Instance.ChainId,
                abi: Web3AuthCanvas.Instance.ABIs.paymentProcessor
                );

            if (m_paymentProcessorContract == null)
            {
                Debug.LogWarning("Could not get DroptPaymentProccessor contract");
                return;
            }
        }

        var wallet = Web3AuthCanvas.Instance.GetActiveWallet();
        if (wallet == null)
        {
            Debug.LogWarning("No active wallet found. Can not process transaction");
            return;
        }

        BigInteger weiAmount = new BigInteger(250) * BigInteger.Pow(10, 18);

        var prepareTxn = await ThirdwebContract.Prepare(
            wallet,
            m_paymentProcessorContract,
            "payWithEssence",
            0,
            weiAmount,
            gotchiId
            );

        Debug.Log("Prepared transaction");

        BigInteger estimateGas = await ThirdwebTransaction.EstimateGasLimit(prepareTxn);
        prepareTxn.SetGasLimit(estimateGas);
        Debug.Log("Estimated and set gas limit: " + estimateGas);

        // Estimate Max Fee per Gas & Max Priority Fee per Gas
        (BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas) = await ThirdwebTransaction.EstimateGasFees(prepareTxn);

        /*
        // we need to set gas higher for amoy
        if (Web3AuthCanvas.Instance.ChainId == 80002)
        {
            BigInteger gwei = BigInteger.Pow(10, 9);
            maxFeePerGas = 49 * gwei;
            maxPriorityFeePerGas = 49 * gwei;
        }
        */

        prepareTxn.SetMaxFeePerGas(maxFeePerGas);
        prepareTxn.SetMaxPriorityFeePerGas(maxPriorityFeePerGas);
        Debug.Log($"Estimated & Set Gas Fees - Max Fee: {maxFeePerGas}, Max Priority Fee: {maxPriorityFeePerGas}");

        var receipt = await ThirdwebTransaction.Send(prepareTxn);

        Debug.Log($"Approval transaction completed. Tx Hash: {receipt.GetType()}");

        BazaarCanvas.Instance.ShowPurchaseSuccessModal();

        return;
    }

    void HandleClick_Exit()
    {
        EssenceBurnCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }



    public void InitById(int id)
    {
        Debug.Log("Init gotchi of id: " + id);
        var gotchiSvg = GotchiDataManager.Instance.GetGotchiSvgsById(id);
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);

        bool isOffchain = GotchiDataManager.Instance.GetOffchainGotchiDataById(id) != null;

        // if we got onchain data, set svg image
        if (!isOffchain)
        {
            Debug.Log("Set burn essence gotchi image");
            m_gotchiImage.sprite = CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingUI.CustomizeSVG(gotchiSvg.Front), UnityEngine.Vector2.zero);
            m_gotchiImage.material = GotchiDataManager.Instance.Material_Unlit_VectorGradientUI;
            m_gotchiName.text = gotchiData.name + " (" + gotchiData.id + ")";
            return;
        }
    }
}
