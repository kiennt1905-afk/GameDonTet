#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUISetup
{
    [MenuItem("Tools/Tạo UI Nhiệm Vụ (Quest)")]
    public static void SetupQuestUI()
    {
        // 1. Kiểm tra / Tạo Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Kiểm tra / Tạo EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Sử dụng Input System mới giống setup cũ của dự án
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(); 
        }

        // 3. Tạo TaskManager GameObject
        TaskManager taskManager = Object.FindObjectOfType<TaskManager>();
        if (taskManager == null)
        {
            GameObject tmObj = new GameObject("TaskManager");
            taskManager = tmObj.AddComponent<TaskManager>();
        }

        // 4. Kiểm tra xóa UI cũ nếu bấm setup nhiều lần
        GameObject oldPanel = GameObject.Find("QuestPanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel);
        }

        // 5. Tạo Panel nền mờ chứa nhiệm vụ
        GameObject questPanel = new GameObject("QuestPanel");
        questPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = questPanel.AddComponent<RectTransform>();
        
        // Neo vào góc trên bên trái màn hình
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20); // Cách mép 20px
        panelRect.sizeDelta = new Vector2(350, 200);

        Image bgImage = questPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.6f); // Nền đen mờ 60%

        // 6. Tạo TextMeshProUGUI hiển thị nhiệm vụ
        GameObject textObj = new GameObject("QuestText");
        textObj.transform.SetParent(questPanel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        
        // Bám theo kích thước Panel, có lề 10px
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(15, 15);
        textRect.offsetMax = new Vector2(-15, -15);

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "<b>Nhiệm vụ:</b>";
        tmpText.fontSize = 24;
        tmpText.color = Color.white;
        tmpText.richText = true;
        tmpText.alignment = TextAlignmentOptions.TopLeft;

        // 7. Tự động gán UI vào TaskManager
        taskManager.taskTextUI = tmpText;

        // Đánh dấu Scene đã thay đổi để có thể lưu lại (Ctrl+S)
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("<color=cyan>Đã setup xong UI Nhiệm Vụ thành công! Hãy kiểm tra trong Canvas.</color>");
    }
}
#endif
