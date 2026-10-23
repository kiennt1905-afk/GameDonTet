using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerBox_ChungCake : MonoBehaviour
{
    [Tooltip("Tên của Scene muốn chuyển tới (ví dụ: MiniGame)")]
    public string sceneToLoad = "MiniGame";

    public void Interact()
    {
        Debug.Log($"[TriggerBox_ChungCake] Đang tải scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}
