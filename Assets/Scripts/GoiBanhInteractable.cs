using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GoiBanhInteractable : MonoBehaviour
{
    [Header("Cấu hình Chuyển Cảnh")]
    public string minigameSceneName = "Minigame";
    public string promptText = "Bấm E để gói bánh";

    [Header("Cấu hình Viền Trắng (Outline)")]
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.006f;

    private Renderer meshRenderer;
    private Material[] originalMaterials;
    private Material outlineMaterial;
    private bool isHovered = false;

    // Canvas UI Prompt Static Shared Instance
    private static GameObject uiPromptInstance;
    private static TextMeshProUGUI uiPromptText;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            originalMaterials = meshRenderer.sharedMaterials;
        }

        Shader outlineShader = Shader.Find("Custom/URPOutline");
        if (outlineShader != null)
        {
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    private void Update()
    {
        // Kiểm tra raycast từ Camera nếu đang ở chế độ First Person (khóa chuột)
        CheckCameraRaycast();

        // Xử lý nhấn phím E khi đang trỏ vào vật phẩm
        if (isHovered)
        {
            bool ePressed = false;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ePressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ePressed = true;
            }

            if (ePressed)
            {
                Interact();
            }
        }
    }

    private void CheckCameraRaycast()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Tia bắn từ giữa màn hình
            ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            // Tia bắn từ con trỏ chuột
            ray = mainCam.ScreenPointToRay(Input.mousePosition);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                if (!isHovered) SetHoverState(true);
                return;
            }
        }

        // Nếu không hit vật thể này và không phải đang MouseEnter thì tắt hover
        if (isHovered && Cursor.lockState == CursorLockMode.Locked)
        {
            SetHoverState(false);
        }
    }

    private void OnMouseEnter()
    {
        SetHoverState(true);
    }

    private void OnMouseExit()
    {
        SetHoverState(false);
    }

    public void SetHoverState(bool hover)
    {
        if (isHovered == hover) return;
        isHovered = hover;

        ApplyOutline(isHovered);
        ShowUIPrompt(isHovered);
    }

    private void ApplyOutline(bool enable)
    {
        if (meshRenderer == null || outlineMaterial == null) return;

        if (enable)
        {
            var mats = meshRenderer.sharedMaterials;
            // Kiểm tra xem đã có outline material chưa
            bool exists = false;
            foreach (var m in mats)
            {
                if (m != null && m.shader != null && m.shader.name == "Custom/URPOutline")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                Material[] newMats = new Material[mats.Length + 1];
                for (int i = 0; i < mats.Length; i++)
                {
                    newMats[i] = mats[i];
                }
                newMats[mats.Length] = outlineMaterial;
                meshRenderer.materials = newMats;
            }
        }
        else
        {
            if (originalMaterials != null && originalMaterials.Length > 0)
            {
                meshRenderer.materials = originalMaterials;
            }
        }
    }

    private void ShowUIPrompt(bool show)
    {
        EnsureUIPromptCreated();

        if (uiPromptInstance != null)
        {
            uiPromptInstance.SetActive(show);
            if (show && uiPromptText != null)
            {
                uiPromptText.text = promptText;
            }
        }
    }

    private void EnsureUIPromptCreated()
    {
        if (uiPromptInstance != null) return;

        // Tìm Canvas có sẵn trong Scene hoặc tạo mới
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GoiBanh_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Tạo Panel Prompt
        uiPromptInstance = new GameObject("PromptPanel");
        uiPromptInstance.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = uiPromptInstance.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.2f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(280f, 50f);

        UnityEngine.UI.Image bgImage = uiPromptInstance.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);

        // Tạo Text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(uiPromptInstance.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        uiPromptText = textObj.AddComponent<TextMeshProUGUI>();
        uiPromptText.text = promptText;
        uiPromptText.alignment = TextAlignmentOptions.Center;
        uiPromptText.fontSize = 22;
        uiPromptText.color = Color.white;
        uiPromptText.fontStyle = FontStyles.Bold;

        uiPromptInstance.SetActive(false);
    }

    public void Interact()
    {
        // Tắt Hover state & UI Prompt
        SetHoverState(false);

        // Lưu vị trí người chơi nếu có
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            var pc = FindObjectOfType<FirstPersonController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player != null)
        {
            Vector3 returnPos = player.transform.position;
            PlayerPrefs.SetFloat("PlayerPosX", returnPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", returnPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", returnPos.z);
            PlayerPrefs.SetFloat("PlayerRotY", player.transform.rotation.eulerAngles.y);
            PlayerPrefs.SetInt("HasSavedPos", 1);
        }

        // Lưu tên Scene hiện tại để sau khi chơi hết giờ tự quay lại đúng Scene này
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("ReturnScene", currentScene);
        PlayerPrefs.Save();

        Debug.Log("Đã bấm E gói bánh -> Chuyển đến Scene: " + minigameSceneName);
        SceneManager.LoadScene(minigameSceneName);
    }

    private void OnDisable()
    {
        SetHoverState(false);
    }
}
