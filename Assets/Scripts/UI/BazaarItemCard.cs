using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Thirdweb.Unity;
using Thirdweb;
using System.Numerics;

public class BazaarItemCard : MonoBehaviour
{
    [Header("Assign in Scene")]
    [SerializeField] private BazaarItem_SO m_bazaarItem_SO;

    [Header("Assign in Prefab")]
    [SerializeField] private Image m_image;
    [SerializeField] private TextMeshProUGUI m_titleText;
    [SerializeField] private TextMeshProUGUI m_descriptionText;
    [SerializeField] private TextMeshProUGUI m_ghstCost;
    [SerializeField] private Button buyButton;

    private ThirdwebContract m_paymentProcessorContract;

    private void Awake()
    {
        buyButton.onClick.AddListener(HandleClickBuy);
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveAllListeners();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_bazaarItem_SO == null)
        {
            Debug.LogWarning("No BazaarItem_SO assigned to BazaarItemCard!");
            return;
        }

        m_image.sprite = m_bazaarItem_SO.sprite;
        m_titleText.text = m_bazaarItem_SO.titleText + " x" + m_bazaarItem_SO.purchaseQty.ToString();
        m_descriptionText.text = m_bazaarItem_SO.descriptionText;
        m_ghstCost.text = m_bazaarItem_SO.ghstCost.ToString();
    }

    void HandleClickBuy()
    {
        _ = HandleClickBuyAsync();
    }

    async UniTaskVoid HandleClickBuyAsync()
    {
        try
        {
            var qty = m_bazaarItem_SO.purchaseQty;

            switch (m_bazaarItem_SO.purchaseType)
            {
                case BazaarItem_SO.PurchaseType.Bomb:
                    if (qty == 10) await TryWeb3Purchase(1, m_bazaarItem_SO.ghstCost);
                    if (qty == 25) await TryWeb3Purchase(2, m_bazaarItem_SO.ghstCost);
                    
                    break;
                case BazaarItem_SO.PurchaseType.PortaHole:
                    if (qty == 1) await TryWeb3Purchase(3, m_bazaarItem_SO.ghstCost);
                    if (qty == 3) await TryWeb3Purchase(4, m_bazaarItem_SO.ghstCost);

                    break;
                case BazaarItem_SO.PurchaseType.ZenCricket:
                    if (qty == 2) await TryWeb3Purchase(5, m_bazaarItem_SO.ghstCost);

                    break;
                case BazaarItem_SO.PurchaseType.Ecto:
                    if (qty == 100) await TryWeb3Purchase(6, m_bazaarItem_SO.ghstCost);

                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    

    async UniTask<bool> TryWeb3Purchase(BigInteger catalogId, float ghstCost)
    {
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
                return false;
            }
        }

        var wallet = Web3AuthCanvas.Instance.GetActiveWallet();
        if (wallet == null)
        {
            Debug.LogWarning("No active wallet found. Can not process transaction");
            return false;
        }

        BigInteger weiAmount = new BigInteger(ghstCost) * BigInteger.Pow(10, 18);

        var prepareTxn = await ThirdwebContract.Prepare(
            wallet,
            m_paymentProcessorContract,
            "payWithGHST",
            0,
            weiAmount,
            catalogId
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

        return true;
    }



}
