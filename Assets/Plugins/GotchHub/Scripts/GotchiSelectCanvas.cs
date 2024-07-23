using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using GotchiHub;

namespace GotchiHub
{
    public class GotchiSelectCanvas : MonoBehaviour
    {
        public static GotchiSelectCanvas Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject gotchiListItemPrefab;

        [Header("Child Object References")]
        public Button SelectGotchiButton;
        public GameObject gotchiList;
        public SVGImage AvatarSvgImage;
        public GotchiStatsCard GotchiStatsCard;

        [Header("Menus")]
        public GameObject GotchiSelect_Menu;
        public GameObject GotchiSelect_Loading;
        public GameObject GotchiSelect_NoGotchis;
        public GameObject GotchiSelect_NotConnected;

        [Header("Menu Items")]
        public TMPro.TextMeshProUGUI LoadingInfoText;
        public TMPro.TextMeshProUGUI WalletInfoText;
        public Button VisitAavegotchiButton;

        // private variables
        private GotchiDataManager m_gotchiDataManager;
        private string m_walletAddress = "";
        private bool m_isShowButtonJustClicked = false;

        public enum ReorganizeMethod
        {
            BRSLowToHigh,
            BRSHighToLow,
            IdLowToHigh,
            IdHighToLow
        }

        private void Awake()
        {
            Instance = this;
            SelectGotchiButton.onClick.AddListener(HandleOnClick_GotchiSelect_ShowButton);
            VisitAavegotchiButton.onClick.AddListener(HandleOnClick_VisitAavegotchiButton);
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

        private void OnDestroy()
        {
            // Unsubscribe from events
            SelectGotchiButton.onClick.RemoveListener(HandleOnClick_GotchiSelect_ShowButton);
            VisitAavegotchiButton.onClick.RemoveListener(HandleOnClick_VisitAavegotchiButton);
            m_gotchiDataManager.onFetchGotchiDataSuccess -= HandleOnFetchGotchiDataSuccess;
        }


        void HandleOnClick_VisitAavegotchiButton()
        {
            Application.OpenURL("https://dapp.aavegotchi.com/baazaar/aavegotchis");
        }

        void HandleOnFetchGotchiDataSuccess()
        {
            UpdateGotchiList();
        }

        async void HandleOnClick_GotchiSelect_ShowButton()
        {
            try
            {
                // Show loading
                GotchiSelect_Loading.SetActive(true);
                GotchiSelect_Menu.SetActive(false);

                var connected = await ThirdwebManager.Instance.SDK.Wallet.IsConnected();
                if (!connected)
                {
                    HideAllMenus();
                    GotchiSelect_NotConnected.SetActive(true);
                }

                var address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

                if (address != m_walletAddress)
                {
                    m_walletAddress = address;
                    WalletInfoText.text = m_walletAddress;
                    await m_gotchiDataManager.FetchGotchiData();

                    if (m_gotchiDataManager.gotchiData.Count > 0)
                    {
                        // Highlight the highest brs gotchi
                        HighlightById(m_gotchiDataManager.GetSelectedGotchiId());
                    }
                }

                // Show menu
                HideAllMenus();
                GotchiSelect_Menu.SetActive(m_gotchiDataManager.gotchiData.Count > 0);
                GotchiSelect_NoGotchis.SetActive(m_gotchiDataManager.gotchiData.Count <= 0);

            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            m_isShowButtonJustClicked = true;
        }

        private void Update()
        {
            // Check for clicks outside menu box
            if (Input.GetMouseButtonDown(0) && !m_isShowButtonJustClicked)
            {
                RectTransform menuRect = null;
                if (GotchiSelect_Menu.activeSelf) menuRect = GotchiSelect_Menu.GetComponent<RectTransform>();
                if (GotchiSelect_Loading.activeSelf) menuRect = GotchiSelect_Loading.GetComponent<RectTransform>();
                if (GotchiSelect_NoGotchis.activeSelf) menuRect = GotchiSelect_NoGotchis.GetComponent<RectTransform>();
                if (GotchiSelect_NotConnected.activeSelf) menuRect = GotchiSelect_NotConnected.GetComponent<RectTransform>();

                if (menuRect != null)
                {
                    // Hide menus
                    if (!RectTransformUtility.RectangleContainsScreenPoint(menuRect, Input.mousePosition))
                    {
                        HideAllMenus();
                    }
                }
            }

            m_isShowButtonJustClicked = false;

            // update loading info text (if its active)
            if (LoadingInfoText.enabled)
            {
                LoadingInfoText.text = m_gotchiDataManager.StatusString;
            }
        }

        void HideAllMenus()
        {
            GotchiSelect_Loading.SetActive(false);
            GotchiSelect_Menu.SetActive(false);
            GotchiSelect_NoGotchis.SetActive(false);
            GotchiSelect_NotConnected.SetActive(false);
        }

        private void InitAvatarById(int id)
        {
            var gotchiSvg = m_gotchiDataManager.GetGotchiSvgsById(id);
            if (gotchiSvg == null) return;

            AvatarSvgImage.sprite =
                CustomSvgLoader.CreateSvgSprite(m_gotchiDataManager.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
            AvatarSvgImage.material = m_gotchiDataManager.Material_Unlit_VectorGradientUI;

            GotchiStatsCard.UpdateStatsCard();
        }

        public void HighlightById(int id)
        {
            var gotchiData = m_gotchiDataManager.GetGotchiDataById(id);
            if (gotchiData == null) return;

            // Deselect all selections except our chosen
            for (int i = 0; i < gotchiList.transform.childCount; i++)
            {
                var listItem = gotchiList.transform.GetChild(i).GetComponent<GotchiSelectListItem>();
                listItem.SetSelected(listItem.Id == id);
            }

            // Set our avatar
            InitAvatarById(id);
        }

        public void UpdateGotchiList()
        {
            // Clear out existing children
            ClearGotchiListChildren();

            // Create new instance of gotchi list item and set parent to gotchi list
            var gotchiSvgs = m_gotchiDataManager.gotchiSvgSets;
            var gotchiData = m_gotchiDataManager.gotchiData;
            for (int i = 0; i < gotchiSvgs.Count; i++)
            {
                var newListItem = Instantiate(gotchiListItemPrefab);
                newListItem.transform.SetParent(gotchiList.transform, false);

                var listItem = newListItem.GetComponent<GotchiSelectListItem>();
                listItem.InitById(gotchiData[i].id);
            }

            // Organize list by BRS
            ReorganizeList(ReorganizeMethod.BRSHighToLow); // Example usage, you can change the method as needed
            HighlightById(m_gotchiDataManager.GetSelectedGotchiId());

        }

        void ClearGotchiListChildren()
        {
            // Create a list of children to destroy
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < gotchiList.transform.childCount; i++)
            {
                children.Add(gotchiList.transform.GetChild(i).gameObject);
            }

            // Destroy all children
            foreach (var child in children)
            {
                Destroy(child);
            }

            children.Clear();
        }

        public void ReorganizeList(ReorganizeMethod method)
        {
            List<GotchiSelectListItem> gotchiListItems = new List<GotchiSelectListItem>();

            // Get all GotchiSelect_ListItem components under the parent
            foreach (Transform child in gotchiList.transform)
            {
                GotchiSelectListItem listItem = child.GetComponent<GotchiSelectListItem>();
                if (listItem != null)
                {
                    gotchiListItems.Add(listItem);
                }
            }

            // Sort the list based on the selected method
            switch (method)
            {
                case ReorganizeMethod.BRSLowToHigh:
                    gotchiListItems.Sort((item1, item2) => item1.BRS.CompareTo(item2.BRS));
                    break;
                case ReorganizeMethod.BRSHighToLow:
                    gotchiListItems.Sort((item1, item2) => item2.BRS.CompareTo(item1.BRS));
                    break;
                case ReorganizeMethod.IdLowToHigh:
                    gotchiListItems.Sort((item1, item2) => item1.Id.CompareTo(item2.Id));
                    break;
                case ReorganizeMethod.IdHighToLow:
                    gotchiListItems.Sort((item1, item2) => item2.Id.CompareTo(item1.Id));
                    break;
            }

            // Reorder the child transforms based on the sorted list
            for (int i = 0; i < gotchiListItems.Count; i++)
            {
                gotchiListItems[i].transform.SetSiblingIndex(i);
            }

            Debug.Log("Gotchi list has been reorganized by " + method.ToString());
        }
    }
}