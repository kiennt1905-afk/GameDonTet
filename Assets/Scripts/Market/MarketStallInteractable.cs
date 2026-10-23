using UnityEngine;

/// <summary>
/// Gắn lên TriggerBox của mỗi quầy hàng trong chợ.
/// 
/// Flow:
///   1. Player vào TriggerBox → hiện MarketPromptUI "[E] Mua nếp" (nếu task đang active)
///   2. Player bấm E → FloatingText bay lên + bắt đầu Dialog
///   3. Dialog kết thúc → CompleteTask → task tiếp theo mở khóa
///   4. (Đặc biệt cho quầy thịt 2) → sau dialog unlock TriggerBox nhóc con Ô Ăn Quan
///
/// Yêu cầu scene: MarketPromptUI, FloatingTextSpawner, DialogueManager, TaskManager đều
/// có mặt trong scene (hoặc DontDestroyOnLoad từ scene trước).
/// </summary>
public class MarketStallInteractable : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Auto-setup
    // ─────────────────────────────────────────────
    private void Awake()
    {
        // Đảm bảo GameObject này có Collider Is Trigger
        // → tránh lỗi silent khi quên gắn Collider trong Editor
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2.5f, 2f);
            Debug.LogWarning($"[MarketStallInteractable] '{gameObject.name}' thiếu Collider — đã tự tạo BoxCollider. " +
                             "Hãy chỉnh size/center cho đúng trong Inspector.");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[MarketStallInteractable] '{gameObject.name}' Collider chưa Is Trigger — đã tự bật.");
        }
    }


    // ─────────────────────────────────────────────
    //  Task Ordering
    // ─────────────────────────────────────────────
    [Header("Task Ordering")]
    [Tooltip("Task ID phải đang ACTIVE (là task chưa hoàn thành đầu tiên) thì quầy này mới phản hồi.\n" +
             "Thường trùng với taskIdToComplete.\n" +
             "VD: \"buy_nep\"")]
    public string taskIdRequired = "";

    [Tooltip("Task ID sẽ được đánh dấu hoàn thành sau khi dialog kết thúc.\n" +
             "VD: \"buy_nep\"")]
    public string taskIdToComplete = "";

    // ─────────────────────────────────────────────
    //  UI
    // ─────────────────────────────────────────────
    [Header("UI")]
    [Tooltip("Text hiển thị trong prompt khi Player vào vùng.\nVD: \"Mua nếp\", \"Mua thịt\"")]
    public string buyPromptText = "Mua hàng";

    [Tooltip("Text nổi hiện khi Player bấm E và bắt đầu tương tác.\nVD: \"Đã mua nếp!\", \"Đã đặt mua thịt!\"")]
    public string floatingText = "Đã mua!";

    [Tooltip("Màu của floating text. Mặc định: vàng ấm.")]
    public Color floatingTextColor = new Color(1f, 0.85f, 0.1f);

    // ─────────────────────────────────────────────
    //  Dialogue
    // ─────────────────────────────────────────────
    [Header("Dialogue")]
    [Tooltip("Các dòng thoại theo thứ tự. Điền speakerName, dialogueText, audioClip.")]
    public DialogueLine[] dialogueLines;

    // ─────────────────────────────────────────────
    //  Đặc biệt: Chợ thịt lần 2 — chờ chặt thịt → Ô Ăn Quan
    // ─────────────────────────────────────────────
    [Header("Special: Unlock Ô Ăn Quan sau dialog")]
    [Tooltip("Nếu true: sau khi dialog kết thúc sẽ bật oAnQuanTriggerObject.\n" +
             "Dùng cho quầy thịt lần 2 (chú bảo chờ → nhóc con mời chơi).")]
    public bool unlockOAnQuanAfterDialogue = false;

    [Tooltip("GameObject TriggerBox của nhóc con (đang ẩn). Sẽ SetActive(true) sau dialog.")]
    public GameObject oAnQuanTriggerObject;

    // ─────────────────────────────────────────────
    //  Internal
    // ─────────────────────────────────────────────
    private bool _playerInside = false;
    private bool _interacted = false; // Mỗi quầy chỉ tương tác 1 lần

    // ─────────────────────────────────────────────
    //  Trigger
    // ─────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        _playerInside = true;
        RefreshPrompt();
    }

    /// <summary>
    /// OnTriggerStay đảm bảo prompt hiện ngay khi task trước vừa complete
    /// trong khi Player đang đứng trong vùng trigger.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other)) return;
        _playerInside = true;
        RefreshPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        _playerInside = false;
        MarketPromptUI.Hide();
    }

    // ─────────────────────────────────────────────
    //  API gọi từ PlayerInteract (bypass trigger collider size issue)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Gọi bởi PlayerInteract.OnTriggerEnter khi Player bước vào vùng detect của quầy.
    /// Không phụ thuộc vào size của collider trên TriggerBox.
    /// </summary>
    public void OnPlayerEnter()
    {
        _playerInside = true;

        bool active = IsTaskActive();
        Debug.Log($"[MarketStall:{gameObject.name}] OnPlayerEnter | taskRequired='{taskIdRequired}' | IsTaskActive={active} | _interacted={_interacted} | PromptUI={MarketPromptUI.Instance != null}");

        RefreshPrompt();
    }


    /// <summary>
    /// Gọi bởi PlayerInteract.OnTriggerExit khi Player rời khỏi vùng detect của quầy.
    /// </summary>
    public void OnPlayerExit()
    {
        _playerInside = false;
        MarketPromptUI.Hide();
    }


    /// <summary>
    /// Hiện hoặc ẩn prompt tuỳ theo task có đang active không.
    /// </summary>
    private void RefreshPrompt()
    {
        if (_interacted) return;
        if (IsTaskActive())
            MarketPromptUI.Show(buyPromptText);
        else
            MarketPromptUI.Hide();
    }

    /// <summary>
    /// Kiểm tra collider có thuộc Player không — dùng tag hoặc PlayerInteract trên hierarchy.
    /// </summary>
    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.GetComponentInParent<PlayerInteract>() != null;
    }



    // ─────────────────────────────────────────────
    //  Interact — được gọi bởi PlayerInteract khi bấm E
    // ─────────────────────────────────────────────
    /// <summary>
    /// Gọi bởi PlayerInteract.InteractWithObject() khi Player bấm E trong trigger.
    /// </summary>
    public void Interact()
    {
        // Bỏ qua nếu task không active hoặc đã tương tác rồi
        if (!IsTaskActive() || _interacted) return;

        // Đánh dấu đã tương tác (tránh spam)
        _interacted = true;

        // Ẩn prompt UI
        MarketPromptUI.Hide();

        // Hiện floating text tại vị trí quầy
        FloatingTextSpawner.Spawn(floatingText, transform.position, floatingTextColor);

        // Bắt đầu dialog → callback khi xong
        if (DialogueManager.Instance != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            // Không có dialog → hoàn thành task ngay
            OnDialogueEnd();
        }
    }

    // ─────────────────────────────────────────────
    //  Callback sau dialog
    // ─────────────────────────────────────────────
    private void OnDialogueEnd()
    {
        // Đặc biệt: unlock trigger nhóc con Ô Ăn Quan TRƯỚC khi complete task
        // (để nhóc con xuất hiện và player có thể đến chơi)
        if (unlockOAnQuanAfterDialogue && oAnQuanTriggerObject != null)
        {
            oAnQuanTriggerObject.SetActive(true);
            Debug.Log("[MarketStallInteractable] Đã unlock TriggerBox nhóc con Ô Ăn Quan.");
        }

        // Hoàn thành task nếu KHÔNG phải quầy thịt 2 (thịt 2 complete sau khi về từ minigame)
        // Nếu là quầy thịt 2: taskIdToComplete để trống, việc complete giao cho SceneReturnHandler
        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            Debug.Log($"[MarketStallInteractable] Completed task: {taskIdToComplete}");
        }
    }

    // ─────────────────────────────────────────────
    //  Helper
    // ─────────────────────────────────────────────
    private bool IsTaskActive()
    {
        if (TaskManager.Instance == null) return false;
        return TaskManager.Instance.IsTaskActive(taskIdRequired);
    }
}
