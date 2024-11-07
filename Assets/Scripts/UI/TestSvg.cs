using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

public class TestSvg : MonoBehaviour
{
    public Wearable.NameEnum name = Wearable.NameEnum._1337Laptop;

    private void Update()
    {
        var wearable = WearableManager.Instance.GetWearable(name);
        if (wearable != null)
        {
            var svg = GetComponent<SVGImage>();
            if (svg != null) svg.sprite = wearable.SvgSprite;
        }
    }
}
