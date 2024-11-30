using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using TMPro;
using Cysharp.Threading.Tasks;
using Thirdweb.Unity;

namespace GotchiHub
{
    public class GotchiSelectCanvas : DroptCanvas
    {
        public static GotchiSelectCanvas Instance { get; private set; }

        [HideInInspector] public Interactable interactable;

        [Header("Prefabs")]
        public GameObject gotchiSelectCard;

        [Header("Child Object References")]
        public GameObject gotchiList;

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

        [Header("Wallet Card")]
        public TextMeshProUGUI ghstBalance;

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
        public Image LH_OffchainImage;
        public Outline LH_Outline;

        [Header("Wearable - Right Hand")]
        public TextMeshProUGUI RH_NameText;
        public TextMeshProUGUI RH_SubtitleText;
        public TextMeshProUGUI RH_HpText;
        public TextMeshProUGUI RH_AtkText;
        public TextMeshProUGUI RH_CritText;
        public TextMeshProUGUI RH_ApText;
        public SVGImage RH_OnchainSVGImage;
        public Image RH_OffchainImage;
        public Outline RH_Outline;

        [Header("Wearable - Body")]
        public TextMeshProUGUI Body_NameText;
        public TextMeshProUGUI Body_HpText;
        public TextMeshProUGUI Body_AtkText;
        public TextMeshProUGUI Body_CritText;
        public TextMeshProUGUI Body_ApText;
        public SVGImage Body_OnchainSVGImage;
        public Outline Body_Outline;

        [Header("Wearable - Face")]
        public TextMeshProUGUI Face_NameText;
        public TextMeshProUGUI Face_HpText;
        public TextMeshProUGUI Face_AtkText;
        public TextMeshProUGUI Face_CritText;
        public TextMeshProUGUI Face_ApText;
        public SVGImage Face_OnchainSVGImage;
        public Outline Face_Outline;

        [Header("Wearable - Eyes")]
        public TextMeshProUGUI Eyes_NameText;
        public TextMeshProUGUI Eyes_HpText;
        public TextMeshProUGUI Eyes_AtkText;
        public TextMeshProUGUI Eyes_CritText;
        public TextMeshProUGUI Eyes_ApText;
        public SVGImage Eyes_OnchainSVGImage;
        public Outline Eyes_Outline;

        [Header("Wearable - Head")]
        public TextMeshProUGUI Head_NameText;
        public TextMeshProUGUI Head_HpText;
        public TextMeshProUGUI Head_AtkText;
        public TextMeshProUGUI Head_CritText;
        public TextMeshProUGUI Head_ApText;
        public SVGImage Head_OnchainSVGImage;
        public Outline Head_Outline;

        [Header("Wearable - Pet")]
        public TextMeshProUGUI Pet_NameText;
        public TextMeshProUGUI Pet_HpText;
        public TextMeshProUGUI Pet_AtkText;
        public TextMeshProUGUI Pet_CritText;
        public TextMeshProUGUI Pet_ApText;
        public SVGImage Pet_OnchainSVGImage;
        public Outline Pet_Outline;

        private string polygonGHSTAddress = "0x385eeac5cb85a38a9a07a70c73e0a3271cfb54a7";

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
            Connected,
            NoGotchis,
            Hidden,
        }
        private MenuScreen m_menuScreen = MenuScreen.NotConnected;

        private void Awake()
        {
            Instance = this;
            VisitAavegotchiButton.onClick.AddListener(HandleOnClick_VisitAavegotchiButton);
            ConfirmButton.onClick.AddListener(ClickOnConfirm);
            ConnectButton.onClick.AddListener(ClickOnConnect);
            InstaHideCanvas();
        }

