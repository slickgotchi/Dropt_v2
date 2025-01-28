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


    [SerializeField] private Button m_exitButton;

    [SerializeField] private Button m_approveGhstButton;
    [SerializeField] private InputField m_approveGhstInputField;

    
    
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

        InstaHideCanvas();

    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();

    }

    public override void OnHideCanvas()
    {
        base.OnHideCanvas();
    }

    public override void OnUpdate()
    {

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

    void HandleClickApprove()
    {
        int approveAmount = 10;

        HandleClickApproveAsync();
    }

    async UniTaskVoid HandleClickApproveAsync()
    {
        Debug.Log("Approving 10 GHST on Amoy");

        try
        {
            // Load ABI for the GHST contract
            TextAsset abiFile = Resources.Load<TextAsset>("ghst-abi");
            if (abiFile == null)
            {
                Debug.LogError("ghst-abi.json file not found in Resources folder.");
                return;
            }

            string ghstAbi = abiFile.text;
            Debug.Log("ABI Loaded: " + ghstAbi);

            // Variables
            BigInteger amount = BigInteger.Parse("10000000000000000000"); // 10 GHST in wei
            string spenderAddress_amoy = "0x32EFD2fBb0a43eE1918621D0544EFbd2c7F77beE"; // Spender contract
            string ghstContractAddress_amoy = "0xF679b8D109b2d23931237Ce948a7D784727c0897"; // GHST token contract
            string ghstContractAddress_polygon = "0x385eeac5cb85a38a9a07a70c73e0a3271cfb54a7";

            var chainId = 80002;

            

            // Get the GHST contract instance
            ThirdwebContract contract = await ThirdwebManager.Instance.GetContract(
                address: ghstContractAddress_amoy,
                chainId: chainId,
                abi: ghstAbi
            );

            if (contract == null)
            {
                Debug.LogError("Failed to retrieve the GHST contract.");
                return;
            }

            Debug.Log("GHST Contract retrieved: " + contract);

            // Get the user's wallet
            IThirdwebWallet wallet = ThirdwebManager.Instance.GetActiveWallet();
            if (wallet == null)
            {
                Debug.LogError("No active wallet found!");
                return;
            }
            
            ThirdwebTransactionReceipt receipt = await ThirdwebContract.Write(
                wallet,
                contract,
                "approve",
                0, // No ETH required for this transaction
                spenderAddress_amoy,
                amount
            );

            Debug.Log($"Approval transaction completed. Tx Hash: {receipt.TransactionHash}");
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during approval: {ex.Message}");
        }
    }

}
