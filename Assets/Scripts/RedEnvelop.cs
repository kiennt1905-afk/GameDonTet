using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RedEnvelop : MonoBehaviour
{
    [TextArea(5, 10)]
    public string envelopContent = "Nhận được tiền đi chợ";

    [Header("UI Reference")]
    public GameObject envelopUIPanel;
    public TextMeshProUGUI envelopTextDisplay;

    [Header("Task Settings")]
    [Tooltip("Nhập ID của Task muốn hoàn thành khi mở lì xì (Vd: Shopping)")]
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
        
        // Đảm bảo UI đọc phong bao tắt lúc bắt đầu game
        if (envelopUIPanel != null)
        {
            envelopUIPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Kiểm tra bàn phím
        if (Keyboard.current == null) return;

        // Nếu đang mở bao lì xì và nhấn phím E hoặc Esc thì sẽ đóng lại
        if (isReading)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseEnvelop();
            }
        }
    }

    public void OpenEnvelop()
    {
        if (isReading) return;

        isReading = true;

        // Gọi TaskManager để hoàn thành nhiệm vụ
        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
        }
        
        // Hiện giao diện phong bao
        if (envelopUIPanel != null)
        {
            envelopUIPanel.SetActive(true);
            if (envelopTextDisplay != null)
            {
                envelopTextDisplay.text = envelopContent;
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

    public void CloseEnvelop()
    {
        isReading = false;

        // Ẩn giao diện
        if (envelopUIPanel != null)
        {
            envelopUIPanel.SetActive(false);
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
