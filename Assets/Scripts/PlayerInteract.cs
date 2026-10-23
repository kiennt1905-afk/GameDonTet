using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The object the player is currently standing near")]
    public Collider currentInteractable;

    void Update()
    {
        // Kiểm tra xem bàn phím có được kết nối không
        if (Keyboard.current == null) return;

        // Nhấn E để tương tác (Dùng Input System mới)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                InteractWithObject(currentInteractable);
            }
            else
            {
                Debug.Log("Không có object tương tác nào ở gần.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Khi người chơi đi vào vùng trigger, kiểm tra xem nó có chứa script tương tác không
        bool hasNote = other.GetComponent<PaperNote>() != null;
        bool hasDoor = other.GetComponent<DoorInteract>() != null;
        bool hasEnv = other.GetComponent<RedEnvelop>() != null;
        bool hasDlg = other.GetComponent<DialogueTrigger>() != null;
        bool hasStall = other.GetComponent<MarketStallInteractable>() != null;
        bool hasOAnQuan = other.GetComponent<OAnQuanTransition1>() != null;
        bool hasShop = other.GetComponent<ShoppingGame_Transition>() != null;
        bool hasRace = other.GetComponent<Racing_Transition>() != null;
        bool hasCar = other.GetComponent<Trigger_Car>() != null;
        bool hasDust = other.GetComponent<DustMinigame>() != null || other.GetComponentInParent<DustMinigame>() != null;
        bool hasNotice = other.GetComponent<NoticeBox>() != null;
        bool hasChungCake = other.GetComponent<TriggerBox_ChungCake>() != null || other.GetComponentInParent<TriggerBox_ChungCake>() != null;
        bool hasDonate = other.GetComponent<Trigger_Donate>() != null || other.GetComponentInParent<Trigger_Donate>() != null;
        bool hasCountDown = other.GetComponent<Trigger_CountDown>() != null || other.GetComponentInParent<Trigger_CountDown>() != null;

        if (hasNote || hasDoor || hasEnv || hasDlg || hasStall || hasOAnQuan || hasShop || hasRace || hasCar || hasDust || hasNotice || hasChungCake || hasDonate || hasCountDown)
        {
            currentInteractable = other;
            Debug.Log($"Đã vào vùng tương tác của: {other.gameObject.name}");
        }

        // Thông báo cho MarketStallInteractable biết Player đã vào
        // (không dùng trigger của stall vì body Player có thể không chạm tới)
        MarketStallInteractable stall = other.GetComponent<MarketStallInteractable>();
        if (stall != null)
        {
            Debug.Log($"[PlayerInteract] Đã tìm thấy stall trên {other.gameObject.name}, gọi OnPlayerEnter...");
            stall.OnPlayerEnter();
        }
        else
        {
            if (hasStall) Debug.LogError($"[PlayerInteract] hasStall là TRUE nhưng stall lại NULL?!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Khi người chơi đi ra khỏi vùng, xóa object đã lưu
        if (currentInteractable == other)
        {
            Debug.Log("Đã ra khỏi vùng tương tác của: " + other.gameObject.name);
            currentInteractable = null;
        }

        // Thông báo cho MarketStallInteractable biết Player đã ra
        MarketStallInteractable stall = other.GetComponent<MarketStallInteractable>();
        if (stall != null) stall.OnPlayerExit();
    }

    private void InteractWithObject(Collider col)
    {
        // Các script trên vật thể sẽ tự động gọi TaskManager khi tương tác, 
        // không cần hardcode kiểm tra tên Object ở đây nữa.
        
        // Gọi hàm tương ứng trên các script
        PaperNote note = col.GetComponent<PaperNote>();
        DoorInteract door = col.GetComponent<DoorInteract>();
        RedEnvelop envelop = col.GetComponent<RedEnvelop>();
        DialogueTrigger dialogue = col.GetComponent<DialogueTrigger>();
        MarketStallInteractable marketStall = col.GetComponent<MarketStallInteractable>();
        OAnQuanTransition1 oAnQuan = col.GetComponent<OAnQuanTransition1>();
        ShoppingGame_Transition shopping = col.GetComponent<ShoppingGame_Transition>();
        Racing_Transition racing = col.GetComponent<Racing_Transition>();
        Trigger_Car car = col.GetComponent<Trigger_Car>();

        if (note != null)
        {
            note.ReadNote();
        }
        
        if (door != null)
        {
            door.Interact();
        }
        
        if (envelop != null)
        {
            envelop.OpenEnvelop();
        }
        
        if (dialogue != null)
        {
            dialogue.TriggerDialogue();
        }

        if (marketStall != null)
        {
            marketStall.Interact();
        }

        if (oAnQuan != null)
        {
            oAnQuan.Interact(this.transform);
        }

        if (shopping != null)
        {
            shopping.Interact(this.transform);
        }

        if (racing != null)
        {
            racing.Interact(this.transform);
        }

        if (car != null)
        {
            car.Interact();
        }

        DustMinigame dustGame = col.GetComponent<DustMinigame>();
        if (dustGame == null) dustGame = col.GetComponentInParent<DustMinigame>();
        if (dustGame != null)
        {
            dustGame.Interact();
            
            // Xóa bộ nhớ để chữ E biến mất ngay lập tức và không đi theo nhân vật
            currentInteractable = null;
            
            // Vô hiệu hóa vùng Trigger này để người chơi không thể tương tác lại lần 2
            col.enabled = false;
        }

        TriggerBox_ChungCake chungCake = col.GetComponent<TriggerBox_ChungCake>();
        if (chungCake == null) chungCake = col.GetComponentInParent<TriggerBox_ChungCake>();
        if (chungCake != null)
        {
            chungCake.Interact();
        }

        Trigger_Donate donate = col.GetComponent<Trigger_Donate>();
        if (donate == null) donate = col.GetComponentInParent<Trigger_Donate>();
        if (donate != null)
        {
            donate.Interact();
            return; // Trigger_Donate tự xử lý floating text
        }

        Trigger_CountDown countDown = col.GetComponent<Trigger_CountDown>();
        if (countDown == null) countDown = col.GetComponentInParent<Trigger_CountDown>();
        if (countDown != null)
        {
            currentInteractable = null; // Ẩn E prompt ngay lập tức
            countDown.Interact();
            return;
        }

        // Kích hoạt hiệu ứng chữ bay lên của NoticeBox nếu có
        NoticeBox noticeBox = col.GetComponent<NoticeBox>();
        if (noticeBox != null)
        {
            noticeBox.ShowFloatingText();
        }
    }

    // ─────────────────────────────────────────────
    //  UI Giao diện (thay thế cho text 3D)
    // ─────────────────────────────────────────────
    private void OnGUI()
    {
        // Chỉ hiện hộp text nếu đang đứng gần object tương tác
        if (currentInteractable == null) return;

        // Mặc định
        string promptText = "[E] Tương tác";

        // Kiểm tra xem object có script NoticeBox để lấy chữ custom không
        NoticeBox noticeBox = currentInteractable.GetComponent<NoticeBox>();
        if (noticeBox != null && !string.IsNullOrEmpty(noticeBox.promptText))
        {
            promptText = noticeBox.promptText;
        }
        else
        {
            // Tương thích ngược: lấy chữ từ quầy hàng nếu có
            MarketStallInteractable stall = currentInteractable.GetComponent<MarketStallInteractable>();
            if (stall != null && !string.IsNullOrEmpty(stall.buyPromptText))
            {
                promptText = "[E] " + stall.buyPromptText;
            }
        }

        // Vẽ cái hộp đen mờ (GUI Box) như DustMinigame
        const int boxWidth = 280;
        const int boxHeight = 42;

        // Đặt ở tọa độ 65% chiều ngang (lệch phải) và 55% chiều dọc (hơi thấp hơn tâm một chút)
        // Điều này giúp text hiện ngay bên cạnh người chơi thay vì ở dưới đáy màn hình
        float centerX = Screen.width * 0.65f;
        float centerY = Screen.height * 0.55f;
        Rect rect = new Rect(centerX - boxWidth * 0.5f, centerY - boxHeight * 0.5f, boxWidth, boxHeight);

        GUI.depth = -1000;
        GUI.Box(rect, GUIContent.none);

        // Định dạng style cho chữ
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        // Vẽ chữ lên trên hộp
        GUI.Label(rect, promptText, style);
    }
}
