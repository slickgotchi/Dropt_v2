using PortalDefender.AavegotchiKit;
using PortalDefender.AavegotchiKit.GraphQL;
using System;
using System.Collections.Generic;
using Thirdweb.Unity;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PortalDefender.AavegotchiKit;

namespace GotchiHub
{
    public class GotchiDataManager : MonoBehaviour
    {
        public static GotchiDataManager Instance { get; private set; }

        [Header("Graph Manager")]
        public GraphManager graphManager;

        [Header("Materials")]
        public Material Material_Sprite_Unlit_Default;
        public Material Material_Sprite_Lit_Default;
        public Material Material_Unlit_VectorGradient;
        public Material Material_Unlit_VectorGradientUI;
        public Material Material_Unlit_VectorUI;

        [Header("Sprite Styling")]
        public GotchiSvgStyling stylingGame;
        public GotchiSvgStyling stylingUI;

        // these are default gotchis free players can use
        [Header("Offchain & Default Gotchis")]
        public List<DefaultGotchiData> offchainGotchiConfigs = new List<DefaultGotchiData>();
        public List<GotchiData> offchainGotchiData = new List<GotchiData>();

        // these our local players wallet gotchis
        [HideInInspector] public List<GotchiData> localWalletGotchiData = new List<GotchiData>();
        [HideInInspector] public List<GotchiSvgSet> localWalletGotchiSvgSets = new List<GotchiSvgSet>();

        // these are other players we need to load to show remotely
        [HideInInspector] public List<GotchiData> remoteGotchiData = new List<GotchiData>();
        [HideInInspector] public List<GotchiSvgSet> remoteGotchiSvgSets = new List<GotchiSvgSet>();

        private int m_selectedGotchiId = 69420;
        public int GetSelectedGotchiId() { return m_selectedGotchiId; }

        // Event declaration
        public event Action<int> onSelectedGotchi;
        public event Action onFetchAllGotchiDataAndSvgs;
        public event Action onFetchedLocalWalletGotchiData;
        public event Action onFetchedWalletGotchiSVG;

        public enum DroptStat { Hp, AttackPower, CriticalChance, Ap, DoubleStrikeChance, CriticalDamage }
        public enum StatBreakdown { Total, Gotchi, Equipment }

        public enum ReorganizeMethod
        {
            BRSLowToHigh,
            BRSHighToLow,
            IdLowToHigh,
            IdHighToLow
        }

