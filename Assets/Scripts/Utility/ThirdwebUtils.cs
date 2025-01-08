using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;

namespace Dropt.Utils
{
    public static class Thirdweb
    {
        async public static UniTask ConnectWallet()
        {
            try
            {
                var newProvider = WalletProvider.MetaMaskWallet;

                if (Application.isEditor)
                {
                    newProvider = WalletProvider.WalletConnectWallet;
                }

                Debug.Log($"Set provider: {newProvider.ToString()}");

                var walletOptions = new WalletOptions(provider: newProvider, chainId: 137);

                await ThirdwebManager.Instance.ConnectWallet(walletOptions);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
}
