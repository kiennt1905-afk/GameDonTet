using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class GameTask
{
    public string id;           // ID dùng để gọi khi hoàn thành (vd: "PaperNote", "redevelop")
    public string description;  // Nội dung hiển thị (vd: "Đọc tin nhắn của mẹ")
    public bool isCompleted;    // Trạng thái hoàn thành
    [HideInInspector]
    public bool isHidden;       // Đã ẩn khỏi UI
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI Reference")]
    [Tooltip("Kéo TextMeshProUGUI của bạn vào đây")]
    public TextMeshProUGUI taskTextUI;

    [Header("Task List")]
    public List<GameTask> tasks = new List<GameTask>();

    private void Awake()
    {
        // Setup Singleton để dễ dàng gọi từ các script khác
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Update UI reference from the new scene
            Instance.taskTextUI = this.taskTextUI;
            Instance.UpdateUI();
            
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Khởi tạo các nhiệm vụ ban đầu nếu danh sách trống (bạn cũng có thể setup trực tiếp trên Inspector)
        if (tasks.Count == 0)
        {
            tasks.Add(new GameTask { id = "PaperNote",     description = "Đọc tin nhắn của mẹ",           isCompleted = false });
            tasks.Add(new GameTask { id = "redevelop",     description = "Nhận tiền đi chợ",              isCompleted = false });
            tasks.Add(new GameTask { id = "buy_nep",       description = "Mua nếp",                       isCompleted = false });
            tasks.Add(new GameTask { id = "buy_dau_xanh",  description = "Mua đậu xanh",                  isCompleted = false });
            tasks.Add(new GameTask { id = "buy_thit_1",    description = "Mua thịt",                      isCompleted = false });
            tasks.Add(new GameTask { id = "buy_la_lac",    description = "Mua lá chuối và lạc buộc",      isCompleted = false });
            tasks.Add(new GameTask { id = "buy_thit_2",    description = "Mua thịt (chờ chú chặt)",       isCompleted = false });
        }
        
        UpdateUI();

        // Thông báo task đang active đầu tiên cho tất cả các Indicator trong scene
        string firstActiveId = GetCurrentActiveTaskId();
        if (firstActiveId != null)
        {
            NotifyIndicatorsActive(firstActiveId);
        }
    }

    /// <summary>
    /// Kiểm tra xem task có đang ACTIVE không (tức là task đó là task đầu tiên chưa hoàn thành).
    /// Dùng bởi MarketStallInteractable để chỉ cho phép tương tác khi đúng thứ tự.
    /// </summary>
    public bool IsTaskActive(string taskId)
    {
        if (string.IsNullOrEmpty(taskId)) return false;
        foreach (var task in tasks)
        {
            if (!task.isCompleted)
            {
                // Task chưa hoàn thành đầu tiên — đây là task đang active
                return task.id == taskId;
            }
        }
        return false; // Tất cả đã hoàn thành
    }

    /// <summary>
    /// Gọi hàm này từ script tương tác của bạn.
    /// Ví dụ: TaskManager.Instance.CompleteTask("PaperNote");
    /// </summary>
    public void CompleteTask(string taskId)
    {
        bool taskUpdated = false;
        foreach (var task in tasks)
        {
            if (task.id == taskId && !task.isCompleted)
            {
                task.isCompleted = true;
                taskUpdated = true;

                // Ẩn box của task vừa hoàn thành
                NotifyIndicatorsCompleted(taskId);

                // Bắt đầu đếm ngược 5 giây để ẩn task
                StartCoroutine(HideTaskAfterDelay(task, 5f));
                break;
            }
        }

        // Chỉ cập nhật UI nếu có nhiệm vụ vừa hoàn thành
        if (taskUpdated)
        {
            // Báo cho UI chỉ hiện chữ (hoàn thành), khoan hiện task tiếp theo
            UpdateUI(false);
            
            // Đợi 1 giây rồi mới hiện task tiếp theo
            StartCoroutine(WaitAndShowNextTask(1f));

            // Hiện box của task tiếp theo (nếu có)
            string nextActiveId = GetCurrentActiveTaskId();
            if (nextActiveId != null)
            {
                NotifyIndicatorsActive(nextActiveId);
            }
        }
    }

    private System.Collections.IEnumerator HideTaskAfterDelay(GameTask task, float delay)
    {
        yield return new WaitForSeconds(delay);
        task.isHidden = true;
        UpdateUI(true); // Cập nhật lại UI sau khi ẩn
    }
    
    private System.Collections.IEnumerator WaitAndShowNextTask(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateUI(true); // Cập nhật lại UI và cho phép hiện task tiếp theo
    }

    /// <summary>
    /// Cập nhật nội dung text trên UI
    /// </summary>
    private void UpdateUI(bool showNextTask = true)
    {
        if (taskTextUI == null)
        {
            Debug.LogWarning("Chưa gán taskTextUI cho TaskManager, hoặc UI ở scene trước đã bị huỷ! Hãy thêm prefab TaskManager vào scene này.");
            return;
        }

        string uiText = "<b>Nhiệm vụ:</b>\n";
        foreach (var task in tasks)
        {
            if (task.isHidden)
            {
                continue; // Bỏ qua task đã bị ẩn
            }

            if (task.isCompleted)
            {
                // Thay đổi màu xám và thêm text "(hoàn thành)"
                uiText += $"<color=#00FF00>- {task.description} (hoàn thành)</color>\n";
            }
            else
            {
                // Nếu được phép hiển thị task tiếp theo thì mới in ra
                if (showNextTask)
                {
                    uiText += $"- {task.description}\n";
                }
                
                // Dừng vòng lặp tại đây để KHÔNG hiển thị các nhiệm vụ sau nó
                break;
            }
        }
        
        taskTextUI.text = uiText;
    }

    // ─────────────────────────────────────────────────────
    //  Các hàm hỗ trợ TaskLocationIndicator
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Trả về ID của task đầu tiên chưa hoàn thành (task đang active).
    /// </summary>
    public string GetCurrentActiveTaskId()
    {
        foreach (var task in tasks)
        {
            if (!task.isCompleted)
                return task.id;
        }
        return null;
    }

    /// <summary>
    /// Trả về danh sách ID tất cả task chưa hoàn thành. Dùng cho UpdateAllIndicators khi load save.
    /// </summary>
    public List<string> GetActiveTaskIds()
    {
        var activeIds = new List<string>();
        foreach (var task in tasks)
        {
            if (!task.isCompleted)
                activeIds.Add(task.id);
        }
        return activeIds;
    }

    /// <summary>
    /// Tìm tất cả TaskLocationIndicator trong scene và báo hiệu task active.
    /// </summary>
    private void NotifyIndicatorsActive(string taskId)
    {
        var indicators = FindObjectsByType<TaskLocationIndicator>(FindObjectsSortMode.None);
        foreach (var indicator in indicators)
        {
            indicator.OnTaskActive(taskId);
        }
    }

    /// <summary>
    /// Tìm tất cả TaskLocationIndicator trong scene và báo hiệu task hoàn thành.
    /// </summary>
    private void NotifyIndicatorsCompleted(string taskId)
    {
        var indicators = FindObjectsByType<TaskLocationIndicator>(FindObjectsSortMode.None);
        foreach (var indicator in indicators)
        {
            indicator.OnTaskCompleted(taskId);
        }
    }
}

