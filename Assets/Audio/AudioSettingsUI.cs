using UnityEngine;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Kéo file hình audio_icon.png của bạn vào đây")]
    public Texture2D audioIcon; 
    
    private AudioSource backgroundAudioSource;
    private bool showSlider = false;

    void Start()
    {
        // Tự động tìm AudioSource từ object BackgroundMusic
        GameObject bgMusicObj = GameObject.Find("BackgroundMusic");
        if (bgMusicObj != null)
        {
            backgroundAudioSource = bgMusicObj.GetComponent<AudioSource>();
        }
    }

    void OnGUI()
    {
        // Nếu không tìm thấy nguồn phát nhạc, không hiển thị UI
        if (backgroundAudioSource == null) return;

        // Thiết lập kích thước icon là 50x50 pixel, cách lề 20 pixel
        float iconSize = 60f;
        float padding = 20f;
        
        // Tạo một khung chữ nhật (Rect) ở góc trên bên phải màn hình
        Rect buttonRect = new Rect(Screen.width - iconSize - padding, padding, iconSize, iconSize);

        // Tạo một style để nút bấm trong suốt, chỉ hiện hình ảnh icon
        GUIStyle iconStyle = new GUIStyle(GUI.skin.button);
        iconStyle.normal.background = null; 
        iconStyle.active.background = null;
        iconStyle.hover.background = null;

        // Vẽ nút bấm. Nếu người chơi bấm vào thì bật/tắt thanh trượt
        if (GUI.Button(buttonRect, audioIcon, iconStyle))
        {
            showSlider = !showSlider;
        }

        // Nếu cờ showSlider đang bật, vẽ thanh trượt âm lượng
        if (showSlider)
        {
            float sliderWidth = 20f;
            float sliderHeight = 150f;
            
            // Đặt thanh trượt ngay bên dưới icon
            Rect sliderRect = new Rect(Screen.width - (iconSize / 2) - (sliderWidth / 2) - padding, padding + iconSize + 10f, sliderWidth, sliderHeight);

            // Vẽ thanh trượt dạng đứng. Giá trị trên cùng là 1.0 (max), dưới cùng là 0.0 (min)
            backgroundAudioSource.volume = GUI.VerticalSlider(sliderRect, backgroundAudioSource.volume, 1.0f, 0.0f);
        }
    }
}
