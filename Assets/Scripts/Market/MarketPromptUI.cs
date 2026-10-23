using UnityEngine;
using TMPro;

/// <summary>
/// Singleton quản lý UI prompt hiện lên khi Player vào vùng trigger quầy hàng.
/// Hiện text kiểu "[E] Mua nếp" ở góc dưới giữa màn hình.
/// Gắn script này lên GameObject chứa Panel prompt trong Canvas.
/// </summary>
public class MarketPromptUI : MonoBehaviour
{
    public static MarketPromptUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Panel chứa prompt, sẽ được bật/tắt")]
    public GameObject promptPanel;

    [Tooltip("TextMeshPro hiển thị nội dung prompt")]
    public TextMeshProUGUI promptText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Cập nhật references từ scene mới
            Instance.promptPanel = this.promptPanel;
            Instance.promptText = this.promptText;
            Destroy(gameObject);
            return;
        }

        // Ẩn panel lúc khởi đầu
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    /// <summary>
    /// Hiện prompt với nội dung chỉ định. Gọi khi Player vào TriggerBox quầy hàng.
    /// </summary>
    /// <param name="text">Ví dụ: "Mua nếp", "Mua thịt"</param>
    public static void Show(string text)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[MarketPromptUI] Không tìm thấy Instance trong scene!");
            return;
        }

        if (Instance.promptText != null)
            Instance.promptText.text = $"[E]  {text}";

        if (Instance.promptPanel != null)
            Instance.promptPanel.SetActive(true);
    }

    /// <summary>
    /// Ẩn prompt. Gọi khi Player ra khỏi TriggerBox.
    /// </summary>
    public static void Hide()
    {
        if (Instance == null) return;

        if (Instance.promptPanel != null)
            Instance.promptPanel.SetActive(false);
    }
}
