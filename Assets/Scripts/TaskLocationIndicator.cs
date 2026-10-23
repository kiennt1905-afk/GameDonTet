using System;
using System.Collections.Generic;
using UnityEngine;

// Định nghĩa một cấu trúc để lưu trữ thông tin của từng Box
[Serializable]
public class TaskIndicatorBox
{
    [Tooltip("GameObject muốn hiển thị (Icon, Box, Particle, 3D Object...)")]
    public GameObject boxObject;
    
    [Tooltip("ID của task yêu cầu. Nhập 'show' để luôn hiển thị.")]
    public string requireTaskId;
}

public class TaskLocationIndicator : MonoBehaviour
{
    [Header("Cài đặt Box Chỉ Đường")]
    // List này cho phép bạn chỉnh số lượng box thoải mái trên Inspector
    public List<TaskIndicatorBox> indicatorBoxes = new List<TaskIndicatorBox>();

    private void Start()
    {
        // Ban đầu ẩn tất cả các box (ngoại trừ những box có require = "show")
        HideAllAndCheckShow();

        // Tự động đồng bộ với TaskManager ngay khi sinh ra
        // Việc này sửa lỗi "Box 1 không hiện" do script này chạy Start() SAU TaskManager
        // hoặc khi load sang scene mới mà TaskManager không chạy lại Start().
        if (TaskManager.Instance != null)
        {
            string currentTask = TaskManager.Instance.GetCurrentActiveTaskId();
            if (!string.IsNullOrEmpty(currentTask))
            {
                OnTaskActive(currentTask);
            }
        }
    }

    /// <summary>
    /// Gọi hàm này khi người chơi NHẬN (active) một task mới
    /// </summary>
    /// <param name="activeTaskId">ID của task vừa được kích hoạt</param>
    public void OnTaskActive(string activeTaskId)
    {
        foreach (var indicator in indicatorBoxes)
        {
            if (indicator.boxObject == null) continue;

            // Nếu ID trùng khớp với task đang active thì hiển thị box lên
            if (string.Equals(indicator.requireTaskId, activeTaskId, StringComparison.OrdinalIgnoreCase))
            {
                indicator.boxObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Gọi hàm này khi người chơi HOÀN THÀNH một task
    /// </summary>
    /// <param name="completedTaskId">ID của task vừa hoàn thành</param>
    public void OnTaskCompleted(string completedTaskId)
    {
        foreach (var indicator in indicatorBoxes)
        {
            if (indicator.boxObject == null) continue;

            // Nếu ID trùng khớp với task vừa hoàn thành thì ẩn box đó đi
            if (string.Equals(indicator.requireTaskId, completedTaskId, StringComparison.OrdinalIgnoreCase))
            {
                indicator.boxObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hàm tiện ích: Cập nhật lại toàn bộ trạng thái (Dùng khi load save game để check)
    /// </summary>
    /// <param name="currentActiveTaskIds">Danh sách các task đang active của người chơi</param>
    public void UpdateAllIndicators(List<string> currentActiveTaskIds)
    {
        foreach (var indicator in indicatorBoxes)
        {
            if (indicator.boxObject == null) continue;

            // Nếu require = "show" hoặc list task hiện tại chứa ID yêu cầu thì hiển thị
            if (string.Equals(indicator.requireTaskId, "show", StringComparison.OrdinalIgnoreCase) || 
                currentActiveTaskIds.Contains(indicator.requireTaskId))
            {
                indicator.boxObject.SetActive(true);
            }
            else
            {
                indicator.boxObject.SetActive(false);
            }
        }
    }

    private void HideAllAndCheckShow()
    {
        foreach (var indicator in indicatorBoxes)
        {
            if (indicator.boxObject != null)
            {
                // Chỉ hiển thị nếu require = "show", còn lại ẩn hết
                bool shouldShow = string.Equals(indicator.requireTaskId, "show", StringComparison.OrdinalIgnoreCase);
                indicator.boxObject.SetActive(shouldShow);
            }
        }
    }
}
