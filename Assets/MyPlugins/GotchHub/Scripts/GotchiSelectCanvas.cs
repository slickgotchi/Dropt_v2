using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using TMPro;

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
        public TextMeshProUGUI LoadingInfoText;
        public Button VisitAavegotchiButton;
        public Button ConnectButton;
        public Button ConfirmButton;

        [Header("Gotchi Avatar & Name Card")]
        public TextMeshProUGUI NameText;
        public SVGImage OnchainSvgImage;
        public Image OffchainImage;

        [Header("Gotchi Stats Card")]
        public TextMeshProUGUI HpTotalText;
        public TextMeshProUGUI HpGotchiText;
        public TextMeshProUGUI HpGearText;

        public TextMeshProUGUI AtkTotalText;
        public TextMeshProUGUI AtkGotchiText;
        public TextMeshProUGUI AtkGearText;

        public TextMeshProUGUI CritChanceTotalText;
        public TextMeshProUGUI CritChanceGotchiText;
        public TextMeshProUGUI CritChanceGearText;

        public TextMeshProUGUI ApTotalText;
        public TextMeshProUGUI ApGotchiText;
        public TextMeshProUGUI ApGearText;

        public TextMeshProUGUI DblStrikeTotalText;
        public TextMeshProUGUI DblStrikeGotchiText;
        public TextMeshProUGUI DblStrikeGearText;

        public TextMeshProUGUI CritDamageTotalText;
        public TextMeshProUGUI CritDamageGotchiText;
        public TextMeshProUGUI CritDamageGearText;

        public TextMeshProUGUI KinshipText;
        public TextMeshProUGUI LevelText;

        [Header("Wearable - Left Hand")]
        public TextMeshProUGUI LH_NameText;
        public TextMeshProUGUI LH_SubtitleText;
        public TextMeshProUGUI LH_HpText;
        public TextMeshProUGUI LH_AtkText;
        public TextMeshProUGUI LH_CritText;
        public TextMeshProUGUI LH_ApText;
        public SVGImage LH_OnchainSVGImage;
        public Outline LH_Outline;

        [Header("Wearable - Right Hand")]
        public TextMeshProUGUI RH_NameText;
        public TextMeshProUGUI RH_SubtitleText;
        public TextMeshProUGUI RH_HpText;
        public TextMeshProUGUI RH_AtkText;
        public TextMeshProUGUI RH_CritText;
        public TextMeshProUGUI RH_ApText;
        public SVGImage RH_OnchainSVGImage;
        public Outline RH_Outline;


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

            // reset wallet address
            m_walletAddress = "";

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

                    ConnectButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = address;

                    // fetch new gotchis due to different address
                    Debug.Log("Fetch gotchis");
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



        private void InitGotchiStatsCardById(int id)
        {
            HpTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.NRG).ToString("F0");
            HpGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.NRG).ToString("F0");
            HpGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.NRG).ToString("F0");

            AtkTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.AGG).ToString("F0");
            AtkGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.AGG).ToString("F0");
            AtkGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.AGG).ToString("F0");

            CritChanceTotalText.text = (DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.SPK)*100).ToString("F0");
            CritChanceGotchiText.text = (DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.SPK)*100).ToString("F0");
            CritChanceGearText.text = (DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.SPK)*100).ToString("F0");

            ApTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.BRN).ToString("F0");
            ApGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.BRN).ToString("F0");
            ApGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.BRN).ToString("F0");

            DblStrikeTotalText.text = (DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.EYS)*100).ToString("F0");
            DblStrikeGotchiText.text = (DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.EYS)*100).ToString("F0");
            DblStrikeGearText.text = (DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.EYS)*100).ToString("F0");

            CritDamageTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.EYC).ToString("F2");
            CritDamageGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.EYC).ToString("F2");
            CritDamageGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.EYC).ToString("F2");

            var gotchiData = m_gotchiDataManager.GetGotchiDataById(id);
            if (gotchiData != null)
            {
                KinshipText.text = gotchiData.kinship.ToString();
                LevelText.text = gotchiData.level.ToString();
                return;
            }

            var offchainGotchiData = m_gotchiDataManager.GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                KinshipText.text = offchainGotchiData.kinship.ToString();
                LevelText.text = offchainGotchiData.level.ToString();
                return;
            }
        }

        public void HighlightById(int id)
        {
            // Deselect all selections except our chosen
            for (int i = 0; i < gotchiList.transform.childCount; i++)
            {
                var gotchiSelectCard = gotchiList.transform.GetChild(i).GetComponent<GotchiSelectCard>();
                gotchiSelectCard.SetSelected(gotchiSelectCard.Id == id);
            }

            // Set up the left side of the canvas
            InitGotchiStatsCardById(id);
            InitGotchiAvatarById(id);
            InitGotchiWearablesById(id);
        }

        private void InitGotchiAvatarById(int id)
        {
            var gotchiSvg = GotchiDataManager.Instance.GetGotchiSvgsById(id);
            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);

            // if we got onchain data, set svg image
            if (gotchiData != null)
            {
                OnchainSvgImage.sprite = CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
                OnchainSvgImage.material = GotchiDataManager.Instance.Material_Unlit_VectorGradientUI;
                NameText.text = gotchiData.name + "(" + gotchiData.id + ")";
                OffchainImage.enabled = false;
                return;
            }

            // else lets get offchain data
            var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                OffchainImage.sprite = offchainGotchiData.spriteFront;
                OffchainImage.material = GotchiDataManager.Instance.Material_Sprite_Unlit_Default;
                NameText.text = offchainGotchiData.gotchiName + "(" + offchainGotchiData.id + ")";
                OnchainSvgImage.enabled = false;
                return;
            }
        }

        private void InitGotchiWearablesById(int id)
        {
            var lhId = 0;
            var rhId = 0;



            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
            if (gotchiData != null)
            {
                lhId = (int)gotchiData.equippedWearables[5];
                rhId = (int)gotchiData.equippedWearables[4];
            }

            var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                lhId = (int)offchainGotchiData.equippedWearables[5];
                rhId = (int)offchainGotchiData.equippedWearables[4];
            }

            // set all wearable cards
            SetWearableCard(lhId, id, LH_NameText, LH_SubtitleText, LH_HpText, LH_AtkText, LH_CritText, LH_ApText, LH_OnchainSVGImage, LH_Outline, true);
            SetWearableCard(rhId, id, RH_NameText, RH_SubtitleText, RH_HpText, RH_AtkText, RH_CritText, RH_ApText, RH_OnchainSVGImage, RH_Outline, false);

            // LEFT HAND
            //var lhWearable = WearableManager.Instance.GetWearable(lhId);

            //var hp = DroptStatCalculator.GetDroptStatByWearableId(lhId, TraitType.NRG);
            //var atk = DroptStatCalculator.GetDroptStatByWearableId(lhId, TraitType.AGG);
            //var crit = DroptStatCalculator.GetDroptStatByWearableId(lhId, TraitType.SPK);
            //var ap = DroptStatCalculator.GetDroptStatByWearableId(lhId, TraitType.BRN);

            //var damage = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.AGG) * lhWearable.RarityMultiplier * GetBaseAttackMultiplier(lhWearable.WeaponType);

            //LH_NameText.text = lhWearable.Name;
            //LH_SubtitleText.text = lhWearable.WeaponType.ToString() + " | " + "Left Hand" + " | " + damage.ToString("F0") + " Dmg";
            //LH_HpText.text = "+" + hp.ToString("F0") + " Health";
            //LH_AtkText.text = "+" + atk.ToString("F0") + " Attack";
            //LH_CritText.text = "+" + (crit*100).ToString("F1") + " Critical %";
            //LH_ApText.text = "+" + ap.ToString("F0") + " Ability Pts";
            //LH_OffchainImage.sprite = WeaponSpriteManager.Instance.GetSprite(lhWearable.NameType, PlayerGotchi.Facing.Right);

            //LH_HpText.color = hp <= 0.1f ? inactiveTextColor : activeTextColor;
            //LH_AtkText.color = atk <= 0.1f ? inactiveTextColor : activeTextColor;
            //LH_CritText.color = crit < 0.01f ? inactiveTextColor : activeTextColor;
            //LH_ApText.color = ap < 0.1f ? inactiveTextColor : activeTextColor;

            //// set border color
            //LH_Outline.effectColor = lhWearable.RarityColor;
        }

        public void SetWearableCard(int wearableId, int gotchiId, TextMeshProUGUI nameText, TextMeshProUGUI subtitleText,
            TextMeshProUGUI hpText, TextMeshProUGUI atkText, TextMeshProUGUI critText, TextMeshProUGUI apText, SVGImage onchainSvgImage, Outline outline, bool isLeftIfWeapon)
        {
            // set some text colorisations
            var activeTextColor = Dropt.Utils.Color.HexToColor("#ffffff");
            var inactiveTextColor = Dropt.Utils.Color.HexToColor("#b4b4b4");

            // get wearable
            var wearable = WearableManager.Instance.GetWearable(wearableId);
            var isWeapon = wearable.Slot == Wearable.SlotEnum.Hand;

            var hp = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.NRG);
            var atk = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.AGG);
            var crit = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.SPK);
            var ap = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.BRN);

            var damage = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(gotchiId, TraitType.AGG) * wearable.RarityMultiplier * GetBaseAttackMultiplier(wearable.WeaponType);

            nameText.text = isWeapon ? wearable.Name : wearable.Name + " | " + wearable.Slot;
            if (isWeapon) subtitleText.text = wearable.WeaponType.ToString() + " | " + (isLeftIfWeapon ? "Left Hand" : "Right Hand") + " | " + damage.ToString("F0") + " Dmg";
            hpText.text = "+" + hp.ToString("F0") + " Health";
            atkText.text = "+" + atk.ToString("F0") + " Attack";
            critText.text = "+" + (crit * 100).ToString("F1") + " Critical %";
            apText.text = "+" + ap.ToString("F0") + " Ability Pts";
            //LH_OffchainImage.sprite = WeaponSpriteManager.Instance.GetSprite(lhWearable.NameType, PlayerGotchi.Facing.Right);

            hpText.color = hp <= 0.1f ? inactiveTextColor : activeTextColor;
            atkText.color = atk <= 0.1f ? inactiveTextColor : activeTextColor;
            critText.color = crit < 0.01f ? inactiveTextColor : activeTextColor;
            apText.color = ap < 0.1f ? inactiveTextColor : activeTextColor;

            // set border color
            LH_Outline.effectColor = wearable.RarityColor;
        }

        public float GetBaseAttackMultiplier(Wearable.WeaponTypeEnum weaponType)
        {
            switch (weaponType)
            {
                case Wearable.WeaponTypeEnum.Unarmed:
                    var unarmedPunch = FindAnyObjectByType<UnarmedPunch>();
                    return unarmedPunch != null ? unarmedPunch.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Cleave:
                    var cleaveSlash = FindAnyObjectByType<CleaveSlash>();
                    return cleaveSlash != null ? cleaveSlash.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Smash:
                    var smashSwipe = FindAnyObjectByType<SmashSwipe>();
                    return smashSwipe != null ? smashSwipe.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Pierce:
                    var pierceThrust = FindAnyObjectByType<PierceThrust>();
                    return pierceThrust != null ? pierceThrust.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Ballistic:
                    var ballisticShot = FindAnyObjectByType<BallisticShot>();
                    return ballisticShot != null ? ballisticShot.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Magic:
                    var magicCast = FindAnyObjectByType<MagicCast>();
                    return magicCast != null ? magicCast.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Splash:
                    var splashLob = FindAnyObjectByType<SplashLob>();
                    return splashLob != null ? splashLob.DamageMultiplier : 1;
                case Wearable.WeaponTypeEnum.Shield:
                    var shieldBash = FindAnyObjectByType<ShieldBash>();
                    return shieldBash != null ? shieldBash.DamageMultiplier : 1;
                default: break;
            }

            return 1;
        }

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