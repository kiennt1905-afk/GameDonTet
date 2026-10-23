#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ShoppingGame;

public class ShoppingGameSetup : EditorWindow
{
    [MenuItem("Tools/Setup Shopping Game")]
    public static void SetupGame()
    {
        Debug.Log("Starting Shopping Game Setup...");

        // 1. Setup Tags
        AddTag("ShoppingItem");

        // 2. Setup Prefabs (Colliders, Rigidbodies, Scripts)
        SetupItemPrefab("Assets/MiniGameScenes/Shopping/broccoli_scale_3.prefab", "Broccoli");
        SetupItemPrefab("Assets/MiniGameScenes/Shopping/cabbage_scale_0.1.prefab", "Cabbage");
        SetupItemPrefab("Assets/MiniGameScenes/Shopping/carp_fish_scale_7.prefab", "Carpfish");
        SetupItemPrefab("Assets/MiniGameScenes/Shopping/chicken_scale_0.1.prefab", "Chicken");
        
        GameObject basketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/Shopping/basket_scale_0.06.prefab");
        if (basketPrefab != null)
        {
            GameObject basketContents = PrefabUtility.LoadPrefabContents("Assets/MiniGameScenes/Shopping/basket_scale_0.06.prefab");
            basketContents.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            
            if (basketContents.GetComponent<BoxCollider>() == null)
            {
                var col = basketContents.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }
            FitColliderToChildren(basketContents);
            BoxCollider bCol = basketContents.GetComponent<BoxCollider>();
            if (bCol != null)
            {
                float origY = bCol.size.y;
                bCol.size = new Vector3(bCol.size.x * 0.7f, origY * 0.2f, bCol.size.z * 0.7f);
                bCol.center = new Vector3(bCol.center.x, bCol.center.y - (origY / 2f) + (bCol.size.y / 2f) + (origY * 0.1f), bCol.center.z);
            }
            if (basketContents.GetComponent<Rigidbody>() == null)
            {
                var rb = basketContents.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            if (basketContents.GetComponent<ShoppingBasket>() == null)
            {
                basketContents.AddComponent<ShoppingBasket>();
            }
            
            PrefabUtility.SaveAsPrefabAsset(basketContents, "Assets/MiniGameScenes/Shopping/basket_scale_0.06.prefab");
            PrefabUtility.UnloadPrefabContents(basketContents);
        }
        else
        {
            Debug.LogError("Basket prefab not found at Assets/MiniGameScenes/Shopping/basket_scale_0.06.prefab");
        }

        // 3. Setup Scene Objects
        GameObject gameManagerObj = new GameObject("ShoppingGameManager");
        ShoppingGameManager manager = gameManagerObj.AddComponent<ShoppingGameManager>();

        // Set prefabs in GameManager
        manager.itemPrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/Shopping/broccoli_scale_3.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/Shopping/cabbage_scale_0.1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/Shopping/carp_fish_scale_7.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/Shopping/chicken_scale_0.1.prefab")
        };

        // 4. Instantiate Basket in Scene
        if (basketPrefab != null)
        {
            GameObject basketInstance = (GameObject)PrefabUtility.InstantiatePrefab(basketPrefab);
            basketInstance.transform.position = new Vector3(0, -3f, 0); // Bottom of screen
        }

        // 5. Setup Camera
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }

        // 6. Setup UI
        SetupUI(manager);

