using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;
    public float laneWidth = 3f;
    
    private float timer = 0f;
    
    // Các vị trí spawn cụ thể
    public Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(3f, 0.7f, 3f),
        new Vector3(-3f, 0.7f, 3.1f)
    };

    private void Update()
    {
        if (RacingGameManager.Instance != null && (!RacingGameManager.Instance.isGameStarted || RacingGameManager.Instance.isGameOver))
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
            
            // Tăng độ khó theo thời gian (giảm thời gian spawn)
            if (spawnInterval > 0.8f)
            {
                spawnInterval -= 0.05f;
            }
        }
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;

        // Chọn ngẫu nhiên 1 trong các vị trí spawn (chỉ lấy X và Y)
        Vector3 randomPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
        
        // Đẩy trục Z ra tít xa bằng với vị trí của Spawner (mặc định Z = 80)
        // Nếu dùng Z = 3 thì nó sẽ spawn ngay sát xe của người chơi
        Vector3 spawnPos = new Vector3(randomPos.x, randomPos.y, transform.position.z);
        
        GameObject newObstacle = Instantiate(obstaclePrefab, spawnPos, obstaclePrefab.transform.rotation);
        
        // Vì bản thân prefab gốc trong scene đã bị ẩn (SetActive = false) 
        // nên các bản clone được tạo ra cũng bị ẩn. Ta cần bật nó lên lại!
        newObstacle.SetActive(true);
    }
}
