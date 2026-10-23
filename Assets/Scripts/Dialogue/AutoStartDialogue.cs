using UnityEngine;

/// <summary>
/// Script dùng để tự động chạy một đoạn hội thoại (Dialogue) khi load scene.
/// Hỗ trợ chạy 1 lần duy nhất trong suốt quá trình chơi game.
/// </summary>
public class AutoStartDialogue : MonoBehaviour
{
    [Header("Cài đặt Hội thoại")]
    [Tooltip("Kéo script DialogueTrigger chứa nội dung hội thoại vào đây")]
    public DialogueTrigger dialogueTrigger;

    [Tooltip("Thời gian chờ (giây) sau khi load xong scene mới bắt đầu nói")]
    public float delayBeforeStart = 1f;

    [Header("Cài đặt Lặp lại")]
    [Tooltip("Bật cái này nếu chỉ muốn hội thoại chạy ĐÚNG 1 LẦN duy nhất trong cả game (không bị lặp lại khi đi minigame về)")]
    public bool playOnlyOnce = true;
    
    [Tooltip("Tên ID để lưu trạng thái (Mỗi đoạn hội thoại cần 1 ID khác nhau, VD: 'IntroNha', 'GapGoNPC')")]
    public string uniqueDialogueID = "IntroNha_01";

    private void Start()
    {
        Debug.Log($"[AutoStartDialogue] Start() chạy! ID: {uniqueDialogueID}, PlayOnlyOnce: {playOnlyOnce}");

        // Kiểm tra xem đoạn hội thoại này đã từng chạy chưa (nếu yêu cầu chỉ chạy 1 lần)
        if (playOnlyOnce && PlayerPrefs.GetInt(uniqueDialogueID, 0) == 1)
        {
            Debug.Log($"[AutoStartDialogue] Đoạn hội thoại {uniqueDialogueID} đã chạy trước đây. Bỏ qua.");
            return; // Đã chạy rồi -> Thoát luôn
        }

        Debug.Log($"[AutoStartDialogue] Sẽ chạy hội thoại sau {delayBeforeStart} giây...");
        // Đếm ngược thời gian và chạy
        Invoke(nameof(PlayDialogue), delayBeforeStart);
    }

    private void PlayDialogue()
    {
        Debug.Log($"[AutoStartDialogue] Đang gọi PlayDialogue cho ID: {uniqueDialogueID}");
        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerDialogue();

            // Đánh dấu là đã chạy
            if (playOnlyOnce)
            {
                PlayerPrefs.SetInt(uniqueDialogueID, 1);
                PlayerPrefs.Save();
            }
        }
        else
        {
            Debug.LogWarning("[AutoStartDialogue] Chưa gắn DialogueTrigger!");
        }
    }

    [ContextMenu("Reset Lại Trạng Thái (Để test lại)")]
    private void ResetState()
    {
        PlayerPrefs.DeleteKey(uniqueDialogueID);
        PlayerPrefs.Save();
        Debug.Log($"[AutoStartDialogue] Đã xóa lịch sử của ID '{uniqueDialogueID}'. Bạn có thể Play lại game để xem hội thoại chạy.");
    }
}
