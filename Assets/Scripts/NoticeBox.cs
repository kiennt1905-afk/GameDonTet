using UnityEngine;

/// <summary>
/// Gắn script này vào các Trigger Box nếu bạn muốn thay đổi nội dung hiển thị chữ.
/// Nếu không gắn, PlayerInteract sẽ tự động hiển thị "[E] Tương tác" mặc định.
/// </summary>
public class NoticeBox : MonoBehaviour
{
    [Header("Nội dung")]
    [Tooltip("Nội dung hiển thị trên màn hình. Mặc định là '[E] Tương tác'.")]
    public string promptText = "[E] Tương tác";

    [Header("Floating Text (Bay lên khi bấm E)")]
    [Tooltip("Chữ bay lên khi tương tác. Để trống nếu không muốn có hiệu ứng này.")]
    public string floatingText = "";
    
    [Tooltip("Thời gian hiển thị của chữ (giây)")]
    public float floatTime = 1.5f;
    
    [Tooltip("Tốc độ chữ bay lên")]
    public float floatSpeed = 40f;

    // Trạng thái nội bộ
    private bool isFloating = false;
    private float currentFloatY = 0f;
    private float floatTimer = 0f;

    /// <summary>
    /// Kích hoạt hiệu ứng chữ bay lên, hàm này được PlayerInteract gọi khi người chơi bấm E
    /// </summary>
    public void ShowFloatingText()
    {
        if (string.IsNullOrEmpty(floatingText)) return;
        ShowFloatingText(floatingText);
    }

    /// <summary>
    /// Kích hoạt floating text với nội dung tùy ý (dùng bởi Trigger_Donate, v.v.)
    /// </summary>
    public void ShowFloatingText(string text)
    {
        floatingText = text;
        isFloating = true;
        currentFloatY = 0f;
        floatTimer = floatTime;
    }

    private void Update()
    {
        if (isFloating)
        {
            floatTimer -= Time.deltaTime;
            currentFloatY += floatSpeed * Time.deltaTime; 

            if (floatTimer <= 0)
            {
                isFloating = false;
            }
        }
    }

    private void OnGUI()
    {
        if (!isFloating) return;

        // Vị trí gốc là 65% màn hình ngang, cao hơn hộp [E] Tương tác một chút (55% dọc - 30px)
        float centerX = Screen.width * 0.65f;
        float startY = (Screen.height * 0.55f) - 30f; 
        
        // Trừ currentFloatY để đẩy chữ đi lên trên (vì toạ độ Y của GUI từ trên xuống)
        float actualY = startY - currentFloatY; 

        const int boxWidth = 300;
        const int boxHeight = 40;
        Rect rect = new Rect(centerX - boxWidth * 0.5f, actualY - boxHeight * 0.5f, boxWidth, boxHeight);

        GUI.depth = -1001; // Đè lên trên giao diện khác

        // Tạo hiệu ứng mờ dần (Alpha fade out) theo thời gian
        float alpha = Mathf.Clamp01(floatTimer / floatTime);
        GUI.color = new Color(1f, 1f, 1f, alpha);

        // Style chữ chính
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.red } // Đổi sang chữ màu đỏ
        };

        // Style bóng đổ đen
        GUIStyle shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = new Color(0, 0, 0, alpha);

        // Vẽ bóng đen lệch xuống dưới phải 1 chút cho chữ dễ đọc
        Rect shadowRect = new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height);
        GUI.Label(shadowRect, floatingText, shadowStyle);

        // Vẽ chữ chính thức
        GUI.Label(rect, floatingText, style);

        // Reset màu GUI về gốc
        GUI.color = Color.white;
    }
}
