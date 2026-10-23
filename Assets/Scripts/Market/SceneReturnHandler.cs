using UnityEngine;

/// <summary>
/// Đặt script này lên Player (hoặc một GameObject persistent) trong scene Cho.
/// 
/// Mục đích: sau khi Player hoàn thành mini-game Ô Ăn Quan và quay về scene Cho,
/// script sẽ tự động hoàn thành task đang pending (được lưu qua PlayerPrefs).
/// 
/// Cách hoạt động:
///   1. Trước khi chuyển sang scene mini-game, OAnQuanTransition1 lưu:
///      PlayerPrefs.SetString("PendingTaskComplete", "buy_thit_2")
///   2. Khi scene Cho load lại, Start() của script này sẽ đọc và gọi CompleteTask.
/// </summary>
public class SceneReturnHandler : MonoBehaviour
{
    private const string PENDING_TASK_KEY = "PendingTaskComplete";

    private void Start()
    {
        CheckPendingTask();
        RestorePlayerPosition();
    }

    /// <summary>
    /// Kiểm tra và hoàn thành task pending nếu có.
    /// </summary>
    private void CheckPendingTask()
    {
        if (!PlayerPrefs.HasKey(PENDING_TASK_KEY)) return;

        string taskId = PlayerPrefs.GetString(PENDING_TASK_KEY);
        PlayerPrefs.DeleteKey(PENDING_TASK_KEY);
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(taskId) && TaskManager.Instance != null)
        {
            Debug.Log($"[SceneReturnHandler] Hoàn thành pending task: '{taskId}'");
            TaskManager.Instance.CompleteTask(taskId);
        }
        else
        {
            Debug.LogWarning($"[SceneReturnHandler] Pending task '{taskId}' nhưng TaskManager chưa sẵn sàng!");
        }
    }

    /// <summary>
    /// Khôi phục vị trí Player đã lưu trước khi vào mini-game (nếu có).
    /// </summary>
    private void RestorePlayerPosition()
    {
        if (PlayerPrefs.GetInt("HasSavedPos", 0) != 1) return;

        float x = PlayerPrefs.GetFloat("PlayerPosX", 0f);
        float y = PlayerPrefs.GetFloat("PlayerPosY", 0f);
        float z = PlayerPrefs.GetFloat("PlayerPosZ", 0f);

        // Tìm Player theo tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(x, y, z);
            Debug.Log($"[SceneReturnHandler] Đã khôi phục vị trí Player: ({x:F1}, {y:F1}, {z:F1})");
        }

        PlayerPrefs.DeleteKey("HasSavedPos");
        PlayerPrefs.Save();
    }
}
