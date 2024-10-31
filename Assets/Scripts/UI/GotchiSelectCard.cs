using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using GotchiHub;

public class GotchiSelectCard : MonoBehaviour
{
    [HideInInspector] public int Id = 0;
    [HideInInspector] public int BRS = 0;

    public TMPro.TextMeshProUGUI HpText;
    public TMPro.TextMeshProUGUI AtkText;
    public TMPro.TextMeshProUGUI CritText;
    public TMPro.TextMeshProUGUI ApText;

    public Image CardBackgroundImage;
    public SVGImage OnchainSvgImage;
    public Image OffchainImage;

    public Button SelectMeButton;
    private TMPro.TextMeshProUGUI m_nameText;

    private void Awake()
    {
        SelectMeButton = GetComponent<Button>();
        m_nameText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        OnchainSvgImage = GetComponentInChildren<SVGImage>();
        CardBackgroundImage = GetComponentInChildren<Image>();

        SelectMeButton.onClick.AddListener(HandleOnClick);
        CardBackgroundImage.color = Dropt.Utils.Color.HexToColor("#3d3d3d");
    }

    private void Start()
    {
    }

    public void InitById(int id)
    {
        Id = id;
        var gotchiSvg = GotchiDataManager.Instance.GetGotchiSvgsById(id);
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);

        // set stats
        HpText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.NRG).ToString("F0");
        AtkText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.AGG).ToString("F0");
        CritText.text = (DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.SPK) * 100).ToString("F0");
        ApText.text = DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(id, TraitType.BRN).ToString("F0");

        // if we got onchain data, set svg image
        if (gotchiData != null)
        {
            OnchainSvgImage.sprite = CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
            OnchainSvgImage.material = GotchiDataManager.Instance.Material_Unlit_VectorGradientUI;
            m_nameText.text = gotchiData.name;
            BRS = DroptStatCalculator.GetBRS(gotchiData.numericTraits);
            OffchainImage.enabled = false;
            return;
        }

        // else lets get offchain data
        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
        if (offchainGotchiData != null)
        {
            Debug.Log("Setting offchain gotchi data");
            OffchainImage.sprite = offchainGotchiData.spriteFront;
            OffchainImage.material = GotchiDataManager.Instance.Material_Sprite_Unlit_Default;
            m_nameText.text = offchainGotchiData.gotchiName;
            BRS = DroptStatCalculator.GetBRS(offchainGotchiData.numericTraits);
            OnchainSvgImage.enabled = false;
            return;
        }
    }

    private void HandleOnClick()
    {
        Debug.Log("Clicked ID: " + Id);
        GotchiDataManager.Instance.SetSelectedGotchiById(Id);
        GotchiSelectCanvas.Instance.HighlightById(Id);
    }

    public void SetSelected(bool isSelected)
    {
        //CardBackgroundImage.enabled = isSelected;
        CardBackgroundImage.color = Dropt.Utils.Color.HexToColor(isSelected ? "#3b1443" : "#3d3d3d");
    }
}

