using UnityEngine;

/// <summary>
/// Gắn vào TriggerBox_Shopping1.
/// Khi Player vào vùng trigger, bấm E → mở dialog → hoàn thành task "shopping1".
/// </summary>
public class ShoppingDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Content")]
    [Tooltip("Các dòng thoại (text + audio) sẽ hiện theo thứ tự")]
    public DialogueLine[] dialogueLines;

    [Header("Task Settings")]
    [Tooltip("ID của task cần đánh dấu hoàn thành sau khi kết thúc dialog")]
    public string taskIdToComplete = "shopping1";

    [Tooltip("Chỉ hoàn thành task 1 lần duy nhất")]
    public bool completeOnce = true;

    private bool _taskCompleted = false;

    /// <summary>
    /// Được gọi bởi PlayerInteract khi người chơi bấm E trong trigger.
    /// </summary>
    public void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[ShoppingDialogueTrigger] Không tìm thấy DialogueManager trong Scene!");
            return;
        }

        // Bắt đầu dialog; khi dialog kết thúc sẽ hoàn thành task
        DialogueManager.Instance.StartDialogue(dialogueLines, OnDialogueEnd);
    }

    private void OnDialogueEnd()
    {
        if (completeOnce && _taskCompleted) return;

        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            _taskCompleted = true;
            Debug.Log($"[ShoppingDialogueTrigger] Task '{taskIdToComplete}' đã được hoàn thành.");
        }
        else
        {
            Debug.LogWarning("[ShoppingDialogueTrigger] Không tìm thấy TaskManager hoặc taskId trống.");
        }
    }
}
