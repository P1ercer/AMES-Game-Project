#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

// Editor helper to create a Perk Chooser UI prefab and place it in Assets/Prefabs
public static class CreatePerkChooserPrefab
{
    [MenuItem("Tools/Create Perk Chooser Prefab")]
    public static void CreatePrefab()
    {
        // ensure folder exists
        var prefabsDir = Path.Combine(Application.dataPath, "Prefabs");
        if (!Directory.Exists(prefabsDir))
            Directory.CreateDirectory(prefabsDir);

        // create Canvas
        var canvasGO = new GameObject("PerkChooserCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Panel background
        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0f);
        panelRT.anchorMax = new Vector2(0.5f, 0f);
        panelRT.pivot = new Vector2(0.5f, 0f);
        panelRT.sizeDelta = new Vector2(400, 120);
        panelRT.anchoredPosition = new Vector2(0, 60);
        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.5f);

        // Create Button
        var buttonGO = new GameObject("RandomButton", typeof(RectTransform), typeof(Button), typeof(Image));
        buttonGO.transform.SetParent(panelGO.transform, false);
        var btnRT = buttonGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0f, 0.5f);
        btnRT.anchorMax = new Vector2(0f, 0.5f);
        btnRT.pivot = new Vector2(0f, 0.5f);
        btnRT.sizeDelta = new Vector2(140, 50);
        btnRT.anchoredPosition = new Vector2(20, 0);
        var btnImg = buttonGO.GetComponent<Image>();
        btnImg.color = new Color(0.8f, 0.4f, 0.2f, 1f);
        var button = buttonGO.GetComponent<Button>();

        // Button text
        var btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        var btnText = btnTextGO.GetComponent<Text>();
        btnText.text = "Random Perk";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.sizeDelta = Vector2.zero;

        // Selected perk text
        var selectedGO = new GameObject("SelectedPerkText", typeof(RectTransform), typeof(Text));
        selectedGO.transform.SetParent(panelGO.transform, false);
        var selectedText = selectedGO.GetComponent<Text>();
        selectedText.text = "Selected: None";
        selectedText.alignment = TextAnchor.MiddleLeft;
        selectedText.color = Color.white;
        selectedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var selRT = selectedGO.GetComponent<RectTransform>();
        selRT.anchorMin = new Vector2(0f, 0.5f);
        selRT.anchorMax = new Vector2(1f, 0.5f);
        selRT.pivot = new Vector2(0f, 0.5f);
        selRT.sizeDelta = new Vector2(-180, 30);
        selRT.anchoredPosition = new Vector2(180, 15);

        // Active count text
        var countGO = new GameObject("ActiveCountText", typeof(RectTransform), typeof(Text));
        countGO.transform.SetParent(panelGO.transform, false);
        var countText = countGO.GetComponent<Text>();
        countText.text = "Active: 0/0";
        countText.alignment = TextAnchor.MiddleLeft;
        countText.color = Color.white;
        countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var countRT = countGO.GetComponent<RectTransform>();
        countRT.anchorMin = new Vector2(0f, 0f);
        countRT.anchorMax = new Vector2(1f, 0f);
        countRT.pivot = new Vector2(0f, 0f);
        countRT.sizeDelta = new Vector2(-40, 30);
        countRT.anchoredPosition = new Vector2(20, 10);

        // Attach PerkChooserUI component
        var chooser = canvasGO.AddComponent<AmesGame.PerkChooserUI>();
        // try to find existing PerkController in the scene to assign (not serialized into prefab reliably)
        var controller = Object.FindObjectOfType<AmesGame.PerkController>();
        if (controller != null)
            chooser.perkController = controller;
        chooser.randomButton = button;
        chooser.selectedPerkText = selectedText;
        chooser.activeCountText = countText;

        // Save as prefab
        string localPath = "Assets/Prefabs/PerkChooser.prefab";
        // ensure Assets/Prefabs exists in project
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(canvasGO, localPath);
        if (prefab != null)
        {
            Debug.Log("Perk Chooser prefab created at " + localPath);
        }
        else
        {
            Debug.LogError("Failed to create Perk Chooser prefab.");
        }

        // cleanup created scene objects
        Object.DestroyImmediate(canvasGO);

        // refresh asset database
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif