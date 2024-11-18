using PortalDefender.AavegotchiKit;
using PortalDefender.AavegotchiKit.GraphQL;
using System;
using System.Collections.Generic;
using Thirdweb;
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

        [Header("Offchain & Default Gotchis")]
        public List<DefaultGotchiData> offchainGotchiData = new List<DefaultGotchiData>();

        // these our local wallet gotchis
        [HideInInspector] public List<GotchiData> localWalletGotchiData = new List<GotchiData>();
        [HideInInspector] public List<GotchiSvgSet> localWalletGotchiSvgSets = new List<GotchiSvgSet>();

        // these are other players we need to load to show remotely
        [HideInInspector] public List<GotchiData> remoteGotchiData = new List<GotchiData>();
        [HideInInspector] public List<GotchiSvgSet> remoteGotchiSvgSets = new List<GotchiSvgSet>();

        private int m_selectedGotchiId = 69420;
        public int GetSelectedGotchiId() { return m_selectedGotchiId; }

        // Event declaration
        public event Action<int> onSelectedGotchi;
        public event Action onFetchGotchiDataSuccess;
        public event Action onFetchedLocalWalletGotchiCount;
        public event Action onFetchedWalletGotchiSVG;

        public enum DroptStat { Hp, AttackPower, CriticalChance, Ap, DoubleStrikeChance, CriticalDamage }
        public enum StatBreakdown { Total, Gotchi, Equipment }

        private void Awake()
        {
            Instance = this;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            localWalletGotchiData.Clear();
            localWalletGotchiSvgSets.Clear();
            remoteGotchiSvgSets.Clear();
        }

        public bool SetSelectedGotchiById(int id)
        {
            for (int i = 0; i < localWalletGotchiData.Count; i++)
            {
                if (id == localWalletGotchiData[i].id)
                {
                    m_selectedGotchiId = id;
                    onSelectedGotchi?.Invoke(m_selectedGotchiId); // Trigger event
                    return true;
                }
            }

            for (int i = 0; i < offchainGotchiData.Count; i++)
            {
                if (id == offchainGotchiData[i].id)
                {
                    m_selectedGotchiId = id;
                    onSelectedGotchi?.Invoke(m_selectedGotchiId);
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
                    Debug.Log("foudn remote gotchi match: " + remoteGotchiData[i]);
                    return remoteGotchiData[i];
                }
            }

            // now check local
            for (int i = 0; i < localWalletGotchiData.Count; i++)
            {
                if (id == localWalletGotchiData[i].id)
                {
                    Debug.Log("found local gotchi match: " + localWalletGotchiData[i]);
                    return localWalletGotchiData[i];
                }
            }

            // if got here we were passed invalid id
            //Debug.Log("Invalid id passed to GetGotchiDataById()");
            return null;
        }

        public DefaultGotchiData GetOffchainGotchiDataById(int id)
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
            Debug.Log("Invalid id passed to GetGotchiSvgsById()");
            return null;
        }

        public async UniTask FetchWalletGotchiData()
        {
            try
            {
                // clear all current data
                localWalletGotchiData.Clear();
                localWalletGotchiSvgSets.Clear();

                // get wallet address
                var walletAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                walletAddress = walletAddress.ToLower();

                // fetch gotchis with aavegotchi kit
                var userAccount = await graphManager.GetUserAccount(walletAddress);

                onFetchedLocalWalletGotchiCount?.Invoke();

                // save base gotchi data
                var gotchiIds = new List<string>();
                foreach (var gotchi in userAccount.gotchisOwned)
                {
                    localWalletGotchiData.Add(gotchi);
                    gotchiIds.Add(gotchi.id.ToString());
                }

                // get svgs
                var svgs = await graphManager.GetGotchiSvgs(gotchiIds);
                for (int i = 0; i < svgs.Count; i++)
                {
                    var svgSet = svgs[i];
                    localWalletGotchiSvgSets.Add(new GotchiSvgSet
                    {
                        id = int.Parse(gotchiIds[i]),
                        Front = svgSet.svg,
                        Back = svgSet.back,
                        Left = svgSet.left,
                        Right = svgSet.right,
                    });
                }

                gotchiIds.Clear();

                // default to highest brs gotchi
                if (localWalletGotchiData.Count > 0)
                {
                    SetSelectedGotchiById(GetGotchiIdByHighestBRS());

                    onFetchGotchiDataSuccess?.Invoke();
                }

            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public async UniTask<bool> FetchGotchiById(int id)
        {

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
