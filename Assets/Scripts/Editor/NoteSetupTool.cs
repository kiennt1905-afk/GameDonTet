using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class NoteSetupTool : EditorWindow
{
    [MenuItem("Tools/Tạo Nhanh Note và UI (Đọc Giấy)")]
    public static void GenerateNoteSetup()
    {
        // 1. Tìm hoặc tạo Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Tạo UI Panel cho tờ giấy
        GameObject noteUIPanel = new GameObject("NoteUIPanel");
        noteUIPanel.transform.SetParent(canvas.transform, false);
        Image panelImage = noteUIPanel.AddComponent<Image>();
        panelImage.color = new Color(0.99f, 0.96f, 0.9f, 0.95f); // Màu giấy hơi vàng
        
        RectTransform panelRect = noteUIPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 400);

        // 3. Tạo UI Text (TextMeshPro)
        GameObject noteTextObj = new GameObject("NoteText");
        noteTextObj.transform.SetParent(noteUIPanel.transform, false);
        TextMeshProUGUI noteText = noteTextObj.AddComponent<TextMeshProUGUI>();
        noteText.text = "Nội dung tờ giấy...";
        noteText.color = Color.black;
        noteText.fontSize = 24;
        noteText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = noteTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 20); // padding
        textRect.offsetMax = new Vector2(-20, -20);

        // Tạm ẩn panel
        noteUIPanel.SetActive(false);

        // 4. Tìm Player và Camera để gắn PlayerInteract
        GameObject playerObj = GameObject.Find("Player (1)");
        if (playerObj != null)
        {
            Camera playerCam = playerObj.GetComponentInChildren<Camera>();
            if (playerCam != null)
            {
                if (playerCam.gameObject.GetComponent<PlayerInteract>() == null)
                {
                    playerCam.gameObject.AddComponent<PlayerInteract>();
                    Debug.Log("Đã gắn PlayerInteract vào Camera của Player.");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Camera con trong Player!");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameObject tên là 'Player (1)' trong Scene.");
        }

        // 5. Tạo 1 Object tờ giấy 3D trên Scene
        GameObject paperCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paperCube.name = "PaperNoteObject";
        paperCube.transform.localScale = new Vector3(0.5f, 0.02f, 0.6f);
        
        // Đặt phía trước camera nếu có, không thì đặt ở gốc tọa độ
        if (Camera.main != null)
        {
            paperCube.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        }

        // Gắn script PaperNote và liên kết UI
        PaperNote noteScript = paperCube.AddComponent<PaperNote>();
        noteScript.noteUIPanel = noteUIPanel;
        noteScript.noteTextDisplay = noteText;
        noteScript.noteContent = "Bạn có thể sửa nội dung này trong Inspector!";

        // Chọn luôn tờ giấy vừa tạo để dễ di chuyển
        Selection.activeGameObject = paperCube;

        Debug.Log("Đã tạo thành công UI, Tờ giấy và thiết lập Script!");
    }
}
