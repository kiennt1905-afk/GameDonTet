using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Gắn vào TriggerBox giao thừa.
/// Bấm E → phát tối đa 12 giây countdown audio → hiện video pháo hoa toàn màn hình + credit cuộn bằng OnGUI.
/// </summary>
public class Trigger_CountDown : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip countdownClip;
    [Tooltip("Giới hạn thời gian countdown (giây)")]
    public float countdownMaxSeconds = 12f;
    public AudioClip fireworksClip;

    [Header("Video")]
    public VideoClip fireworksVideo;

    [Header("Credit Settings")]
    [Tooltip("Tốc độ cuộn chữ (pixels/giây)")]
    public float creditScrollSpeed = 80f;
    [Tooltip("Dừng cuối trước khi tắt")]
    public float holdAtEndSeconds = 3f;

    // ── Runtime ──
    private bool hasTriggered   = false;
    private bool showOverlay    = false;  // Canvas video đang hiện
    private bool showCredits    = false;  // Đang cuộn credit
    private float creditY       = 0f;    // Vị trí hiện tại (px từ đáy)

    private AudioSource audioSource;
    private RenderTexture renderTex;
    private VideoPlayer videoPlayer;

    // Nội dung credit (cắt thành từng dòng)
    private static readonly string[] CreditLines = new string[]
    {
        "",
        "✦ LỜI CẢM ƠN ✦",
        "",
        "Xin chân thành cảm ơn bạn đã chơi game.",
        "",
        "",
        "THỰC HIỆN BỞI",
        "Nhóm 3",
        "Lập trình, thiết kế và phát triển",
        "",
        "",
        "CÔNG CỤ PHÁT TRIỂN",
        "",
        "Unity",
        "Antigravity",
        "Blender",
        "Mixamo",
        "Tencent HY 3D GLOBAL",
        "Sketchfab",
        "speechgen.io",
        "",
        "",
        "",
        "🎆 Chúc mừng năm mới! 🎆",
        "",
        "",
    };

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void Interact()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // Vô hiệu hoá trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(RunSequence());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Luồng chính
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator RunSequence()
    {
        // 1. Phát countdown audio (tối đa countdownMaxSeconds)
        if (countdownClip != null)
        {
            audioSource.clip = countdownClip;
            audioSource.Play();
            float wait = Mathf.Min(countdownClip.length, countdownMaxSeconds);
            yield return new WaitForSeconds(wait);
            audioSource.Stop();
        }

        // 2. Chuẩn bị video player
        SetupVideo();

        // 3. Hiện overlay video (OnGUI sẽ vẽ nền đen trước khi video bắt đầu)
        showOverlay = true;

        if (videoPlayer != null)
            videoPlayer.Play();

        // 4. Phát audio pháo hoa
        if (fireworksClip != null)
        {
            audioSource.clip = fireworksClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 5. Đợi 0.5s rồi bắt đầu cuộn credit
        yield return new WaitForSeconds(0.5f);
        creditY = -Screen.height;   // bắt đầu từ dưới màn hình
        showCredits = true;

        // 6. Cuộn credit và đồng thời đếm 40 giây video
        float fireworksDuration = 40f;
        float elapsed = 0f;

        while (elapsed < fireworksDuration)
        {
            creditY += creditScrollSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 7. Load scene MainMenu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void SetupVideo()
    {
        if (fireworksVideo == null) return;

        renderTex = new RenderTexture(Screen.width, Screen.height, 0);

        GameObject vpGO = new GameObject("CountDown_VideoPlayer");
        DontDestroyOnLoad(vpGO);

        videoPlayer = vpGO.AddComponent<VideoPlayer>();
        videoPlayer.renderMode    = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTex;
        videoPlayer.clip          = fireworksVideo;
        videoPlayer.isLooping     = true;
        videoPlayer.playOnAwake   = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // audio riêng
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Update cuộn credit mỗi frame
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ScrollCredits()
    {
        const float lineHeight   = 40f;
        float totalHeight = CreditLines.Length * lineHeight + Screen.height;
        float targetY     = totalHeight; // cuộn đến khi hết chữ

        while (creditY < targetY)
        {
            creditY += creditScrollSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(holdAtEndSeconds);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  OnGUI – vẽ video + credit lên màn hình
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        if (!showOverlay) return;

        GUI.depth = -2000; // Đè lên mọi thứ

        // ── Nền đen (backup khi video chưa kịp phát) ─────────────────────────
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Video pháo hoa ────────────────────────────────────────────────────
        if (renderTex != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTex,
                ScaleMode.ScaleToFit, false);
        }

        if (!showCredits) return;

        // ── Lớp mờ để chữ dễ đọc ─────────────────────────────────────────────
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Vẽ từng dòng credit ───────────────────────────────────────────────
        const float lineHeight = 40f;
        float centerX  = Screen.width  * 0.5f;
        float baseY    = Screen.height - creditY; // creditY âm = chưa xuất hiện

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment  = TextAnchor.MiddleCenter,
            fontSize   = 36,
            fontStyle  = FontStyle.Bold,
            normal     = { textColor = Color.yellow },
        };

        GUIStyle bigStyle = new GUIStyle(headerStyle)
        {
            fontSize = 28,
            normal   = { textColor = Color.white },
        };

        GUIStyle normalStyle = new GUIStyle(bigStyle)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Normal,
        };

        GUIStyle finaleStyle = new GUIStyle(headerStyle)
        {
            fontSize = 46,
            normal   = { textColor = Color.yellow },
        };

        const float lineW = 700f;

        for (int i = 0; i < CreditLines.Length; i++)
        {
            float y = baseY + i * lineHeight;

            // Chỉ vẽ khi dòng nằm trong vùng màn hình
            if (y < -lineHeight || y > Screen.height + lineHeight) continue;

            string line = CreditLines[i];
            Rect lineRect = new Rect(centerX - lineW * 0.5f, y, lineW, lineHeight);

            // Chọn style theo nội dung
            GUIStyle style;
            if (line.StartsWith("✦") || line.StartsWith("🎆"))
                style = finaleStyle;
            else if (line == line.ToUpper() && line.Length > 2 && !line.Contains("🎆"))
                style = headerStyle;  // Chữ HOA = tiêu đề
            else
                style = normalStyle;

            // Vẽ bóng đen
            GUIStyle shadow = new GUIStyle(style);
            shadow.normal.textColor = new Color(0, 0, 0, 0.7f);
            GUI.Label(new Rect(lineRect.x + 2, lineRect.y + 2, lineRect.width, lineRect.height), line, shadow);

            // Vẽ chữ chính
            GUI.Label(lineRect, line, style);
        }
    }
}
