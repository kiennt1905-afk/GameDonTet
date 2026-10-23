using UnityEngine;
using UnityEngine.UI;

public class SpeedometerUI : MonoBehaviour
{
    public Image speedometerImage;
    public Sprite[] speedSprites; // Mảng chứa 6 ảnh: 40 -> 42 -> 44 -> 53 -> 58 -> 60

    private float currentIndex = 5f; // Mặc định ở tốc độ 60km/h (index = 5)
    private float animationSpeed = 5f; // Tốc độ trượt chuyển cảnh giữa các khung hình

    private void Update()
    {
        if (RacingGameManager.Instance != null && (!RacingGameManager.Instance.isGameStarted || RacingGameManager.Instance.isGameOver))
            return;

        if (speedSprites == null || speedSprites.Length == 0 || speedometerImage == null)
            return;

        // Xác định index mục tiêu dựa vào thao tác phanh
        float targetIndex = 5f; // Không bấm S -> 60km/h
        if (Input.GetKey(KeyCode.S))
        {
            targetIndex = 0f; // Bấm S -> 40km/h
        }

        // Dùng hàm Lerp để tạo hiệu ứng animation mượt mà (như kim đồng hồ trượt dần)
        currentIndex = Mathf.Lerp(currentIndex, targetIndex, Time.deltaTime * animationSpeed);

        // Chuyển đổi sang số nguyên gần nhất để chọn frame ảnh
        int spriteIndex = Mathf.Clamp(Mathf.RoundToInt(currentIndex), 0, speedSprites.Length - 1);
        
        speedometerImage.sprite = speedSprites[spriteIndex];
    }
}
