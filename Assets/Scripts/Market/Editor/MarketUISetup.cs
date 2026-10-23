#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tools/Market/Generate Market Prompt UI
/// Tạo MarketPromptPanel chuẩn trong Canvas hiện tại của scene.
/// Style: pill-shaped nền tối, icon bàn phím [E], text vàng, bo tròn.
/// </summary>
public class MarketUISetup : EditorWindow
{
    [MenuItem("Tools/Market/Generate Market Prompt UI")]
    public static void GenerateMarketPromptUI()
    {
        // ── 1. Canvas ──────────────────────────────────────────────
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasObj;
        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[MarketUISetup] Tạo mới Canvas.");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // ── 2. Kiểm tra đã có MarketPromptPanel chưa ──────────────
        Transform existing = canvasObj.transform.Find("MarketPromptPanel");
        if (existing != null)
        {
            Debug.LogWarning("[MarketUISetup] Đã có MarketPromptPanel trong Canvas. Xóa cái cũ trước nếu muốn tạo lại.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // ── 3. Panel nền — pill shape (rounded via sprite) ─────────
        // Dùng Unity built-in rounded rect (UIRoundedCorners nếu có), 
        // ở đây dùng Image + Sprite "UISprite" (Unity default rounded)
        GameObject panelObj = new GameObject("MarketPromptPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        // Vị trí: giữa-dưới màn hình, cách bottom 120px
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot    = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(520f, 80f);
        panelRect.anchoredPosition = new Vector2(0f, 120f);

        Image panelImg = panelObj.AddComponent<Image>();
        // Dùng sprite bo tròn built-in của Unity
        panelImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        panelImg.type   = Image.Type.Sliced;
        panelImg.color  = new Color(0.06f, 0.06f, 0.10f, 0.88f); // nền tối, hơi xanh đậm

        // ── 4. Viền sáng (outline glow) ────────────────────────────
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(panelObj.transform, false);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(6f, 6f);   // dày hơn panel 3px mỗi bên
        borderRect.anchoredPosition = Vector2.zero;

        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = panelImg.sprite;
        borderImg.type   = Image.Type.Sliced;
        borderImg.color  = new Color(1f, 0.84f, 0.1f, 0.35f); // viền vàng mờ

        // ── 5. Badge phím [E] ──────────────────────────────────────
        GameObject badgeObj = new GameObject("KeyBadge");
        badgeObj.transform.SetParent(panelObj.transform, false);

        RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0f, 0.5f);
        badgeRect.anchorMax = new Vector2(0f, 0.5f);
        badgeRect.pivot     = new Vector2(0f, 0.5f);
        badgeRect.sizeDelta = new Vector2(52f, 52f);
        badgeRect.anchoredPosition = new Vector2(16f, 0f);

        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.sprite = panelImg.sprite;
        badgeImg.type   = Image.Type.Sliced;
        badgeImg.color  = new Color(1f, 0.84f, 0.1f, 1f); // vàng đậm

        // Chữ E trong badge
        GameObject eTextObj = new GameObject("EText");
        eTextObj.transform.SetParent(badgeObj.transform, false);
        TextMeshProUGUI eText = eTextObj.AddComponent<TextMeshProUGUI>();
        eText.text      = "E";
        eText.fontSize  = 32f;
        eText.fontStyle = FontStyles.Bold;
        eText.color     = new Color(0.06f, 0.06f, 0.10f, 1f); // chữ tối trên nền vàng
        eText.alignment = TextAlignmentOptions.Center;

        RectTransform eRect = eTextObj.GetComponent<RectTransform>();
        eRect.anchorMin = Vector2.zero;
        eRect.anchorMax = Vector2.one;
        eRect.sizeDelta = Vector2.zero;
        eRect.anchoredPosition = Vector2.zero;

        // ── 6. Prompt Text chính ────────────────────────────────────
        GameObject promptTextObj = new GameObject("PromptText");
        promptTextObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI promptTMP = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptTMP.text      = "Mua nếp";
        promptTMP.fontSize  = 34f;
        promptTMP.fontStyle = FontStyles.Bold;
        promptTMP.color     = new Color(1f, 0.92f, 0.6f, 1f); // vàng ấm
        promptTMP.alignment = TextAlignmentOptions.MidlineLeft;
        promptTMP.enableWordWrapping = false;

        RectTransform promptRect = promptTMP.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0f, 0f);
        promptRect.anchorMax = new Vector2(1f, 1f);
        promptRect.sizeDelta = new Vector2(-100f, -10f); // padding trái cho badge
        promptRect.anchoredPosition = new Vector2(40f, 0f);

        // ── 7. Gắn MarketPromptUI script ──────────────────────────
        MarketPromptUI promptScript = panelObj.AddComponent<MarketPromptUI>();
        promptScript.promptPanel = panelObj;
        promptScript.promptText  = promptTMP;

        // ── 8. Tắt panel ban đầu ─────────────────────────────────
        panelObj.SetActive(false);

        // ── 9. FloatingTextSpawner (nếu chưa có) ─────────────────
        if (FindObjectOfType<FloatingTextSpawner>() == null)
        {
            GameObject spawnerObj = new GameObject("FloatingTextSpawner");
            spawnerObj.AddComponent<FloatingTextSpawner>();
            Debug.Log("[MarketUISetup] Đã tạo FloatingTextSpawner.");
        }

        // ── 10. Chọn object vừa tạo ──────────────────────────────
        Selection.activeGameObject = panelObj;
        EditorUtility.SetDirty(panelObj);

        Debug.Log("[MarketUISetup] ✅ Đã tạo MarketPromptPanel thành công! " +
                  "Panel đã gắn MarketPromptUI với đủ references. Nhớ kéo vào scene Cho.");
    }

    [MenuItem("Tools/Market/Generate Market Prompt UI", validate = true)]
    public static bool ValidateGenerateMarketPromptUI()
    {
        // Chỉ cho chạy khi đang ở Edit Mode (không phải Play Mode)
        return !Application.isPlaying;
    }
}
#endif
