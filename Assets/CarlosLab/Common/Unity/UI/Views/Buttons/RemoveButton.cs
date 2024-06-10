#region

using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

namespace CarlosLab.Common.UI
{
    [UxmlElement]
    public partial class RemoveButton : Button
    {
        public RemoveButton()
        {
            string removeButtonPath = null;
#if UNITY_EDITOR
            removeButtonPath = EditorGUIUtility.isProSkin
                ? UIBuilderResourcePaths.RemoveButton_Dark
                : UIBuilderResourcePaths.RemoveButton_Light;
#else
            removeButtonPath = UIBuilderResourcePaths.RemoveButton_Dark;
#endif
            StyleSheet styleSheet = Resources.Load<StyleSheet>(removeButtonPath);
            if (styleSheet == null) return;
            
            styleSheets.Add(styleSheet);
            AddToClassList("remove-button");
        }
    }
}