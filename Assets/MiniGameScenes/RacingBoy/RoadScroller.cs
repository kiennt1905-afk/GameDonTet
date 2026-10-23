using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    public float scrollSpeed = 26.67f;
    private float baseScrollSpeed = 26.67f;
    public float roadLength = 100f; // Độ dài của một model đường

    private void Update()
    {
        if (RacingGameManager.Instance != null && (!RacingGameManager.Instance.isGameStarted || RacingGameManager.Instance.isGameOver))
            return;

        // Tăng dần tốc độ theo thời gian để game khó hơn
        baseScrollSpeed += Time.deltaTime * 0.2f;

        // Cơ chế phanh: Khi bấm S, giảm tốc độ về bằng với tốc độ xe tải (16f)
        if (Input.GetKey(KeyCode.S))
        {
            scrollSpeed = 16f;
        }
        else
        {
            scrollSpeed = baseScrollSpeed;
        }

        // Bắt buộc dùng Space.World để đường luôn trượt theo trục Z của thế giới
        // Tránh tình trạng model .glb có rotation mặc định làm đường chạy xuống dưới đất
        transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);

        // Nếu đường chạy lùi quá khuất camera thì đẩy nó lên phía trước
        if (transform.position.z < -roadLength)
        {
            Vector3 newPos = transform.position;
            newPos.z += roadLength * 2; // Giả sử có 2 mảng đường nối tiếp nhau
            transform.position = newPos;
        }
    }
}
