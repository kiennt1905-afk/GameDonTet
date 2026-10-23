using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PaperNote : MonoBehaviour
{
    [TextArea(5, 10)]
    public string noteContent = "Nội dung tờ giấy của bạn ở đây...";

    [Header("UI Reference")]
    public GameObject noteUIPanel;
    public TextMeshProUGUI noteTextDisplay;

    [Header("Task Settings")]
    [Tooltip("Nhập ID của Task muốn hoàn thành khi đọc tờ giấy này (Vd: Note)")]
    public string taskIdToComplete;

    private PlayerController playerController;
    private bool isReading = false;

    void Start()
    {
        // Tự động tìm GameObject có tên "Player (1)" và lấy script PlayerController
        GameObject playerObj = GameObject.Find("Player (1)");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
        
        // Đảm bảo UI đọc giấy tắt lúc bắt đầu game
        if (noteUIPanel != null)
        {
            noteUIPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Kiểm tra bàn phím
        if (Keyboard.current == null) return;

        // Bấm Esc để thoát (Phím E giờ do PlayerInteract quản lý để tránh kẹt phím)
        if (isReading)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseNote();
            }
        }
    }

    // Hàm này giờ sẽ đóng vai trò Bật/Tắt tờ giấy
    public void ReadNote()
    {
        if (isReading)
        {
            CloseNote();
            return;
        }

        isReading = true;
        
        // Gọi TaskManager để hoàn thành nhiệm vụ
        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
        }
        
        // Hiện giao diện tờ giấy
        if (noteUIPanel != null)
        {
            noteUIPanel.SetActive(true);
            if (noteTextDisplay != null)
            {
                noteTextDisplay.text = noteContent;
            }
        }

        // Tạm dừng việc di chuyển và quay camera của người chơi
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Hiện con trỏ chuột ra
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseNote()
    {
        isReading = false;

        // Ẩn giao diện
        if (noteUIPanel != null)
        {
            noteUIPanel.SetActive(false);
        }

        // Bật lại việc di chuyển và quay camera
        if (playerController != null)
        {
            playerController.enabled = true;
            
            // Khóa lại chuột
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
