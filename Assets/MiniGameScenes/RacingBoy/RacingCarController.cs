using UnityEngine;
using System.Collections;

public class RacingCarController : MonoBehaviour
{
    public float moveSpeed = 13.33f;
    public float boundaryX = 4.5f; // Giới hạn lề đường

    public AudioSource crashSource;
    public AudioSource driftSource;

    private bool isCrashed = false;

    private void Update()
    {
        if (RacingGameManager.Instance != null && (!RacingGameManager.Instance.isGameStarted || RacingGameManager.Instance.isGameOver))
        {
            if (driftSource != null && driftSource.isPlaying) driftSource.Stop();
            return;
        }

        // Xử lý Input
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        if (horizontal != 0)
        {
            if (driftSource != null)
            {
                if (!driftSource.isPlaying)
                {
                    driftSource.time = 0.7f; // Bắt đầu từ 0.3s
                    driftSource.Play();
                }
                else if (driftSource.time >= 1.1f)
                {
                    driftSource.time = 0.7f; // Tới 1.2s thì quay lại 0.3s (tạo loop)
                }
            }
        }
        else
        {
            if (driftSource != null && driftSource.isPlaying)
            {
                driftSource.Stop();
            }
        }

        // Di chuyển xe
        Vector3 newPosition = transform.position + Vector3.right * horizontal * moveSpeed * Time.deltaTime;
        
        // Giới hạn xe trong đường đua
        newPosition.x = Mathf.Clamp(newPosition.x, -boundaryX, boundaryX);
        transform.position = newPosition;

        // Lưu ý: Đã bỏ phần xoay xe (transform.rotation = ...) vì file .glb thường có
        // rotation gốc (vd X = -90). Nếu gán đè Quaternion.Euler(0, 0, tilt) sẽ làm xe bị dựng ngược lên trời.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCrashed) return;

        // Kiểm tra xem có đụng vào xe tải không hoặc lề đường không
        if (other.GetComponent<ObstacleMover>() != null || other.name.Contains("Wall"))
        {
            isCrashed = true;
            if (driftSource != null && driftSource.isPlaying) driftSource.Stop();

            if (crashSource != null)
            {
                crashSource.Play();
                StartCoroutine(StopCrashSoundAfter(1f));
            }

            if (RacingGameManager.Instance != null)
            {
                string reason = other.name.Contains("Wall") ? "GẶP TAI NẠN\n(Tông vào lề đường)" : "GẶP TAI NẠN";
                RacingGameManager.Instance.GameOver(reason);
            }
        }
    }

    private IEnumerator StopCrashSoundAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (crashSource != null)
        {
            crashSource.Stop();
        }
    }
}
