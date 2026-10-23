using UnityEngine;
using UnityEngine.AI;

public class NPCRouteMover : MonoBehaviour
{
    private Transform[] waypoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;
    private Animator animator; 
    private SpawnManager spawnManager; // Tham chiếu tới Manager để xin đường mới

    [Header("Cơ Chế Dự Phòng (Anti-Stuck)")]
    [Tooltip("Thời gian tối đa cho phép NPC đứng im (giây). Quá giờ tự biến mất.")]
    public float maxStuckTime = 4f; 

    private float stuckTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); 
        // Tự động tìm kiếm SpawnManager có trên Map
        spawnManager = Object.FindFirstObjectByType<SpawnManager>();

        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        // 1. Đồng bộ Animation với trạng thái di chuyển
        if (animator != null && agent != null)
        {
            bool walking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isRunning", walking);
        }

        // 2. Kiểm tra nếu đứng im quá lâu -> Biến mất (Giải phóng chỗ trống cho con mới)
        if (agent != null)
        {
            if (agent.velocity.magnitude < 0.1f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= maxStuckTime)
                {
                    Debug.Log($"[Anti-Stuck] {gameObject.name} bị hủy vì lỗi đứng im quá {maxStuckTime} giây.");
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                stuckTimer = 0f; // Reset nếu vẫn đang đi mượt
            }
        }

        if (waypoints == null || waypoints.Length == 0) return;

        // 3. Logic tuần tra vòng vòng (Không Despawn)
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPointIndex++;

            if (currentPointIndex < waypoints.Length)
            {
                // Nếu chưa đi hết lộ trình hiện tại -> Điểm tiếp theo
                agent.SetDestination(waypoints[currentPointIndex].position);
            }
            else
            {
                // NẾU ĐÃ ĐI ĐẾN ĐIỂM CUỐI CÙNG: Không Destroy nữa!
                // Xin SpawnManager một lộ trình ngẫu nhiên mới để đi tiếp vòng lặp
                if (spawnManager != null)
                {
                    Transform[] nextRoute = spawnManager.GetRandomRoute();
                    SetupRoute(nextRoute); 
                }
            }
        }
    }

    public void SetupRoute(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentPointIndex = 0;
        
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (waypoints != null && waypoints.Length > 0 && agent != null)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
}