using UnityEngine;

/// <summary>
/// Gắn vào TriggerBox dâng cúng tại đền/chùa trong SampleScene.
/// Mỗi lần bấm E: -1 bánh chưng, hiện floating text "+1 Công Đức".
/// Khi hết bánh: hiện floating text "Hết bánh chưng", +0 công đức.
/// </summary>
[RequireComponent(typeof(NoticeBox))]
public class Trigger_Donate : MonoBehaviour
{
    [Header("Công Đức")]
    [Tooltip("Số công đức cộng mỗi lần dâng 1 bánh")]
    public int congDucMoiLan = 1;

    [Header("Floating Text")]
    public string textDangBanh = "+1 Công Đức";
    public string textHetBanh  = "Hết bánh chưng";

    // Tổng công đức tích lũy (có thể đọc từ ngoài)
    public int TongCongDuc { get; private set; } = 0;

    private NoticeBox noticeBox;

    private void Awake()
    {
        noticeBox = GetComponent<NoticeBox>();
    }

    /// <summary>
    /// Được gọi từ PlayerInteract khi người chơi bấm E
    /// </summary>
    public void Interact()
    {
        int soBanh = BanhChungScore.Instance != null ? BanhChungScore.Instance.SoBanh : 0;

        if (soBanh > 0)
        {
            // Trừ 1 bánh
            BanhChungScore.Instance.LuuSoBanh(soBanh - 1);

            // Cộng công đức
            TongCongDuc += congDucMoiLan;

            // Hiện floating text dâng bánh
            noticeBox.ShowFloatingText(textDangBanh);

            Debug.Log($"[Donate] Dâng 1 bánh. Còn lại: {soBanh - 1} bánh. Công đức: {TongCongDuc}");
        }
        else
        {
            // Hết bánh
            noticeBox.ShowFloatingText(textHetBanh);
            Debug.Log("[Donate] Không còn bánh chưng để dâng.");
        }
    }
}
