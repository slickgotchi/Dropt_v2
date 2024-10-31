using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics; // Ensure you have the SVG package or correct namespace for your SVG solution

[RequireComponent(typeof(AspectRatioFitter), typeof(SVGImage))]
public class PreserveSvgAspect : MonoBehaviour
{
    private AspectRatioFitter _aspectRatioFitter;
    private SVGImage _svgImage;

    private void Awake()
    {
        _aspectRatioFitter = GetComponent<AspectRatioFitter>();
        _svgImage = GetComponent<SVGImage>();
    }

    private void Start()
    {
        UpdateAspectRatio();
    }

    private void OnEnable()
    {
        UpdateAspectRatio();
    }

    public void SetSvg(Sprite newSvg)
    {
        _svgImage.sprite = newSvg;
        UpdateAspectRatio();
    }

    private void UpdateAspectRatio()
    {
        if (_svgImage.sprite != null)
        {
            // Get the SVG's original aspect ratio from its viewport
            float aspectRatio = _svgImage.sprite.rect.width / _svgImage.sprite.rect.height;
            _aspectRatioFitter.aspectRatio = aspectRatio;
        }
    }
}
