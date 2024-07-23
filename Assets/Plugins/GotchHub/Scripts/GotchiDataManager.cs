using PortalDefender.AavegotchiKit;
using PortalDefender.AavegotchiKit.GraphQL;
using System;
using System.Collections;
using System.Collections.Generic;
using Thirdweb;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

        [HideInInspector] public List<GotchiData> gotchiData = new List<GotchiData>();
        [HideInInspector] public List<GotchiSvgSet> gotchiSvgSets = new List<GotchiSvgSet>();
        [HideInInspector] public List<string> gotchiIds = new List<string>();

        private int m_selectedGotchiId = 0;
        public int GetSelectedGotchiId() { return m_selectedGotchiId; }

        [HideInInspector] public string StatusString = "";

        // Event declaration
        public event Action<int> onSelectedGotchi;
        public event Action onFetchGotchiDataSuccess;

        private void Awake()
        {
            Instance = this;
        }

        public bool SetSelectedGotchiById(int id)
        {
            for (int i = 0; i < gotchiData.Count; i++)
            {
                if (id == gotchiData[i].id)
                {
                    m_selectedGotchiId = id;
                    onSelectedGotchi?.Invoke(m_selectedGotchiId); // Trigger event
                    return true;
                }
            }

            Debug.Log("Gotchi with id " + id + " does not exist in GotchiDataManager");
            return false;
        }

        private int GetGotchiIndexById(int id)
        {
            for (int i = 0; i < gotchiData.Count; i++)
            {
                if (id == gotchiData[i].id)
                {
                    return i;
                }
            }

            return -1;
        }

        public GotchiData GetGotchiDataById(int id)
        {
            var index = GetGotchiIndexById(id);
            if (index < 0)
            {
                Debug.Log("Invalid id passed to GetGotchiDataById()");
                return null;
            }

            return gotchiData[index];
        }

        public GotchiSvgSet GetGotchiSvgsById(int id)
        {
            var index = GetGotchiIndexById(id);
            if (index < 0)
            {
                Debug.Log("Invalid id passed to GetGotchSvgsById()");
                return null;
            }

            return gotchiSvgSets[index];
        }

        public async UniTask FetchGotchiData()
        {
            try
            {
                // clear all current data
                gotchiData.Clear();
                gotchiSvgSets.Clear();
                gotchiIds.Clear();

                // get wallet address
                StatusString = "Validating wallet address...";
                var walletAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                walletAddress = walletAddress.ToLower();


                // fetch gotchis with aavegotchi kit
                StatusString = "Wallet validated. Fetching gotchi data...";
                var userAccount = await graphManager.GetUserAccount(walletAddress);

                // save base gotchi data
                foreach (var gotchi in userAccount.gotchisOwned)
                {
                    gotchiData.Add(gotchi);
                    gotchiIds.Add(gotchi.id.ToString());
                }

                // get svgs
                StatusString = userAccount.gotchisOwned.Length.ToString() + " gotchis found. Processing SVGs...";
                var svgs = await graphManager.GetGotchiSvgs(gotchiIds);
                foreach (var svgSet in svgs)
                {
                    gotchiSvgSets.Add(new GotchiSvgSet
                    {
                        Front = svgSet.svg,
                        Back = svgSet.back,
                        Left = svgSet.left,
                        Right = svgSet.right,
                    });
                }

                // default to highest brs gotchi
                StatusString = "SVGs processed. Updating gotchi inventory...";
                if (gotchiData.Count > 0)
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

        public int GetGotchiIdByHighestBRS()
        {
            int highestBrs = 0;
            int highestBrsId = -1;

            for (int i = 0; i < gotchiData.Count; i++)
            {
                var brs = DroptStatCalculator.GetBRS(gotchiData[i].numericTraits);
                if (brs > highestBrs)
                {
                    highestBrs = brs;
                    highestBrsId = gotchiData[i].id;
                }
            }

            return highestBrsId;
        }
    }

    public class GotchiSvgSet
    {
        public string Front;
        public string Back;
        public string Left;
        public string Right;
    }

    public class GotchiSpriteSet
    {
        public Sprite Front;
        public Sprite Back;
        public Sprite Left;
        public Sprite Right;
    }
}
