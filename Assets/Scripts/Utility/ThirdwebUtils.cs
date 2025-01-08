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
#if UNITY_WEBGL
                var newProvider = WalletProvider.MetaMaskWallet;
#else
                var newProvider = WalletProvider.WalletConnectWallet;
#endif

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
