using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Content")]
    public DialogueLine[] dialogueLines;

    public void TriggerDialogue()
    {
        Debug.Log("[DialogueTrigger] Bắt đầu gọi TriggerDialogue...");
        if (DialogueManager.Instance != null)
        {
            Debug.Log("[DialogueTrigger] Đã tìm thấy DialogueManager. Tới bước StartDialogue.");
            DialogueManager.Instance.StartDialogue(dialogueLines);
        }
        else
        {
            Debug.Log("[LỖI] KHÔNG TÌM THẤY DialogueManager trong Scene! Hãy kéo prefab DialogueManager vào Scene này!");
        }
    }
}
