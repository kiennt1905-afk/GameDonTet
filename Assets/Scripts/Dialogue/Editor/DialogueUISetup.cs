#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUISetup : EditorWindow
{
    [MenuItem("Tools/Dialog Box/Generate UI")]
    public static void GenerateDialogueUI()
    {
        // 1. Tìm hoặc tạo Canvas
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
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // 2. Tìm hoặc tạo EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 3. Tạo Hộp Thoại (Dialogue Panel)
        GameObject dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
        Image panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // Đặt vị trí panel ở dưới màn hình
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(1200, 250);
        panelRect.anchoredPosition = new Vector2(0, 50);

        // 4. Tạo Text hiển thị Tên (Name Text)
        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(dialoguePanel.transform, false);
        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Tên Nhân Vật";
        nameText.fontSize = 40;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = new Color(1f, 0.84f, 0f); // Màu vàng
        
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.sizeDelta = new Vector2(-60, 60);
        nameRect.anchoredPosition = new Vector2(30, -20);

        // 5. Tạo Text hiển thị Nội dung (Dialogue Text)
        GameObject dialogueTextObj = new GameObject("DialogueText");
        dialogueTextObj.transform.SetParent(dialoguePanel.transform, false);
        TextMeshProUGUI dialogueText = dialogueTextObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "Nội dung cuộc hội thoại sẽ hiển thị ở đây...";
        dialogueText.fontSize = 32;
        dialogueText.color = Color.white;
        dialogueText.enableWordWrapping = true;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform dialogueRect = dialogueText.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0, 0);
        dialogueRect.anchorMax = new Vector2(1, 1);
        dialogueRect.pivot = new Vector2(0.5f, 0.5f);
        dialogueRect.sizeDelta = new Vector2(-60, -100);
        dialogueRect.anchoredPosition = new Vector2(0, -30);

        // 6. Setup Dialogue Manager
        GameObject managerObj = new GameObject("DialogueManager");
        DialogueManager manager = managerObj.AddComponent<DialogueManager>();
        manager.dialoguePanel = dialoguePanel;
        manager.nameText = nameText;
        manager.dialogueText = dialogueText;
        managerObj.AddComponent<AudioSource>();

        // Tắt panel mặc định
        dialoguePanel.SetActive(false);
        
        // Chọn đối tượng vừa tạo trong Editor
        Selection.activeGameObject = managerObj;

        Debug.Log("Đã tạo UI Hộp Thoại thành công!");
    }
}
#endif
