using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;

namespace GotchiHub
{
    public class GotchiSelectCanvas : DroptCanvas
    {
        public static GotchiSelectCanvas Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject gotchiSelectCard;

        [Header("Child Object References")]
        public GameObject gotchiList;
        //public SVGImage AvatarSvgImage;
        //public GotchiStatsCard GotchiStatsCard;

        [Header("Menus")]
        public GameObject GotchiSelect_Menu;
        public GameObject GotchiSelect_Loading;
        public GameObject GotchiSelect_NoGotchis;
        public GameObject GotchiSelect_NotConnected;

        [Header("Menu Items")]
        public TMPro.TextMeshProUGUI LoadingInfoText;
        public TMPro.TextMeshProUGUI WalletInfoText;
        public Button VisitAavegotchiButton;
        public Button ConnectButton;
        public Button ConfirmButton;

        // private variables
        private GotchiDataManager m_gotchiDataManager;
        private string m_walletAddress = "";

        public enum ReorganizeMethod
        {
            BRSLowToHigh,
            BRSHighToLow,
            IdLowToHigh,
            IdHighToLow
        }

        public enum MenuScreen
        {
            NotConnected,
            Loading,
            GotchiSelect,
            NoGotchis,
            Hidden,
        }
        private MenuScreen m_menuScreen = MenuScreen.NotConnected;

        private void Awake()
        {
            Instance = this;
            VisitAavegotchiButton.onClick.AddListener(HandleOnClick_VisitAavegotchiButton);
            ConfirmButton.onClick.AddListener(() => HideCanvas());
            HideCanvas();
        }

        private void Start()
        {
            if (GotchiDataManager.Instance == null)
            {
                Debug.LogError("A GotchiDataManager must be available in the scene");
                return;
            }

            // get the gotchi data manager
            m_gotchiDataManager = GotchiDataManager.Instance;

            HideAllMenus();

            // Clear out gotchi list children
            ClearGotchiListChildren();

            // sign up to onFetchData success function
            m_gotchiDataManager.onFetchGotchiDataSuccess += HandleOnFetchGotchiDataSuccess;
        }

        public void SetMenuScreen(MenuScreen menuScreen)
        {
            HideAllMenus();

            switch (menuScreen)
            {
                case MenuScreen.NotConnected: GotchiSelect_NotConnected.SetActive(true); break;
                case MenuScreen.Loading: GotchiSelect_Loading.SetActive(true); break;
                case MenuScreen.GotchiSelect: GotchiSelect_Menu.SetActive(true); break;
                case MenuScreen.NoGotchis: GotchiSelect_NoGotchis.SetActive(true); break;
                default: break;
            }

            m_menuScreen = menuScreen;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            VisitAavegotchiButton.onClick.RemoveListener(HandleOnClick_VisitAavegotchiButton);
            m_gotchiDataManager.onFetchGotchiDataSuccess -= HandleOnFetchGotchiDataSuccess;
        }

        public override void OnShowCanvas()
        {
            UpdateGotchiList();
        }

        void HandleOnClick_VisitAavegotchiButton()
        {
            Application.OpenURL("https://dapp.aavegotchi.com/baazaar/aavegotchis");

        }

        void HandleOnFetchGotchiDataSuccess()
        {
            UpdateGotchiList();
        }

        private float k_updateInterval = 0.3f;
        private float m_updateTimer = 0f;

