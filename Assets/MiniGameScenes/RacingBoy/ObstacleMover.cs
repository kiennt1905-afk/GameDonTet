using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private bool isRightLane;
    private RoadScroller roadScroller;
    
    // Tốc độ thực tế của xe tải (để tính toán bù trừ với mặt đường)
    public float truckSpeed = 16f; 

    private void Start()
    {
        roadScroller = FindObjectOfType<RoadScroller>();
        
        // Xác định làn dựa vào tọa độ x (x > 0 là làn phải)
        isRightLane = transform.position.x > 0;
        
        // Làn phải (cùng chiều): Xoay đầu xe lại 180 độ để hướng về phía trước (cùng hướng người chơi)
        if (isRightLane)
        {
            transform.Rotate(0, 180f, 0);
        }
    }

    private void Update()
    {
        if (RacingGameManager.Instance != null && (!RacingGameManager.Instance.isGameStarted || RacingGameManager.Instance.isGameOver))
            return;

        // Lấy tốc độ cuộn đường hiện tại (tương đương tốc độ tiến của người chơi)
        float currentScrollSpeed = 20f;
        if (roadScroller != null)
        {
            currentScrollSpeed = roadScroller.scrollSpeed;
        }

        float moveSpeed;

        if (isRightLane)
        {
            // Làn phải (Cùng chiều): Xe tải chạy về phía trước nhưng chậm hơn người chơi.
            // Vận tốc lùi về camera = Tốc độ đường (người chơi) - Tốc độ xe tải
            moveSpeed = currentScrollSpeed - truckSpeed;
            
            // Đảm bảo xe luôn lùi lại phía người chơi (trường hợp xe chạy nhanh hơn người chơi thì cũng ko bị trôi ngược)
            if (moveSpeed < 5f) moveSpeed = 5f; 
        }
        else
        {
            // Làn trái (Ngược chiều): Xe tải lao ngược về phía người chơi
            // Vận tốc lùi về camera = Tốc độ đường + Tốc độ xe tải
            moveSpeed = currentScrollSpeed + truckSpeed;
        }

        // Space.World giúp xe luôn lùi theo trục Z của không gian bất chấp góc xoay
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // Hủy nếu đi quá xa phía sau camera
        if (transform.position.z < -20f)
        {
            Destroy(gameObject);
        }
    }
}
