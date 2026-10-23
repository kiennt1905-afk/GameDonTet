using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    [Header("Tên Scene Muốn Chuyển Tới")]
    [Tooltip("Ghi chính xác tên Scene. Ví dụ: Racing, Cho, House...")]
    public string sceneToLoad = "Cho";

    [Header("Task Settings")]
    [Tooltip("ID của task sẽ đánh dấu hoàn thành khi tương tác cửa.\nĐể trống nếu không cần hoàn thành task.")]
    public string taskIdToComplete = "";

    public void Interact()
    {
        // Hoàn thành task nếu có
        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            Debug.Log($"[DoorInteract] Đã hoàn thành task: '{taskIdToComplete}'");
        }

        Debug.Log("[DoorInteract] Đang tải Scene: " + sceneToLoad);
        // Lưu ý: Cần phải add Scene này vào File -> Build Settings thì mới load được nhé!
        SceneManager.LoadScene(sceneToLoad);
    }
}