        private string m_walletAddress;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);


        }

        // START UP PROCESS
        // - check for wallet saved in player prefs
        // - if have wallet, connect to that wallet, load gotchis for that wallet
        // - gotchi loader should check if a gotchi is existing in memory first before attempting to load
        // - check if selected gotchi in player prefs
        // - select the gotchi in player prefs OR select default logo gotchi

        private void Start()
        {
            StartAsync();
        }

        private async void StartAsync()
        {
            if (Bootstrap.IsServer()) return;

            ClearGotchiDataAndSvgs();

            var existingWalletAddress = PlayerPrefs.GetString("WalletAddress");
            if (string.IsNullOrEmpty(existingWalletAddress))
            {
                // load default gotchi / do nothing
            }
            else
            {
                await Dropt.Utils.Thirdweb.ConnectWallet();

                var wallet = ThirdwebManager.Instance.GetActiveWallet();
                if (wallet == null)
                {
                    Debug.LogWarning("Failed to connect to a wallet");
                    return;
                }

                var isConnected = await wallet.IsConnected();
                if (!isConnected)
                {
                    Debug.LogWarning("Wallet is not connected");
                    return;
                }

                var connectedWalletAddress = await wallet.GetAddress();
                if (connectedWalletAddress != existingWalletAddress)
                {
                    PlayerPrefs.SetString("WalletAddress", connectedWalletAddress);
                    m_walletAddress = connectedWalletAddress;
                }
                else
                {
                    m_walletAddress = existingWalletAddress;
                }

                LoadGotchiDataAndSvgsForLocalWalletAddress(m_walletAddress);
            }
        }

        private void ClearGotchiDataAndSvgs()
        {
            localWalletGotchiData.Clear();
            localWalletGotchiSvgSets.Clear();
            remoteGotchiData.Clear();
            remoteGotchiSvgSets.Clear();

            offchainGotchiData.Clear();

            // populate offchain gotchis
            foreach (var defaultGotchiData in offchainGotchiConfigs)
            {
                var newGotchiData = new GotchiData
                {
                    id = defaultGotchiData.id,
                    hauntId = defaultGotchiData.hauntId,
                    name = defaultGotchiData.name,
                    collateral = defaultGotchiData.collateral,
                    numericTraits = defaultGotchiData.numericTraits,
                    equippedWearables = defaultGotchiData.equippedWearables,
                    level = defaultGotchiData.level,
                    kinship = defaultGotchiData.kinship,
                    status = defaultGotchiData.status
                };

                offchainGotchiData.Add(newGotchiData);
            }
        }

        public bool SetSelectedGotchiById(int id)
        {
            for (int i = 0; i < localWalletGotchiData.Count; i++)
            {
                if (id == localWalletGotchiData[i].id)
                {
                    m_selectedGotchiId = id;
                    onSelectedGotchi?.Invoke(m_selectedGotchiId); // Trigger event
                    PlayerPrefs.SetInt("GotchiId", id);
                    return true;
                }
            }

            for (int i = 0; i < offchainGotchiData.Count; i++)
            {
                if (id == offchainGotchiData[i].id)
                {
                    m_selectedGotchiId = id;
                    onSelectedGotchi?.Invoke(m_selectedGotchiId);
                    PlayerPrefs.SetInt("GotchiId", id);
                    return true;
                }
            }

            //Debug.Log("Gotchi with id " + id + " does not exist in GotchiDataManager");
            return false;
        }

        public GotchiData GetGotchiDataById(int id)
        {
            // check remote first
            for (int i = 0; i < remoteGotchiData.Count; i++)
            {
                if (id == remoteGotchiData[i].id)
                {
                    //Debug.Log("foudn remote gotchi match: " + remoteGotchiData[i]);
                    return remoteGotchiData[i];
                }
            }

            // now check local
            for (int i = 0; i < localWalletGotchiData.Count; i++)
            {
                if (id == localWalletGotchiData[i].id)
                {
                    //Debug.Log("found local gotchi match: " + localWalletGotchiData[i]);
                    return localWalletGotchiData[i];
                }
            }

            // now check offchain
            for (int i = 0; i < offchainGotchiData.Count; i++)
            {
                if (id == offchainGotchiData[i].id)
                {
                    return offchainGotchiData[i];
                }
            }

            // if got here we were passed invalid id
            //Debug.Log("Invalid id passed to GetGotchiDataById()");
            return null;
        }

        public GotchiData GetOffchainGotchiDataById(int id)
        {
            // finally check offchain
            for (int i = 0; i < offchainGotchiData.Count; i++)
            {
                if (id == offchainGotchiData[i].id)
                {
                    return offchainGotchiData[i];
                }
            }

            // if got here we were passed invalid id
            //Debug.Log("Invalid id passed to GetOffchainGotchiDataById()");
            return null;
        }

        public DefaultGotchiData GetOffchainGotchiConfigById(int id)
        {
            // finally check offchain
            for (int i = 0; i < offchainGotchiConfigs.Count; i++)
            {
                if (id == offchainGotchiConfigs[i].id)
                {
                    return offchainGotchiConfigs[i];
                }
            }

            // if got here we were passed invalid id
            //Debug.Log("Invalid id passed to GetOffchainGotchiDataById()");
            return null;
        }

        public GotchiSvgSet GetGotchiSvgsById(int id)
        {
            // check remote first
            for (int i = 0; i < remoteGotchiSvgSets.Count; i++)
            {
                if (id == remoteGotchiSvgSets[i].id)
                {
                    return remoteGotchiSvgSets[i];
                }
            }

            // now check local
            for (int i = 0; i < localWalletGotchiSvgSets.Count; i++)
            {
                if (id == localWalletGotchiSvgSets[i].id)
                {
                    return localWalletGotchiSvgSets[i];
                }
            }

            // if got here we were passed invalid id
            //Debug.Log("Invalid id passed to GetGotchiSvgsById()");
            return null;
        }

        public async UniTask FetchWalletGotchiData()
        {
            try
            {
                // clear all current data
                ClearGotchiDataAndSvgs();

                // get wallet
                var wallet = ThirdwebManager.Instance.GetActiveWallet();
                if (wallet == null)
                {
                    return;
                }

                // get wallet address
                var walletAddress = await wallet.GetAddress();
                if (walletAddress == null)
                {
                    return;
                }

                walletAddress = walletAddress.ToLower();

                // fetch gotchis with aavegotchi kit
                var userAccount = await graphManager.GetUserAccount(walletAddress);

                // save base gotchi data
                //var gotchiIds = new List<string>();
                foreach (var gotchi in userAccount.gotchisOwned)
                {
                    localWalletGotchiData.Add(gotchi);
                    //gotchiIds.Add(gotchi.id.ToString());
                }

                // lets reorder the data to go from highest brs to lowest
                ReorganizeLocalWalletGotchis(ReorganizeMethod.BRSHighToLow);

                onFetchedLocalWalletGotchiData?.Invoke();

                await FetchGotchiSvgsParallelForLocalAccount(userAccount);

                // default to highest brs gotchi
                if (localWalletGotchiData.Count > 0)
                {
                    SetSelectedGotchiById(GetGotchiIdByHighestBRS());

                    onFetchAllGotchiDataAndSvgs?.Invoke();
                }
                

            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public async UniTask LoadGotchiDataAndSvgsForLocalWalletAddress(string walletAddress)
        {
            try
            {
                walletAddress = walletAddress.ToLower();

                // fetch gotchis with aavegotchi kit
                var userAccount = await graphManager.GetUserAccount(walletAddress);

                // save base gotchi data
                //var gotchiIds = new List<string>();
                foreach (var gotchi in userAccount.gotchisOwned)
                {
                    if (!localWalletGotchiData.Contains(gotchi))
                    {
                        localWalletGotchiData.Add(gotchi);
                    }
                }

                // lets reorder the data to go from highest brs to lowest
                ReorganizeLocalWalletGotchis(ReorganizeMethod.BRSHighToLow);

                await LoadGotchiSvgsParallelForLocalAccount(userAccount);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public async UniTask FetchGotchiSvgsParallelForLocalAccount(UserAccount userAccount)
        {
            try
            {
                List<UniTask> fetchTasks = new List<UniTask>();

                for (int i = 0; i < userAccount.gotchisOwned.Length; i++)
                {
                    Debug.Log("Fetch Gotchi SVG: " + i);
                    int index = i; // Capture the loop variable to use inside the async lambda

                    var task = UniTask.Create(async () =>
                    {
                        // Check for potential null values
                        if (userAccount.gotchisOwned[index] == null)
                        {
                            Debug.LogError($"Gotchi at index {index} is null.");
                            return;
                        }

                        Debug.Log($"Gotchi ID: {userAccount.gotchisOwned[index].id}");
                        if (userAccount.gotchisOwned[index].id == null)
                        {
                            Debug.LogError($"Gotchi ID at index {index} is null.");
                            return;
                        }

                        // check if already have svg
                        for (int j = 0; j < localWalletGotchiSvgSets.Count; j++)
                        {
                            if (localWalletGotchiSvgSets[j].id == userAccount.gotchisOwned[index].id)
                            {
                                return;
                            }
                        }

                        // we can now get the svg
                        var svg = await graphManager.GetGotchiSvg(userAccount.gotchisOwned[index].id.ToString());

                        if (svg == null)
                        {
                            Debug.LogError($"SVG data for Gotchi ID {userAccount.gotchisOwned[index].id} is null.");
                            return;
                        }

                        localWalletGotchiSvgSets.Add(new GotchiSvgSet
                        {
                            id = int.Parse(userAccount.gotchisOwned[index].id.ToString()),
                            Front = svg.svg,
                            Back = svg.back,
                            Left = svg.left,
                            Right = svg.right,
                        });

                        Debug.Log("onFetchedWalletGotchiSVG?.Invoke()");
                        onFetchedWalletGotchiSVG?.Invoke();
                    });

                    fetchTasks.Add(task);
                }

                // Await all tasks to complete concurrently
                await UniTask.WhenAll(fetchTasks);
            }
            catch (System.Exception err)
            {
                Debug.LogError(err.Message);
            }
        }

        public async UniTask LoadGotchiSvgsParallelForLocalAccount(UserAccount userAccount)
        {
            try
            {
                List<UniTask> fetchTasks = new List<UniTask>();

                for (int i = 0; i < userAccount.gotchisOwned.Length; i++)
                {
                    Debug.Log("Fetch Gotchi SVG: " + i);
                    int index = i; // Capture the loop variable to use inside the async lambda

                    var task = UniTask.Create(async () =>
                    {
                        // Check for potential null values
                        if (userAccount.gotchisOwned[index] == null)
                        {
                            Debug.LogError($"Gotchi at index {index} is null.");
                            return;
                        }

                        Debug.Log($"Gotchi ID: {userAccount.gotchisOwned[index].id}");
                        if (userAccount.gotchisOwned[index].id == null)
                        {
                            Debug.LogError($"Gotchi ID at index {index} is null.");
                            return;
                        }

                        // check if already have svg
                        for (int j = 0; j < localWalletGotchiSvgSets.Count; j++)
                        {
                            if (localWalletGotchiSvgSets[j].id == userAccount.gotchisOwned[index].id)
                            {
                                return;
                            }
                        }

                        // we can now get the svg
                        var svg = await graphManager.GetGotchiSvg(userAccount.gotchisOwned[index].id.ToString());

                        if (svg == null)
                        {
                            Debug.LogError($"SVG data for Gotchi ID {userAccount.gotchisOwned[index].id} is null.");
                            return;
                        }

                        localWalletGotchiSvgSets.Add(new GotchiSvgSet
                        {
                            id = int.Parse(userAccount.gotchisOwned[index].id.ToString()),
                            Front = svg.svg,
                            Back = svg.back,
                            Left = svg.left,
                            Right = svg.right,
                        });
                    });

                    fetchTasks.Add(task);
                }

                // Await all tasks to complete concurrently
                await UniTask.WhenAll(fetchTasks);
            }
            catch (System.Exception err)
            {
                Debug.LogError(err.Message);
            }
        }

        public async UniTask<bool> FetchGotchiById(int id)
        {
            // first check if already have that id
            var checkGotchiData = GetGotchiDataById(id);
            if (checkGotchiData != null)
            {
                return true;
            }

            var gotchiData = await graphManager.GetGotchiData(id.ToString());
            if (gotchiData != null && gotchiData.id != 0)   // we need to check against id 0 here too because if we don't have the gotchidata we still seem to return a valid gotchiData object
            {
                var svgs = await graphManager.GetGotchiSvg(id.ToString());
                if (svgs != null)
                {
                    remoteGotchiData.Add(gotchiData);
                    remoteGotchiSvgSets.Add(new GotchiSvgSet
                    {
                        id = id,
                        Front = svgs.svg,
                        Back = svgs.back,
                        Left = svgs.left,
                        Right = svgs.right,
                    });
                    return true;
                }
            }

            // try get an offchain gotchi
            var offchainGotchiData = GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                return true;
            }

            // otherwise, output exception and return false
            return false;
        }

        public async void FetchRemoteGotchiSvgsById(int id)
        {
            try
            {
                var svgs = await graphManager.GetGotchiSvg(id.ToString());
                if (svgs.svg != null)
                {
                    remoteGotchiSvgSets.Add(new GotchiSvgSet
                    {
                        id = id,
                        Front = svgs.svg,
                        Back = svgs.back,
                        Left = svgs.left,
                        Right = svgs.right,
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public int GetGotchiIdByHighestBRS()
        {
            int highestBrs = 0;
            int highestBrsId = -1;

            for (int i = 0; i < localWalletGotchiData.Count; i++)
            {
                var brs = DroptStatCalculator.GetBRS(localWalletGotchiData[i].numericTraits);
                if (brs > highestBrs)
                {
                    highestBrs = brs;
                    highestBrsId = localWalletGotchiData[i].id;
                }
            }

            return highestBrsId;
        }


        public void ReorganizeLocalWalletGotchis(ReorganizeMethod method)
        {
            // Sort the list based on the selected method
            switch (method)
            {
                case ReorganizeMethod.BRSLowToHigh:
                    localWalletGotchiData.Sort((item1, item2) =>
                        DroptStatCalculator.GetBRS(item1.numericTraits).CompareTo(DroptStatCalculator.GetBRS(item2.numericTraits)));
                    break;

                case ReorganizeMethod.BRSHighToLow:
                    localWalletGotchiData.Sort((item1, item2) =>
                        DroptStatCalculator.GetBRS(item2.numericTraits).CompareTo(DroptStatCalculator.GetBRS(item1.numericTraits)));
                    break;

                case ReorganizeMethod.IdLowToHigh:
                    localWalletGotchiData.Sort((item1, item2) =>
                        item1.id.CompareTo(item2.id));
                    break;

                case ReorganizeMethod.IdHighToLow:
                    localWalletGotchiData.Sort((item1, item2) =>
                        item2.id.CompareTo(item1.id));
                    break;
            }
        }
    }

    

    public class GotchiSvgSet
    {
        public int id;
        public string Front;
        public string Back;
        public string Left;
        public string Right;
    }
}
