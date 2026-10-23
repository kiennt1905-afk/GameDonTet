using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float typeSpeed = 0.05f;

    private AudioSource audioSource;
    private Queue<DialogueLine> linesQueue;
    private bool isTyping = false;
    private string currentFullText = "";
    private System.Action onDialogueEndCallback = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Nếu có cảnh mới load, ta lấy các UI từ bản sao mới này truyền cho bản gốc
            Instance.dialoguePanel = this.dialoguePanel;
            Instance.nameText = this.nameText;
            Instance.dialogueText = this.dialogueText;
            
            // Đảm bảo ẩn UI của scene mới khi vừa load
            if (Instance.dialoguePanel != null)
            {
                Instance.dialoguePanel.SetActive(false);
            }
            
            // Xoá bản sao thừa
            Destroy(gameObject);
            return;
        }

        linesQueue = new Queue<DialogueLine>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        bool nextInput = false;
        if (Keyboard.current != null)
        {
            nextInput = Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        if (Mouse.current != null)
        {
            nextInput = nextInput || Mouse.current.leftButton.wasPressedThisFrame;
        }

        // Khi Hộp thoại đang mở và người chơi ấn E, Space, hoặc chuột trái
        if (dialoguePanel != null && dialoguePanel.activeSelf && nextInput)
        {
            if (isTyping)
            {
                // Nếu đang gõ chữ từ từ -> Bấm phát hiện ra luôn
                StopAllCoroutines();
                dialogueText.text = currentFullText;
                isTyping = false;
            }
            else
            {
                // Nếu đã hiện hết chữ -> Chuyển sang câu tiếp theo
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        StartDialogue(dialogueLines, null);
    }

    /// <summary>
    /// Bắt đầu hội thoại. onEnd sẽ được gọi khi hết tất cả dòng thoại.
    /// </summary>
    public void StartDialogue(DialogueLine[] dialogueLines, System.Action onEnd)
    {
        if (dialoguePanel == null)
        {
            Debug.Log("[LỖI] Không tìm thấy UI DialoguePanel ở scene này! Cần có DialogueManager trong scene.");
            onEnd?.Invoke(); // gọi callback dù không có UI
            return;
        }

        // Nếu đang hiện dialogue khác → chờ (không đè lững callback cũ)
        if (dialoguePanel.activeSelf)
        {
            Debug.Log("[LỖI] Đang có dialogue khác chạy! Bỏ qua yêu cầu mới.");
            return;
        }

        Debug.Log("[DialogueManager] Đã nhận lệnh StartDialogue. Đang bật dialoguePanel lên.");
        onDialogueEndCallback = onEnd;
        dialoguePanel.SetActive(true);
        linesQueue.Clear();

        foreach (var line in dialogueLines)
        {
            linesQueue.Enqueue(line);
        }

        Debug.Log($"[DialogueManager] Bắt đầu hiển thị {dialogueLines.Length} câu thoại.");
        
        // Khóa chuyển động người chơi
        MonoBehaviour playerController = FindFirstObjectByType<global::PlayerController>();
        if (playerController != null) playerController.enabled = false;

        DiagnoseUI();
        DisplayNextSentence();
    }

    private void DiagnoseUI()
    {
        Debug.Log("--- BẮT ĐẦU CHẨN ĐOÁN UI ---");
        
        if (dialoguePanel != null)
        {
            Debug.Log($"- Panel activeSelf: {dialoguePanel.activeSelf}, activeInHierarchy: {dialoguePanel.activeInHierarchy}");
            
            Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.Log("- LỖI: Không tìm thấy Canvas nào bọc bên ngoài Dialogue Panel!");
            }
            else
            {
                Debug.Log($"- Canvas tồn tại. RenderMode: {canvas.renderMode}, enabled: {canvas.enabled}");
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    Debug.Log("- CẢNH BÁO: Canvas đang ở dạng World Space (Hiển thị 3D trong thế giới). Rất có thể nó đang bị nằm kẹt trong tường hoặc quay lưng lại với Camera!");
                }
            }

            CanvasGroup group = dialoguePanel.GetComponentInParent<CanvasGroup>();
            if (group != null)
            {
                Debug.Log($"- CanvasGroup alpha: {group.alpha}, interactable: {group.interactable}");
                if (group.alpha <= 0.01f) Debug.Log("- LỖI: CanvasGroup đang có Alpha = 0 (Hoàn toàn tàng hình)!");
            }
        }

        if (dialogueText != null)
        {
            Debug.Log($"- Màu chữ DialogueText: {dialogueText.color} (Alpha: {dialogueText.color.a})");
            if (dialogueText.color.a <= 0.01f) Debug.Log("- LỖI: Màu chữ đang bị trong suốt!");
        }

        Debug.Log("--- KẾT THÚC CHẨN ĐOÁN UI ---");
    }

    public void DisplayNextSentence()
    {
        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();
        
        if (nameText != null) 
        {
            nameText.text = line.speakerName;
        }
        else 
        {
            Debug.Log("[LỖI] Bạn đã quên kéo khung Text Tên vào ô Name Text trong DialogueManager!");
        }

        if (dialogueText == null)
        {
            Debug.Log("[LỖI] Bạn đã quên kéo khung Text Nội Dung vào ô Dialogue Text trong DialogueManager!");
        }

        currentFullText = line.dialogueText;

        // Xử lý âm thanh
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (line.audioClip != null)
        {
            audioSource.clip = line.audioClip;
            audioSource.Play();
        }

        // Bắt đầu hiệu ứng Typewriter
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.dialogueText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        if (dialogueText != null) 
        {
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typeSpeed);
            }
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Mở khóa chuyển động người chơi
        MonoBehaviour playerController = FindFirstObjectByType<global::PlayerController>();
        if (playerController != null) playerController.enabled = true;

        // Gọi callback nếu có (dùng bởi ShoppingDialogueTrigger và các trigger khác)
        System.Action callback = onDialogueEndCallback;
        onDialogueEndCallback = null;
        callback?.Invoke();
    }
}
