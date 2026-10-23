using UnityEngine;

public class MarkerEffect : MonoBehaviour
{
    [Header("Target")]
    public Transform magicRing;

    [Header("Float")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 2f;

    [Header("Rotate")]
    public float rotateSpeed = 40f;

    [Header("Appearance")]
    [Range(0f, 1f)]
    public float opacity = 1f;
    private float currentOpacity = -1f;

    private Vector3 startLocalPos;
    private Renderer[] renderers;

    void Start()
    {
        // Lưu localPosition thay vì position để prefab hoạt động đúng khi bị đặt làm child của object khác
        startLocalPos = transform.localPosition;

        // Lấy tất cả component render (mesh, sprite) của object này và các object con
        renderers = GetComponentsInChildren<Renderer>();
        UpdateOpacity();
    }

    void Update()
    {
        // Chỉ object cha nổi lên xuống - Dùng localPosition
        transform.localPosition = startLocalPos +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // Chỉ Quad con quay
        if (magicRing != null)
        {
            magicRing.Rotate(Vector3.forward, -rotateSpeed * Time.deltaTime, Space.Self);
        }

        // Tự động cập nhật Opacity nếu bạn kéo thanh trượt trong Inspector
        if (Mathf.Abs(currentOpacity - opacity) > 0.001f)
        {
            UpdateOpacity();
        }
    }

    // Hàm cập nhật độ mờ (Alpha) cho object
    public void UpdateOpacity()
    {
        currentOpacity = opacity;
        if (renderers == null) return;

        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            if (rend is SpriteRenderer spriteRend)
            {
                Color c = spriteRend.color;
                c.a = opacity;
                spriteRend.color = c;
            }
            else if (rend.material != null)
            {
                // Với Standard Shader
                if (rend.material.HasProperty("_Color"))
                {
                    Color c = rend.material.color;
                    c.a = opacity;
                    rend.material.color = c;
                }
                // Với URP (Universal Render Pipeline)
                else if (rend.material.HasProperty("_BaseColor"))
                {
                    Color c = rend.material.GetColor("_BaseColor");
                    c.a = opacity;
                    rend.material.SetColor("_BaseColor", c);
                }
                // Với các hiệu ứng Particle/Quad thường
                else if (rend.material.HasProperty("_TintColor"))
                {
                    Color c = rend.material.GetColor("_TintColor");
                    c.a = opacity;
                    rend.material.SetColor("_TintColor", c);
                }
            }
        }
    }
}