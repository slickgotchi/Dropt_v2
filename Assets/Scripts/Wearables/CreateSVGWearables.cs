//using UnityEditor;
//using UnityEngine;
//using System;

//public class CreateSVGWearables : MonoBehaviour
//{
//    [MenuItem("SVGWearable/CreateAll")]
//    private static void DoSomething()
//    {
//        Array enumValues = Enum.GetValues(typeof(Wearable.NameEnum));
//        foreach (Wearable.NameEnum value in enumValues)
//        {
//            Debug.Log(value);
//            SvgWearableSO asset = ScriptableObject.CreateInstance<SvgWearableSO>();
//            asset.Name = value;
//            string assetPath = $"Assets/Resources/SvgWearableSO/{value}.asset";
//            AssetDatabase.CreateAsset(asset, assetPath);
//        }
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//        Debug.Log("FINISH===========");
//    }
//}
