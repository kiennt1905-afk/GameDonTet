using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Sinh ra chữ nổi bay lên theo kiểu World Space khi Player mua hàng.
/// Ví dụ: "Đã mua nếp!", "Đã mua đậu xanh!"
/// Gắn script này lên một GameObject rỗng trong scene (không cần trong Canvas).
/// </summary>
public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }

    [Header("Prefab Settings")]
    [Tooltip("Prefab chứa Canvas (World Space) + TextMeshPro. Nếu để trống, script sẽ tự tạo.")]
    public GameObject floatingTextPrefab;

    [Header("Animation Settings")]
    [Tooltip("Tốc độ bay lên (unit/giây)")]
    public float riseSpeed = 1.5f;

    [Tooltip("Thời gian tồn tại (giây)")]
    public float lifetime = 1.8f;

    [Tooltip("Thời gian bắt đầu fade out (giây, tính từ lúc spawn)")]
    public float fadeStartTime = 1.0f;

    [Header("Text Style")]
    public float fontSize = 4f;
    
    [Tooltip("Góc xoay thêm (theo trục Z). Dùng -90 để quay 90 độ theo chiều kim đồng hồ.")]
    public float zRotationOffset = -90f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawn floating text tại vị trí worldPos với màu chỉ định.
    /// </summary>
    public static void Spawn(string text, Vector3 worldPos, Color color)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[FloatingTextSpawner] Không tìm thấy Instance trong scene!");
            return;
        }
        Instance.StartCoroutine(Instance.AnimateText(text, worldPos, color));
    }

    /// <summary>
    /// Overload tiện dụng dùng màu vàng mặc định.
    /// </summary>
    public static void Spawn(string text, Vector3 worldPos)
    {
        Spawn(text, worldPos, new Color(1f, 0.85f, 0.1f)); // Màu vàng ấm
    }

    private IEnumerator AnimateText(string text, Vector3 startPos, Color color)
    {
        // Tạo World Space Canvas
        GameObject canvasObj = new GameObject("FloatingText_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(4f, 1.5f);
        canvasRect.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        canvasObj.transform.position = startPos + Vector3.up * 1.5f;

        // Tạo TextMeshPro
        GameObject textObj = new GameObject("FloatingText_TMP");
        textObj.transform.SetParent(canvasObj.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false; // Ngăn chữ bị rớt dòng dọc xuống nếu khung quá hẹp

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        float elapsed = 0f;
        Vector3 currentPos = canvasObj.transform.position;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // Bay lên
            currentPos += Vector3.up * riseSpeed * Time.deltaTime;
            canvasObj.transform.position = currentPos;

            // Billboard: luôn quay về phía Camera
            if (Camera.main != null)
            {
                // Áp dụng thêm góc xoay bù (ví dụ -90 độ để quay theo chiều kim đồng hồ)
                canvasObj.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0, 0, zRotationOffset);
            }

            // Fade out
            if (elapsed >= fadeStartTime)
            {
                float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                Color fadedColor = color;
                fadedColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                tmp.color = fadedColor;
            }

            yield return null;
        }

        Destroy(canvasObj);
    }
}
