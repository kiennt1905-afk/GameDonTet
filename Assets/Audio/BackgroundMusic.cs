using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Kiểm tra xem đã có instance nào tồn tại chưa
        if (instance == null)
        {
            // Nếu chưa, gán instance hiện tại và giữ nó không bị hủy khi chuyển scene
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu đã có (tức là ta quay lại MainScene và nó tạo ra 1 object mới), hủy object mới này đi
            Destroy(gameObject);
        }
    }
}
