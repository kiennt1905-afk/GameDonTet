using UnityEngine;

/// <summary>
/// Lưu số bánh chưng đã gói, tồn tại xuyên scene.
/// Các script khác có thể đọc BanhChungScore.Instance.SoBanh bất cứ lúc nào.
/// </summary>
public class BanhChungScore : MonoBehaviour
{
    public static BanhChungScore Instance { get; private set; }

    public int SoBanh { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LuuSoBanh(int so)
    {
        SoBanh = so;
        Debug.Log($"[BanhChungScore] Đã lưu: {SoBanh} bánh chưng.");
    }
}
