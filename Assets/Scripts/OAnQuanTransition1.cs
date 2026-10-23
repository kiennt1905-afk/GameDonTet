using UnityEngine;
using UnityEngine.SceneManagement;

public class OAnQuanTransition1 : MonoBehaviour
{
    public string sceneToLoad = "MiniGame_1"; // Thay bằng tên scene thực tế của Ô Ăn Quan

    [Tooltip("Nếu có, sẽ lưu task ID này vào PlayerPrefs để hoàn thành khi quay về scene cũ.\n"
             + "VD: \"buy_thit_2\". Để trống nếu không cần hoàn thành task nào.")]
    public string pendingTaskId = "";

    public void Interact(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            // Đẩy người chơi lùi lại một khoảng so với hướng họ đang đi vào Trigger
            // và nhích lên trên một chút để không bị kẹt mặt đất
            Vector3 returnPos = playerTransform.position - (playerTransform.forward * 3.5f);
            returnPos.y += 0.5f;
            
            PlayerPrefs.SetFloat("PlayerPosX", returnPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", returnPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", returnPos.z);
            PlayerPrefs.SetInt("HasSavedPos", 1);

            // Lưu task cần complete khi trở về (Q1: B — complete bất kể thắng/thua)
            if (!string.IsNullOrEmpty(pendingTaskId))
            {
                PlayerPrefs.SetString("PendingTaskComplete", pendingTaskId);
                Debug.Log($"[OAnQuanTransition1] Đã lưu PendingTask: '{pendingTaskId}'");
            }

            PlayerPrefs.Save();

            Debug.Log("Đã lưu vị trí 3D và chuyển sang Minigame Ô Ăn Quan!");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
