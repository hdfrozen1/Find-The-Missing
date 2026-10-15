using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace FindTheMissing.Editor
{
    public static class SetupMenuCanvasEditor
    {
        private const string MAIN_ATLAS_GUID = "71036179f6419e84e963d65b560fb5ac";
        private const string SETTING_ATLAS_GUID = "8117eb4566e44db44943f291a3fc4a05";

        [MenuItem("Tools/Find The Missing/Setup Setting Panel (Match Demo)")]
        public static void SetupSettingPanel()
        {
            var settingPanel = GameObject.Find("SettingPanel");
            if (settingPanel == null)
            {
                Debug.LogError("[SetupMenuCanvas] SettingPanel GameObject not found in scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(settingPanel, "Setup Setting Panel");

            // Load setting atlas sprites
            string atlasPath = AssetDatabase.GUIDToAssetPath(SETTING_ATLAS_GUID);
            Sprite bgSprite = null;
            Sprite closeSprite = null;
            Sprite soundSprite = null;
            Sprite musicSprite = null;
            Sprite vibrationSprite = null;

            if (!string.IsNullOrEmpty(atlasPath))
            {
                var allSprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
                foreach (var obj in allSprites)
                {
                    if (obj is Sprite sprite)
                    {
                        if (sprite.name == "UISetting_Bg") bgSprite = sprite;
                        else if (sprite.name == "UISetting_Close") closeSprite = sprite;
                        else if (sprite.name == "UISetting_Sound") soundSprite = sprite;
                        else if (sprite.name == "UISetting_Music") musicSprite = sprite;
                        else if (sprite.name == "UISetting_Vibration") vibrationSprite = sprite;
                    }
                }
            }

            // Setup SettingPanel background
            var panelRect = settingPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(827f, 647f);

            var panelImg = settingPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                if (bgSprite != null) panelImg.sprite = bgSprite;
                panelImg.preserveAspect = true;
            }

            // Configure children
            for (int i = 0; i < settingPanel.transform.childCount; i++)
            {
                var child = settingPanel.transform.GetChild(i);
                var img = child.GetComponent<Image>();
                var rt = child.GetComponent<RectTransform>();
                if (img != null) img.preserveAspect = true;

                if (child.name.Contains("Close") || (img != null && img.sprite == closeSprite))
                {
                    child.name = "CloseBtn";
                    if (closeSprite != null && img != null) img.sprite = closeSprite;
                    if (child.GetComponent<Button>() == null) child.gameObject.AddComponent<Button>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(370f, 290f);
                    rt.sizeDelta = new Vector2(120f, 120f);
                }
                else if (child.name.Contains("Sound") || (img != null && img.sprite == soundSprite) || child.name == "Image")
                {
                    child.name = "SoundItem";
                    if (soundSprite != null && img != null) img.sprite = soundSprite;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-180f, 95f);
                    rt.sizeDelta = new Vector2(300f, 92f);
                }
                else if (child.name.Contains("Music") || (img != null && img.sprite == musicSprite) || child.name == "Image (1)")
                {
                    child.name = "MusicItem";
                    if (musicSprite != null && img != null) img.sprite = musicSprite;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-185f, -10f);
                    rt.sizeDelta = new Vector2(290f, 88f);
                }
                else if (child.name.Contains("Vibration") || (img != null && img.sprite == vibrationSprite) || child.name == "Image (2)")
                {
                    child.name = "VibrationItem";
                    if (vibrationSprite != null && img != null) img.sprite = vibrationSprite;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-150f, -115f);
                    rt.sizeDelta = new Vector2(350f, 98f);
                }
            }

            // Create/Update 3 Toggles
            Vector2 toggleSize = new Vector2(150f, 75f);
            CreateOrUpdateToggle(settingPanel.transform, "SoundToggle", new Vector2(180f, 95f), toggleSize, true);
            CreateOrUpdateToggle(settingPanel.transform, "MusicToggle", new Vector2(180f, -10f), toggleSize, true);
            CreateOrUpdateToggle(settingPanel.transform, "VibrationToggle", new Vector2(180f, -115f), toggleSize, true);

            // Reorder CloseBtn to be last so it renders on top
            var closeTf = settingPanel.transform.Find("CloseBtn");
            if (closeTf != null)
            {
                closeTf.SetAsLastSibling();
            }

            EditorUtility.SetDirty(settingPanel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(settingPanel.scene);
            Debug.Log("[SetupMenuCanvas] SettingPanel and 3 Toggles organized according to demo.png successfully!");
        }

        private static Toggle CreateOrUpdateToggle(Transform parent, string toggleName, Vector2 position, Vector2 size, bool isOn)
        {
            Transform toggleTf = parent.Find(toggleName);
            GameObject toggleGo;
            if (toggleTf == null)
            {
                toggleGo = new GameObject(toggleName, typeof(RectTransform), typeof(Toggle));
                toggleGo.transform.SetParent(parent, false);
            }
            else
            {
                toggleGo = toggleTf.gameObject;
                if (toggleGo.GetComponent<Toggle>() == null) toggleGo.AddComponent<Toggle>();
            }

            var toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
            toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = position;
            toggleRect.sizeDelta = size;

            var toggle = toggleGo.GetComponent<Toggle>();
            toggle.isOn = isOn;

            // Background
            Transform bgTf = toggleGo.transform.Find("Background");
            GameObject bgGo;
            if (bgTf == null)
            {
                bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgGo.transform.SetParent(toggleGo.transform, false);
            }
            else
            {
                bgGo = bgTf.gameObject;
            }

            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = new Color(0.294f, 0.78f, 0.282f, 1f); // Vibrant green

            // Checkmark / Handle
            Transform checkTf = bgGo.transform.Find("Checkmark");
            GameObject checkGo;
            if (checkTf == null)
            {
                checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                checkGo.transform.SetParent(bgGo.transform, false);
            }
            else
            {
                checkGo = checkTf.gameObject;
            }

            var checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.pivot = new Vector2(0.5f, 0.5f);
            checkRect.anchoredPosition = new Vector2(size.x * 0.24f, 0f);
            float knobSize = size.y * 0.8f;
            checkRect.sizeDelta = new Vector2(knobSize, knobSize);

            var checkImg = checkGo.GetComponent<Image>();
            checkImg.color = Color.white;

            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;

            return toggle;
        }

        [MenuItem("Tools/Find The Missing/Setup Main Menu Canvas (Match Demo)")]
        public static void SetupMainMenuCanvas()
        {
            Canvas canvas = null;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c.gameObject.name == "Canvas" || c.gameObject.name == "MainMenuCanvas")
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                Debug.LogError("[SetupMenuCanvas] Main Canvas not found in scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Setup Main Menu Canvas");

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            string atlasPath = AssetDatabase.GUIDToAssetPath(MAIN_ATLAS_GUID);
            Sprite titleSprite = null;
            Sprite playSprite = null;
            Sprite levelSprite = null;
            Sprite settingSprite = null;
            Sprite helpSprite = null;

            if (!string.IsNullOrEmpty(atlasPath))
            {
                var allSprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
                foreach (var obj in allSprites)
                {
                    if (obj is Sprite sprite)
                    {
                        if (sprite.name == "FindTheMissingUI_Title") titleSprite = sprite;
                        else if (sprite.name == "FindTheMissingUI_Play") playSprite = sprite;
                        else if (sprite.name == "FindTheMissingUI_Level") levelSprite = sprite;
                        else if (sprite.name == "FindTheMissingUI_Setting") settingSprite = sprite;
                        else if (sprite.name == "FindTheMissingUI_15") helpSprite = sprite;
                    }
                }
            }

            Transform panelTf = canvas.transform.Find("MenuPanel") ?? canvas.transform.Find("Panel");
            if (panelTf == null && canvas.transform.childCount > 0)
            {
                panelTf = canvas.transform.GetChild(0);
            }

            if (panelTf != null)
            {
                var panelRect = panelTf.GetComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.sizeDelta = Vector2.zero;

                for (int i = 0; i < panelTf.childCount; i++)
                {
                    var child = panelTf.GetChild(i);
                    var img = child.GetComponent<Image>();
                    var btn = child.GetComponent<Button>();

                    if (img != null)
                    {
                        img.preserveAspect = true;
                    }

                    if (img != null && (img.sprite == titleSprite || child.name.Contains("Title") || child.name == "Image"))
                    {
                        child.name = "TitleLogo";
                        if (titleSprite != null) img.sprite = titleSprite;
                        var rt = child.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 1f);
                        rt.anchorMax = new Vector2(0.5f, 1f);
                        rt.pivot = new Vector2(0.5f, 1f);
                        rt.anchoredPosition = new Vector2(0f, -260f);
                        rt.sizeDelta = new Vector2(770f, 496f);
                    }
                    else if ((btn != null && (img?.sprite == playSprite || child.name == "Button (1)" || child.name.Contains("Play"))))
                    {
                        child.name = "PlayButton";
                        if (playSprite != null && img != null) img.sprite = playSprite;
                        var rt = child.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(0f, -180f);
                        rt.sizeDelta = new Vector2(680f, 258f);
                    }
                    else if ((btn != null && (img?.sprite == levelSprite || child.name == "Button" || child.name.Contains("Level"))))
                    {
                        child.name = "LevelsButton";
                        if (levelSprite != null && img != null) img.sprite = levelSprite;
                        var rt = child.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(0f, -450f);
                        rt.sizeDelta = new Vector2(550f, 177f);
                    }
                    else if (img != null && (img.sprite == settingSprite || child.name.Contains("Setting")))
                    {
                        child.name = "SettingsButton";
                        if (settingSprite != null) img.sprite = settingSprite;
                        if (btn == null) child.gameObject.AddComponent<Button>();
                        var rt = child.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0f, 0f);
                        rt.anchorMax = new Vector2(0f, 0f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(160f, 160f);
                        rt.sizeDelta = new Vector2(160f, 160f);
                    }
                    else if (img != null && (img.sprite == helpSprite || child.name.Contains("Help") || child.name == "Image (1)"))
                    {
                        child.name = "HelpButton";
                        if (helpSprite != null) img.sprite = helpSprite;
                        if (btn == null) child.gameObject.AddComponent<Button>();
                        var rt = child.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(1f, 0f);
                        rt.anchorMax = new Vector2(1f, 0f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(-160f, 160f);
                        rt.sizeDelta = new Vector2(160f, 160f);
                    }
                }
            }

            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("[SetupMenuCanvas] Main Menu Canvas organized according to demo.png successfully!");
        }
    }
}