        public void ClickOnConfirm()
        {
            GotchiSelectCanvas.Instance.HideCanvas();
            if (interactable != null)
            {
                PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                    interactable.interactableType);
            }
        }

        public void ClickOnConnect()
        {
            var walletOptions = new WalletOptions(provider: WalletProvider.WalletConnectWallet, chainId: 137);

            ThirdwebManager.Instance.ConnectWallet(walletOptions);
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
            m_gotchiDataManager.onFetchedLocalWalletGotchiCount += HandleOnFetchedLocalWalletGotchiCount;
            m_gotchiDataManager.onFetchedWalletGotchiSVG += HandleOnFetchedWalletGotchiSVG;
        }

        public void SetMenuScreen(MenuScreen menuScreen)
        {
            HideAllMenus();

            switch (menuScreen)
            {
                case MenuScreen.NotConnected: GotchiSelect_NotConnected.SetActive(true); break;
                case MenuScreen.Loading: GotchiSelect_Loading.SetActive(true); break;
                case MenuScreen.Connected: GotchiSelect_Menu.SetActive(true); break;
                case MenuScreen.NoGotchis: GotchiSelect_NoGotchis.SetActive(true); break;
                default: break;
            }

            m_menuScreen = menuScreen;
        }

        private float m_updateGHSTimer = 0f;

        private async void UpdateGHSTBalance()
        {
            m_updateGHSTimer -= Time.deltaTime;
            if (m_updateGHSTimer > 0) return;
            m_updateGHSTimer = 10f;

            try
            {
                var wallet = ThirdwebManager.Instance.GetActiveWallet();
                if (wallet == null) return;

                // get ghst balance
                var bal = await wallet.GetBalance(137, polygonGHSTAddress);
                ghstBalance.text = ((float)bal / 1e18).ToString("F2");
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            VisitAavegotchiButton.onClick.RemoveListener(HandleOnClick_VisitAavegotchiButton);
            if (m_gotchiDataManager != null) m_gotchiDataManager.onFetchGotchiDataSuccess -= HandleOnFetchGotchiDataSuccess;
        }

        public override void OnShowCanvas()
        {
            UpdateGotchiList();
            SelectById(GotchiDataManager.Instance.GetSelectedGotchiId());
        }

        void HandleOnClick_VisitAavegotchiButton()
        {
            Application.OpenURL("https://dapp.aavegotchi.com/baazaar/aavegotchis");

        }

        void HandleOnFetchGotchiDataSuccess()
        {
            //UpdateGotchiList();
        }

        private int m_totalGotchiCount = 0;
        private int m_currentGotchiCount = 0;

        async void HandleOnFetchedLocalWalletGotchiCount()
        {
            await UniTask.DelayFrame(1);

            m_totalGotchiCount = GotchiDataManager.Instance.localWalletGotchiData.Count;
            LoadingInfoText.text = $"Loading Gotchis: {m_currentGotchiCount} / {m_totalGotchiCount}";
            Debug.Log("Gotchi user count: " + m_totalGotchiCount);
        }

        async void HandleOnFetchedWalletGotchiSVG()
        {
            await UniTask.DelayFrame(1);

            m_currentGotchiCount = GotchiDataManager.Instance.localWalletGotchiSvgSets.Count;
            LoadingInfoText.text = $"Loading Gotchis: {m_currentGotchiCount} / {m_totalGotchiCount}";
            Debug.Log("Gotchi SVG count: " + m_currentGotchiCount);

            if (m_currentGotchiCount >= m_totalGotchiCount)
            {
                UpdateGotchiList();
            }
        }

        private float k_updateInterval = 0.3f;
        private float m_updateTimer = 0f;

        private bool m_isFetching = false;

        public async override void OnUpdate()
        {
            base.OnUpdate();

            if (!GotchiSelectCanvas.Instance.isCanvasOpen) return;
            if (m_isFetching) return;

            UpdateGHSTBalance();

            m_updateTimer -= Time.deltaTime;
            if (m_updateTimer > 0) return;
            m_updateTimer = k_updateInterval;

            try
            {
                // get current wallet
                var wallet = ThirdwebManager.Instance.GetActiveWallet();
                if (wallet == null)
                {
                    SetMenuScreen(MenuScreen.NotConnected);
                    return;
                }
                
                // connection check
                var isConnected = await wallet.IsConnected();
                if (!isConnected)
                {
                    SetMenuScreen(MenuScreen.NotConnected);
                    return;
                }

                // address check
                var address = await wallet.GetAddress();
                if (address != m_walletAddress)
                {
                    m_walletAddress = address;

                    ConnectButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = address;
                    ConnectButton.GetComponentInChildren<Image>().color = Dropt.Utils.Color.HexToColor("#d3fc7e");
                    ConnectButton.GetComponentInChildren<TextEllipsisInMiddle>().UpdateTextWithEllipsis();

                    SetMenuScreen(MenuScreen.Loading);

                    // fetch new gotchis due to different address
                    m_isFetching = true;
                    await m_gotchiDataManager.FetchWalletGotchiData();
                    m_isFetching = false;
                }

                // show screen depending on gotchi count
                var numGotchis = m_gotchiDataManager.localWalletGotchiData.Count;
                if (numGotchis <= 0)
                {
                    SetMenuScreen(MenuScreen.NoGotchis);
                } else
                {
                    SetMenuScreen(MenuScreen.Connected);
                }
                
            }
            catch (System.Exception e)
            {
                //Debug.Log(e);
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

            CritChanceTotalText.text = (DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.SPK)*100).ToString("F1");
            CritChanceGotchiText.text = (DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.SPK)*100).ToString("F1");
            CritChanceGearText.text = (DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.SPK)*100).ToString("F1");

            ApTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.BRN).ToString("F0");
            ApGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.BRN).ToString("F0");
            ApGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.BRN).ToString("F0");

            DblStrikeTotalText.text = (DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.EYS)*100).ToString("F1");
            DblStrikeGotchiText.text = (DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.EYS)*100).ToString("F1");
            DblStrikeGearText.text = (DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.EYS)*100).ToString("F1");

            CritDamageTotalText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.EYC).ToString("F2") + "x";
            CritDamageGotchiText.text = DroptStatCalculator.GetDroptStatForGotchiByGotchiId(id, TraitType.EYC).ToString("F2") + "x";
            CritDamageGearText.text = DroptStatCalculator.GetDroptStatForAllWearablesByGotchiId(id, TraitType.EYC).ToString("F2") + "x";

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

        public void SelectById(int id)
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
                OnchainSvgImage.enabled = true;
                OnchainSvgImage.sprite = CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
                OnchainSvgImage.material = GotchiDataManager.Instance.Material_Unlit_VectorGradientUI;
                NameText.text = gotchiData.name + " (" + gotchiData.id + ")";
                OffchainImage.enabled = false;
                return;
            }

            // else lets get offchain data
            var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                OffchainImage.enabled = true;
                OffchainImage.sprite = offchainGotchiData.spriteFront;
                OffchainImage.material = GotchiDataManager.Instance.Material_Sprite_Unlit_Default;
                NameText.text = offchainGotchiData.gotchiName + " (" + offchainGotchiData.id + ")";
                OnchainSvgImage.enabled = false;
                return;
            }
        }

        private void InitGotchiWearablesById(int id)
        {
            // 0 = body, 1 = face, 2 = eyes, 3 = head, 4 = r hand, 5 = l hand, 6 = pet
            var bodyId = 0;
            var faceId = 0;
            var eyesId = 0;
            var headId = 0;
            var rhId = 0;
            var lhId = 0;
            var petId = 0;


            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
            if (gotchiData != null)
            {
                bodyId = (int)gotchiData.equippedWearables[0];
                faceId = (int)gotchiData.equippedWearables[1];
                eyesId = (int)gotchiData.equippedWearables[2];
                headId = (int)gotchiData.equippedWearables[3];
                rhId = (int)gotchiData.equippedWearables[4];
                lhId = (int)gotchiData.equippedWearables[5];
                petId = (int)gotchiData.equippedWearables[6];
            }

            var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
            if (offchainGotchiData != null)
            {
                bodyId = (int)offchainGotchiData.equippedWearables[0];
                faceId = (int)offchainGotchiData.equippedWearables[1];
                eyesId = (int)offchainGotchiData.equippedWearables[2];
                headId = (int)offchainGotchiData.equippedWearables[3];
                rhId = (int)offchainGotchiData.equippedWearables[4];
                lhId = (int)offchainGotchiData.equippedWearables[5];
                petId = (int)offchainGotchiData.equippedWearables[6];
            }

            // set all wearable cards
            SetWearableCard(bodyId, id, Body_NameText, null, Body_HpText, Body_AtkText, Body_CritText, Body_ApText, Body_OnchainSVGImage, Body_Outline, Wearable.SlotEnum.Body);
            SetWearableCard(faceId, id, Face_NameText, null, Face_HpText, Face_AtkText, Face_CritText, Face_ApText, Face_OnchainSVGImage, Face_Outline, Wearable.SlotEnum.Face);
            SetWearableCard(eyesId, id, Eyes_NameText, null, Eyes_HpText, Eyes_AtkText, Eyes_CritText, Eyes_ApText, Eyes_OnchainSVGImage, Eyes_Outline, Wearable.SlotEnum.Eyes);
            SetWearableCard(headId, id, Head_NameText, null, Head_HpText, Head_AtkText, Head_CritText, Head_ApText, Head_OnchainSVGImage, Head_Outline, Wearable.SlotEnum.Head);
            SetWearableCard(rhId, id, RH_NameText, RH_SubtitleText, RH_HpText, RH_AtkText, RH_CritText, RH_ApText, RH_OnchainSVGImage, RH_Outline, Wearable.SlotEnum.Hand, false, RH_OffchainImage);
            SetWearableCard(lhId, id, LH_NameText, LH_SubtitleText, LH_HpText, LH_AtkText, LH_CritText, LH_ApText, LH_OnchainSVGImage, LH_Outline, Wearable.SlotEnum.Hand, true, LH_OffchainImage);
            SetWearableCard(petId, id, Pet_NameText, null, Pet_HpText, Pet_AtkText, Pet_CritText, Pet_ApText, Pet_OnchainSVGImage, Pet_Outline, Wearable.SlotEnum.Pet);
        }

        public void SetWearableCard(int wearableId, int gotchiId, TextMeshProUGUI nameText, TextMeshProUGUI subtitleText,
            TextMeshProUGUI hpText, TextMeshProUGUI atkText, TextMeshProUGUI critText, TextMeshProUGUI apText, SVGImage onchainSvgImage,
            Outline outline, Wearable.SlotEnum slot, bool isLeftIfWeapon = true, Image offchainImage = null)
        {
            // set some text colorisations
            var activeTextColor = Dropt.Utils.Color.HexToColor("#ffffff");
            var inactiveTextColor = Dropt.Utils.Color.HexToColor("#858585");

            // vars
            float hp, atk, crit, ap;

            // check if weapon
            var isWeapon = slot == Wearable.SlotEnum.Hand;

            // check for a 0 werableId and convert to 1000 for unarmed
            if (isWeapon && wearableId == 0) wearableId = 1000;

            // get wearable
            var wearable = WearableManager.Instance.GetWearable(wearableId);

            hp = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.NRG);
            atk = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.AGG);
            crit = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.SPK);
            ap = DroptStatCalculator.GetDroptStatByWearableId(wearableId, TraitType.BRN);

            var damage = wearable != null ?
                DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(gotchiId, TraitType.AGG) * wearable.RarityMultiplier * GetBaseAttackMultiplier(wearable.WeaponType)
                : 0;

            nameText.text = wearable == null ?
                isWeapon ? wearable.Name : "None | " + slot :
                isWeapon ? wearable.Name : wearable.Name + " | " + slot;

            if (isWeapon && subtitleText != null)
            {
                subtitleText.text =
                    ( wearable.WeaponType == Wearable.WeaponTypeEnum.Unarmed ? "Punch" : wearable.WeaponType.ToString() ) +
                    " | " + (isLeftIfWeapon ? "Left Hand" : "Right Hand") + " | " + damage.ToString("F0") + " Dmg";
            }
            hpText.text = "+" + hp.ToString("F0") + " Health";
            atkText.text = "+" + atk.ToString("F0") + " Attack";
            critText.text = "+" + (crit * 100).ToString("F1") + " Critical %";
            apText.text = "+" + ap.ToString("F0") + " Ability Pts";

            if (offchainImage != null) offchainImage.gameObject.SetActive(false);
            if (onchainSvgImage != null) onchainSvgImage.gameObject.SetActive(false);

            //if (wearable != null && wearableId != 0 && wearableId < 990)
            if (!isWeapon && wearableId != 0)
            {
                wearable = WearableManager.Instance.GetWearable(wearable.NameType);
                var svgSprite = wearable.SvgSprite;
                if (svgSprite != null)
                {
                    onchainSvgImage.gameObject.SetActive(true);
                    onchainSvgImage.sprite = wearable.SvgSprite;
                }
            }
            //else if ((wearableId == 0 || wearableId > 990) && isWeapon)
            else if (isWeapon)
            {
                // set custom image instead of the svgs
                offchainImage.gameObject.SetActive(true);
                offchainImage.sprite = WeaponSpriteManager.Instance.GetSprite(wearable.NameType, wearable.AttackView);
            }

            hpText.color = hp <= 0.1f ? inactiveTextColor : activeTextColor;
            atkText.color = atk <= 0.1f ? inactiveTextColor : activeTextColor;
            critText.color = crit < 0.01f ? inactiveTextColor : activeTextColor;
            apText.color = ap < 0.1f ? inactiveTextColor : activeTextColor;

            // set border color
            outline.effectColor = wearable != null ? wearable.RarityColor : Dropt.Utils.Color.HexToColor("#ffffff");
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
            var gotchiSvgs = m_gotchiDataManager.localWalletGotchiSvgSets;
            var gotchiData = m_gotchiDataManager.localWalletGotchiData;
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