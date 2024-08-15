﻿#region

using CarlosLab.Common.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    public class WelcomeScreenWindow : BaseEditorWindow
    {
        public static void StartUp()
        {
            if (FrameworkEditorPrefs.FrameworkVersion != FrameworkRuntimeConsts.FrameworkVersion)
            {
                FrameworkEditorPrefs.FrameworkVersion = FrameworkRuntimeConsts.FrameworkVersion;
                FrameworkEditorPrefs.DontShowAgainWelcomeScreen = false;
            }

            if (FrameworkEditorPrefs.DontShowAgainWelcomeScreen)
                return;

            EditorApplication.delayCall += OpenWindow;
        }

        [MenuItem(WelcomeScreenConsts.MenuPath)]
        public static void OpenWindow()
        {
            Rect rect = new(0, 0, 500, 320);
            GetWindowWithRect<WelcomeScreenWindow>(rect, true, "Welcome");
        }

        protected override void OnInitGUI()
        {
            LoadVisualAsset(WelcomeScreenConsts.VisualAssetPath);
            if(EditorGUIUtility.isProSkin)
                LoadStyleSheet(WelcomeScreenConsts.DarkThemePath);
            else
                LoadStyleSheet(WelcomeScreenConsts.LightThemePath);

            Label welcomeLabel = rootVisualElement.Q<Label>("WelcomeLabel");
            welcomeLabel.text = WelcomeScreenConsts.WelcomeLabelText;
            Label thanksLabel = rootVisualElement.Q<Label>("ThanksLabel");
            thanksLabel.text = WelcomeScreenConsts.ThanksLabelText;

            Label frameworkVersionLabel = rootVisualElement.Q<Label>("FrameworkVersionLabel");
            frameworkVersionLabel.text = WelcomeScreenConsts.FrameworkVersionLabelText;

            Button documentationButton = rootVisualElement.Q<Button>("DocumentationButton");
            documentationButton.clicked += () => Help.BrowseURL(FrameworkEditorConsts.DocumentationUrl);

            Button discordButton = rootVisualElement.Q<Button>("DiscordButton");
            discordButton.clicked += () => Help.BrowseURL(FrameworkEditorConsts.DiscordUrl);
            
            Label supportLabel = rootVisualElement.Q<Label>("SupportLabel");
            supportLabel.text = WelcomeScreenConsts.SuportLabelText;

            Toggle dontShowAgainToggle = rootVisualElement.Q<Toggle>("DontShowAgainToggle");
            dontShowAgainToggle.value = FrameworkEditorPrefs.DontShowAgainWelcomeScreen;
            dontShowAgainToggle.RegisterValueChangedCallback(evt => FrameworkEditorPrefs.DontShowAgainWelcomeScreen = evt.newValue);
        }
    }
}