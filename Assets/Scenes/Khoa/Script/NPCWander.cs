using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 8f;   // Bán kính giới hạn di chuyển
    public float wanderTimer = 4f;    // Thời gian chờ để đổi điểm mới (giây)

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer; // Đặt bằng thời gian hẹn giờ để NPC đi ngay khi bắt đầu
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Nếu đã hết thời gian chờ HOẶC NPC đã đi đến đích cũ trước thời hạn
        if (timer >= wanderTimer || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            Vector3 newPos = GetRandomNavPos(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            timer = 0f; // Reset lại đồng hồ
        }
    }

    // Hàm tìm một vị trí ngẫu nhiên hợp lệ trên lưới NavMesh
    private Vector3 GetRandomNavPos(Vector3 center, float distance)
    {
        // Lấy một điểm ngẫu nhiên trong một hình cầu
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += center;

        NavMeshHit navHit;
        // Kiểm tra xem điểm ngẫu nhiên đó có nằm trên lưới NavMesh không
        if (NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return center; // Nếu lỗi, trả về vị trí hiện tại
    }
}