        // 7. EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        Debug.Log("Shopping Game Setup Complete!");
    }

    private static void SetupItemPrefab(string path, string itemName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Could not find prefab at: " + path);
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        
        float scale = 1f;
        string[] parts = path.Split(new string[] { "scale_" }, System.StringSplitOptions.None);
        if (parts.Length > 1)
        {
            string scaleStr = parts[1].Replace(".prefab", "");
            if (float.TryParse(scaleStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedScale))
            {
                scale = parsedScale;
            }
        }

        if (itemName.Equals("Chicken", System.StringComparison.OrdinalIgnoreCase))
        {
            scale = 0.07f;
        }

        contents.transform.localScale = new Vector3(scale, scale, scale);

        contents.tag = "ShoppingItem";

        if (contents.GetComponent<BoxCollider>() == null)
        {
            var col = contents.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }
        FitColliderToChildren(contents);
        if (contents.GetComponent<Rigidbody>() == null)
        {
            var rb = contents.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        ShoppingItem itemScript = contents.GetComponent<ShoppingItem>();
        if (itemScript == null)
        {
            itemScript = contents.AddComponent<ShoppingItem>();
        }
        itemScript.itemName = itemName;

        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static void SetupUI(ShoppingGameManager manager)
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Text display container
        GameObject containerObj = new GameObject("Requirements");
        containerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);

        manager.requirements = new System.Collections.Generic.List<ShoppingRequirement>();

        int yOffset = 0;
        manager.requirements.Add(CreateUIRequirement("Broccoli", 5, containerObj.transform, ref yOffset));
        manager.requirements.Add(CreateUIRequirement("Cabbage", 1, containerObj.transform, ref yOffset));
        manager.requirements.Add(CreateUIRequirement("Carpfish", 2, containerObj.transform, ref yOffset));
        manager.requirements.Add(CreateUIRequirement("Chicken", 1, containerObj.transform, ref yOffset));

        // Panels
        manager.startPanel = CreateStartPanel(canvasObj.transform, manager);
        manager.gameOverPanel = CreateGameOverPanel(canvasObj.transform, manager);
        manager.victoryPanel = CreateVictoryPanel(canvasObj.transform, manager);
    }

    private static ShoppingRequirement CreateUIRequirement(string itemName, int requiredCount, Transform parent, ref int yOffset)
    {
        GameObject textObj = new GameObject(itemName + "Text");
        textObj.transform.SetParent(parent, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(300, 35);
        rect.anchoredPosition = new Vector2(0, yOffset);
        yOffset -= 40;

        ShoppingRequirement req = new ShoppingRequirement();
        req.itemName = itemName;
        req.requiredCount = requiredCount;
        req.uiText = text;

        return req;
    }

    private static GameObject CreateStartPanel(Transform canvas, ShoppingGameManager manager)
    {
        GameObject panelObj = new GameObject("Start Panel");
        panelObj.transform.SetParent(canvas, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // Title
        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "MINIGAME ĐI CHỢ";
        text.fontSize = 60;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = text.rectTransform;
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = new Vector2(0, 150);

        // Start Button
        CreateButton(panelObj.transform, "StartButton", "Bắt đầu", new Vector2(0, 0), manager.StartGame);

        panelObj.SetActive(true);
        return panelObj;
    }

    private static GameObject CreateGameOverPanel(Transform canvas, ShoppingGameManager manager)
    {
        GameObject panelObj = new GameObject("Game Over Panel");
        panelObj.transform.SetParent(canvas, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.8f, 0, 0, 0.8f);
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "THẤT BẠI!";
        text.fontSize = 80;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = text.rectTransform;
        textRect.sizeDelta = new Vector2(600, 200);
        textRect.anchoredPosition = new Vector2(0, 100);

        GameObject reasonObj = new GameObject("ReasonText");
        reasonObj.transform.SetParent(panelObj.transform, false);
        Text reasonText = reasonObj.AddComponent<Text>();
        reasonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        reasonText.text = "Lý do...";
        reasonText.fontSize = 40;
        reasonText.color = Color.yellow;
        reasonText.alignment = TextAnchor.MiddleCenter;
        RectTransform reasonRect = reasonText.rectTransform;
        reasonRect.sizeDelta = new Vector2(800, 100);
        reasonRect.anchoredPosition = new Vector2(0, -50);

        manager.loseReasonText = reasonText;
        panelObj.SetActive(false);
        return panelObj;
    }

    private static GameObject CreateVictoryPanel(Transform canvas, ShoppingGameManager manager)
    {
        GameObject panelObj = new GameObject("Victory Panel");
        panelObj.transform.SetParent(canvas, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0.6f, 0, 0.9f);
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "CHIẾN THẮNG!";
        text.fontSize = 80;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = text.rectTransform;
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = new Vector2(0, 0);

        panelObj.SetActive(false);
        return panelObj;
    }

    private static void CreateButton(Transform parent, string name, string btnTextStr, Vector2 anchoredPos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(250, 70);
        btnRect.anchoredPosition = anchoredPos;

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = btnTextStr;
        btnText.fontSize = 32;
        btnText.color = Color.black;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform btnTextRect = btnText.rectTransform;
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) { found = true; break; }
        }
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }

    private static void FitColliderToChildren(GameObject go)
    {
        BoxCollider col = go.GetComponent<BoxCollider>();
        if (col == null) return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        col.center = go.transform.InverseTransformPoint(bounds.center);
        
        Vector3 scale = go.transform.localScale;
        col.size = new Vector3(
            bounds.size.x / (scale.x == 0 ? 1 : scale.x),
            bounds.size.y / (scale.y == 0 ? 1 : scale.y),
            bounds.size.z / (scale.z == 0 ? 1 : scale.z)
        );
    }
}
#endif