        protected async override void Update()
        {
            base.Update();

            if (!IsActive()) return;

            if (IsInputActionSelectPressed())
            {
                ThirdwebCanvas.Instance.HideCanvas();
                PlayerInputMapSwitcher.Instance.SwitchToInGame();
                HideCanvas();
                return;
            }

            m_updateTimer -= Time.deltaTime;
            if (m_updateTimer > 0) return;
            m_updateTimer = k_updateInterval;


            try
            {
                // connection check
                var isConnected = await ThirdwebManager.Instance.SDK.Wallet.IsConnected();
                if (!isConnected)
                {
                    SetMenuScreen(MenuScreen.NotConnected);
                    return;
                }


                // address check
                var address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                if (address != m_walletAddress)
                {
                    m_walletAddress = address;
                    WalletInfoText.text = m_walletAddress;

                    // fetch new gotchis due to different address
                    await m_gotchiDataManager.FetchWalletGotchiData();
                }

                // show screen depending on gotchi count
                var numGotchis = m_gotchiDataManager.localGotchiData.Count;
                if (numGotchis <= 0)
                {
                    SetMenuScreen(MenuScreen.NoGotchis);
                } else
                {
                    SetMenuScreen(MenuScreen.GotchiSelect);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        void HideAllMenus()
        {
            GotchiSelect_Loading.SetActive(false);
            GotchiSelect_Menu.SetActive(false);
            GotchiSelect_NoGotchis.SetActive(false);
            GotchiSelect_NotConnected.SetActive(false);
        }



        //private void InitAvatarById(int id)
        //{
        //    var gotchiSvg = m_gotchiDataManager.GetGotchiSvgsById(id);
        //    if (gotchiSvg == null) return;

        //    AvatarSvgImage.sprite =
        //        CustomSvgLoader.CreateSvgSprite(m_gotchiDataManager.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
        //    AvatarSvgImage.material = m_gotchiDataManager.Material_Unlit_VectorGradientUI;

        //    GotchiStatsCard.UpdateStatsCard();
        //}

        //public void HighlightById(int id)
        //{
        //    var gotchiData = m_gotchiDataManager.GetGotchiDataById(id);
        //    if (gotchiData == null) return;

        //    // Deselect all selections except our chosen
        //    for (int i = 0; i < gotchiList.transform.childCount; i++)
        //    {
        //        var listItem = gotchiList.transform.GetChild(i).GetComponent<GotchiSelectListItem>();
        //        listItem.SetSelected(listItem.Id == id);
        //    }

        //    // Set our avatar
        //    InitAvatarById(id);
        //}

        public void UpdateGotchiList()
        {
            // 1. clear existing GotchiSelectCard's
            ClearGotchiListChildren();

            // 2. add offchain gotchis as cards
            foreach (var offchainGotchi in GotchiDataManager.Instance.offchainGotchiData)
            {
                var newGotchiSelectCard = Instantiate(gotchiSelectCard).GetComponent<GotchiSelectCard>();
                if (newGotchiSelectCard != null)
                {
                    newGotchiSelectCard.transform.SetParent(gotchiList.transform, false);
                    newGotchiSelectCard.InitById(offchainGotchi.id);
                }
            }

            // 3. reate new instance of gotchi list item and set parent to gotchi list
            var gotchiSvgs = m_gotchiDataManager.localGotchiSvgSets;
            var gotchiData = m_gotchiDataManager.localGotchiData;
            for (int i = 0; i < gotchiSvgs.Count; i++)
            {
                var newGotchiSelectCard = Instantiate(gotchiSelectCard).GetComponent<GotchiSelectCard>();
                if (newGotchiSelectCard != null)
                {
                    newGotchiSelectCard.transform.SetParent(gotchiList.transform, false);
                    newGotchiSelectCard.InitById(gotchiData[i].id);
                }
            }

            // Organize list by BRS
            ReorganizeList(ReorganizeMethod.BRSHighToLow); // Example usage, you can change the method as needed
            //HighlightById(m_gotchiDataManager.GetSelectedGotchiId());

        }

        void ClearGotchiListChildren()
        {
            foreach (Transform child in gotchiList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        //void ClearGotchiListChildren()
        //{
        //    // Create a list of children to destroy
        //    List<GameObject> children = new List<GameObject>();
        //    for (int i = 0; i < gotchiList.transform.childCount; i++)
        //    {
        //        children.Add(gotchiList.transform.GetChild(i).gameObject);
        //    }

        //    // Destroy all children
        //    foreach (var child in children)
        //    {
        //        Destroy(child);
        //    }

        //    children.Clear();
        //}

        public void ReorganizeList(ReorganizeMethod method)
        {
            List<GotchiSelectCard> gotchiSelectCards = new List<GotchiSelectCard>();

            // Get all GotchiSelect_ListItem components under the parent
            foreach (Transform child in gotchiList.transform)
            {
                GotchiSelectCard gotchiSelectCard = child.GetComponent<GotchiSelectCard>();
                if (gotchiSelectCard != null)
                {
                    gotchiSelectCards.Add(gotchiSelectCard);
                }
            }

            // Sort the list based on the selected method
            switch (method)
            {
                case ReorganizeMethod.BRSLowToHigh:
                    gotchiSelectCards.Sort((item1, item2) => item1.BRS.CompareTo(item2.BRS));
                    break;
                case ReorganizeMethod.BRSHighToLow:
                    gotchiSelectCards.Sort((item1, item2) => item2.BRS.CompareTo(item1.BRS));
                    break;
                case ReorganizeMethod.IdLowToHigh:
                    gotchiSelectCards.Sort((item1, item2) => item1.Id.CompareTo(item2.Id));
                    break;
                case ReorganizeMethod.IdHighToLow:
                    gotchiSelectCards.Sort((item1, item2) => item2.Id.CompareTo(item1.Id));
                    break;
            }

            // Reorder the child transforms based on the sorted list
            for (int i = 0; i < gotchiSelectCards.Count; i++)
            {
                gotchiSelectCards[i].transform.SetSiblingIndex(i);
            }

        }
    }
}