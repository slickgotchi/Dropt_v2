using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

namespace GotchiHub
{
    public class GotchiSelectListItem : MonoBehaviour
    {
        [HideInInspector] public int Id = 0;
        [HideInInspector] public int BRS = 0;

        private Button m_button;
        private TMPro.TextMeshProUGUI m_nameText;
        private SVGImage m_svgImage;
        private Image m_selectedHighlightImage;

        private void Awake()
        {
            m_button = GetComponent<Button>();
            m_nameText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            m_svgImage = GetComponentInChildren<SVGImage>();
            m_selectedHighlightImage = GetComponentInChildren<Image>();

            m_button.onClick.AddListener(HandleOnClick);
            m_selectedHighlightImage.enabled = false;
        }

        private void Start()
        {
        }

        public void InitById(int id)
        {
            Id = id;
            var gotchiSvg = GotchiDataManager.Instance.GetGotchiSvgsById(id);
            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
            m_svgImage.sprite = CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingUI.CustomizeSVG(gotchiSvg.Front), Vector2.zero);
            m_svgImage.material = GotchiDataManager.Instance.Material_Unlit_VectorGradientUI;
            m_nameText.text = gotchiData.name;
            BRS = DroptStatCalculator.GetBRS(gotchiData.numericTraits);
        }

        private void HandleOnClick()
        {
            GotchiDataManager.Instance.SetSelectedGotchiById(Id);

            GotchiSelectCanvas.Instance.HighlightById(Id);
        }

        public void SetSelected(bool isSelected)
        {
            m_selectedHighlightImage.enabled = isSelected;
        }
    }
}
