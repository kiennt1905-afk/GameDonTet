using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad = "MiniGame_1";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Đẩy người chơi lùi lại 3 đơn vị so với hướng họ đang đi vào Trigger
            // và nhích lên trên một chút để không bị kẹt mặt đất
            Vector3 returnPos = other.transform.position - (other.transform.forward * 3.5f);
            returnPos.y += 0.5f;
            
            PlayerPrefs.SetFloat("PlayerPosX", returnPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", returnPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", returnPos.z);
            PlayerPrefs.SetInt("HasSavedPos", 1);
            PlayerPrefs.Save();

            Debug.Log("Đã lưu vị trí 3D và chuyển sang Minigame!");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}