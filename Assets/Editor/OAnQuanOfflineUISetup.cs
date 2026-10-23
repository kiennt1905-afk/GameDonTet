#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class OAnQuanOfflineUISetup
{
    [MenuItem("Tools/Tạo UI Ô Ăn Quan Offline (Custom Theme)")]
    public static void SetupUI()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        OAnQuanManager manager = Object.FindAnyObjectByType<OAnQuanManager>();
        OAnQuanUI ui = Object.FindAnyObjectByType<OAnQuanUI>();
        
        if (manager == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            manager = gameManagerObj.AddComponent<OAnQuanManager>();
            ui = gameManagerObj.AddComponent<OAnQuanUI>();
            ui.manager = manager;
        }

        Sprite stoneAsset = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/stone.png");
        Sprite mandarinAsset = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mandarin.png");
        
        if (ui.stoneSprite == null) ui.stoneSprite = stoneAsset != null ? stoneAsset : circleSprite;
        if (ui.mandarinSprite == null) ui.mandarinSprite = mandarinAsset != null ? mandarinAsset : circleSprite;

        string[] oldPanels = { "OAnQuan_Board", "StartPanel", "InGamePanel", "DirectionPanel", "GameOverPanel", "RulesPanel" };
        foreach (string panelName in oldPanels)
        {
            GameObject oldObj = GameObject.Find(panelName);
            if (oldObj != null) Object.DestroyImmediate(oldObj);
        }

        GameObject mainContainer = new GameObject("OAnQuan_Board");
        mainContainer.transform.SetParent(canvas.transform, false);

        ui.squareTexts = new Text[12];
        ui.squareButtons = new Button[12];
        ui.squareContainers = new Transform[12];

        // --- Bàn cờ ---
        GameObject boardObj = new GameObject("BoardGrid");
        boardObj.transform.SetParent(mainContainer.transform, false);
        RectTransform boardRect = boardObj.AddComponent<RectTransform>();
        boardRect.sizeDelta = new Vector2(1100, 400); 
        
        Image boardImage = boardObj.AddComponent<Image>();
        bool hasCustomSprite = ui.boardSprite != null || manager.mainBoardSprite != null;
        boardImage.sprite = hasCustomSprite ? (ui.boardSprite != null ? ui.boardSprite : manager.mainBoardSprite) : roundedSprite;
        boardImage.type = hasCustomSprite ? Image.Type.Simple : Image.Type.Sliced;
        boardImage.color = hasCustomSprite ? Color.white : new Color(0.75f, 0.50f, 0.30f);

        Vector2[] positions = new Vector2[12];
        positions[11] = new Vector2(-420, 0); 
        positions[5]  = new Vector2(420, 0);  

        for (int i = 0; i < 5; i++)
        {
            positions[i] = new Vector2(-280 + i * 140, -80); 
            positions[10 - i] = new Vector2(-280 + i * 140, 80); 
        }

        Vector2[] sizes = new Vector2[12];
        for(int i=0; i<12; i++)
        {
            if (i == 5 || i == 11) sizes[i] = new Vector2(140, 260);
            else sizes[i] = new Vector2(130, 130);
        }

        for (int i = 0; i < 12; i++)
        {
            GameObject squareObj = new GameObject("Square_" + i);
            squareObj.transform.SetParent(boardObj.transform, false);
            RectTransform rect = squareObj.AddComponent<RectTransform>();
            rect.anchoredPosition = positions[i];
            rect.sizeDelta = sizes[i];

            Image img = squareObj.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0, 0, 0, 0.15f); 
            
            Button btn = squareObj.AddComponent<Button>();
            ui.squareButtons[i] = btn;
            ui.squareContainers[i] = squareObj.transform; 

            Text txt = CreateText(squareObj.transform, "StonesText", new Vector2(0, -10), "", defaultFont, 40);
            txt.GetComponent<RectTransform>().sizeDelta = sizes[i];
            txt.color = Color.yellow; 
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.UpperCenter; 
            ui.squareTexts[i] = txt;

            if (i >= 0 && i <= 4)
            {
                int index = i;
                UnityEventTools.AddIntPersistentListener(btn.onClick, new UnityAction<int>(ui.OnSquareClicked), index);
            }
        }

        // --- Điểm số & Lượt ---
        ui.p1ScoreText = CreateText(mainContainer.transform, "P1Score", new Vector2(0, -220), "Bạn: 0", defaultFont, 40);
        ui.p1ScoreText.color = Color.white;

        ui.p2ScoreText = CreateText(mainContainer.transform, "P2Score", new Vector2(0, 220), "Máy: 0", defaultFont, 40);
        ui.p2ScoreText.color = Color.white;

        ui.turnText = CreateText(mainContainer.transform, "TurnText", new Vector2(0, 320), "Lượt của Bạn", defaultFont, 50);
        ui.turnText.color = Color.yellow;

        // --- Bảng chọn hướng ---
        GameObject dirPanel = new GameObject("DirectionPanel");
        dirPanel.transform.SetParent(mainContainer.transform, false);
        Image dirImg = dirPanel.AddComponent<Image>();
        dirImg.color = new Color(0, 0, 0, 0f); 
        RectTransform dirRect = dirPanel.GetComponent<RectTransform>();
        dirRect.sizeDelta = new Vector2(300, 100);
        ui.directionPanel = dirPanel;

        Button btnLeft = CreateButton(dirPanel.transform, "Btn_Left", new Vector2(-100, 0), "<-", defaultFont, new Vector2(80, 80));
        btnLeft.GetComponent<Image>().sprite = roundedSprite;
        btnLeft.GetComponent<Image>().type = Image.Type.Sliced;
        btnLeft.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);
        UnityEventTools.AddBoolPersistentListener(btnLeft.onClick, new UnityAction<bool>(ui.OnDirectionSelected), false);

        Button btnRight = CreateButton(dirPanel.transform, "Btn_Right", new Vector2(100, 0), "->", defaultFont, new Vector2(80, 80));
        btnRight.GetComponent<Image>().sprite = roundedSprite;
        btnRight.GetComponent<Image>().type = Image.Type.Sliced;
        btnRight.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);
        UnityEventTools.AddBoolPersistentListener(btnRight.onClick, new UnityAction<bool>(ui.OnDirectionSelected), true);
        dirPanel.SetActive(false);

        // --- Start Panel ---
        GameObject startPanelObj = new GameObject("StartPanel");
        startPanelObj.transform.SetParent(canvas.transform, false);
        Image startImg = startPanelObj.AddComponent<Image>();
        startImg.color = new Color(0, 0, 0, 0.9f);
        RectTransform startRect = startPanelObj.GetComponent<RectTransform>();
        startRect.anchorMin = Vector2.zero;
        startRect.anchorMax = Vector2.one;
        startRect.sizeDelta = Vector2.zero;
        ui.startPanel = startPanelObj;

        Text titleTxt = CreateText(startPanelObj.transform, "Title", new Vector2(0, 200), "Ô ĂN QUAN OFFLINE", defaultFont, 80);
        titleTxt.color = Color.yellow;

        Button btnToggleTheme = CreateButton(startPanelObj.transform, "Btn_ToggleTheme", new Vector2(-400, 200), "Đổi\nTheme", defaultFont, new Vector2(160, 100));
        btnToggleTheme.GetComponent<Image>().sprite = roundedSprite;
        btnToggleTheme.GetComponent<Image>().type = Image.Type.Sliced;
        btnToggleTheme.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.1f);
        UnityEventTools.AddVoidPersistentListener(btnToggleTheme.onClick, new UnityAction(ui.ToggleTheme));

        Button btnPlayBot = CreateButton(startPanelObj.transform, "Btn_PlayBot", new Vector2(0, 0), "Bắt Đầu Chơi", defaultFont, new Vector2(350, 80));
        btnPlayBot.GetComponent<Image>().sprite = roundedSprite;
        btnPlayBot.GetComponent<Image>().type = Image.Type.Sliced;
        btnPlayBot.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.2f);
        UnityEventTools.AddVoidPersistentListener(btnPlayBot.onClick, new UnityAction(ui.StartGame));

        Button btnStartExit = CreateButton(startPanelObj.transform, "Btn_Exit", new Vector2(0, -100), "Thoát", defaultFont, new Vector2(250, 60));
        btnStartExit.GetComponent<Image>().sprite = roundedSprite;
        btnStartExit.GetComponent<Image>().type = Image.Type.Sliced;
        btnStartExit.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f);
        UnityEventTools.AddVoidPersistentListener(btnStartExit.onClick, new UnityAction(ui.BackTo3DScene));

        // --- InGame Panel ---
        GameObject inGamePanelObj = new GameObject("InGamePanel");
        inGamePanelObj.transform.SetParent(canvas.transform, false);
        RectTransform igRect = inGamePanelObj.AddComponent<RectTransform>();
        igRect.anchorMin = Vector2.zero;
        igRect.anchorMax = Vector2.one;
        igRect.sizeDelta = Vector2.zero;
        ui.inGamePanel = inGamePanelObj;

        Button btnEndEarly = CreateButton(inGamePanelObj.transform, "Btn_EndEarly", new Vector2(0, 0), "Thoát / Kết Thúc", defaultFont, new Vector2(250, 50));
        btnEndEarly.GetComponent<Image>().sprite = roundedSprite;
        btnEndEarly.GetComponent<Image>().type = Image.Type.Sliced;
        btnEndEarly.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f);
        RectTransform btnEndRect = btnEndEarly.GetComponent<RectTransform>();
        btnEndRect.anchorMin = new Vector2(1, 0);
        btnEndRect.anchorMax = new Vector2(1, 0);
        btnEndRect.anchoredPosition = new Vector2(-150, 50);
        UnityEventTools.AddVoidPersistentListener(btnEndEarly.onClick, new UnityAction(ui.EndGameEarly));

        // --- Nút Luật Chơi ---
        // (Tet có thể chưa có ToggleRulesPanel nhưng nếu có, ta comment lại hoặc chỉ để đây nếu người dùng cần)
        // Để tránh lỗi compile, ta không gọi ui.ToggleRulesPanel() nếu chưa khai báo,
        // Nhưng nếu người dùng mún y chang:
        // (tạm ẩn nút luật vì OAnQuanUI bên Tet chưa có rules panel)
        
        // --- Bảng Game Over ---
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(canvas.transform, false);
        Image goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0, 0, 0, 0.9f);
        RectTransform goRect = goPanel.GetComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.sizeDelta = Vector2.zero;
        ui.gameOverPanel = goPanel;

        ui.winnerText = CreateText(goPanel.transform, "WinnerText", new Vector2(0, 100), "Game Over", defaultFont, 80);
        ui.winnerText.color = Color.yellow;

        Button btnRestart = CreateButton(goPanel.transform, "Btn_Restart", new Vector2(0, -20), "Chơi Lại", defaultFont, new Vector2(250, 60));
        btnRestart.GetComponent<Image>().sprite = roundedSprite;
        btnRestart.GetComponent<Image>().type = Image.Type.Sliced;
        UnityEventTools.AddVoidPersistentListener(btnRestart.onClick, new UnityAction(ui.RestartGame));

        Button btnGoBack = CreateButton(goPanel.transform, "Btn_Back", new Vector2(0, -100), "Thoát", defaultFont, new Vector2(250, 60));
        btnGoBack.GetComponent<Image>().sprite = roundedSprite;
        btnGoBack.GetComponent<Image>().type = Image.Type.Sliced;
        btnGoBack.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f);
        UnityEventTools.AddVoidPersistentListener(btnGoBack.onClick, new UnityAction(ui.BackTo3DScene));


        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>Tạo UI Ô Ăn Quan Offline (Custom Theme) thành công!</color>");
    }

    private static Text CreateText(Transform parent, string name, Vector2 pos, string defaultText, Font font, int fontSize = 24)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 80);
        Text txt = obj.AddComponent<Text>();
        txt.text = defaultText;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        txt.fontSize = fontSize;
        txt.font = font;
        return txt;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 pos, string label, Font font, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        Image img = obj.AddComponent<Image>();
        Button btn = obj.AddComponent<Button>();

        Text txt = CreateText(obj.transform, "Text", Vector2.zero, label, font, 36);
        txt.GetComponent<RectTransform>().sizeDelta = size;
        return btn;
    }
}
#endif
