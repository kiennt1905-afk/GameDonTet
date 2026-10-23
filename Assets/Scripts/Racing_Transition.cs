using UnityEngine;
using UnityEngine.SceneManagement;

public class Racing_Transition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string sceneToLoad = "Racing";

    [Header("Task Settings")]
    [Tooltip("Nhập ID của Task muốn hoàn thành khi chuyển cảnh (Vd: RacingBoy)")]
    public string taskIdToComplete = "RacingBoy";

    public void Interact(Transform playerTransform)
    {
        // Hoàn thành nhiệm vụ
        if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
        }

        if (playerTransform != null)
        {
            // Lưu lại vị trí của Player để sau khi chơi xong MiniGame có thể quay lại đúng chỗ
            Vector3 returnPos = playerTransform.position - (playerTransform.forward * 3.5f);
            returnPos.y += 0.5f;
            
            PlayerPrefs.SetFloat("PlayerPosX", returnPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", returnPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", returnPos.z);
            PlayerPrefs.SetFloat("PlayerRotY", playerTransform.rotation.eulerAngles.y);
            PlayerPrefs.SetInt("HasSavedPos", 1);
            PlayerPrefs.Save();
        }

        Debug.Log("Chuyển sang scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
