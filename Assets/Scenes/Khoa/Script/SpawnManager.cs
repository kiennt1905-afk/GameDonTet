using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.AI;

[System.Serializable]
public struct NPCRarity
{
    public GameObject prefab;
    [Range(0f, 100f)] public float spawnChance;
}

public class SpawnManager : MonoBehaviour
{
    [Header("Cấu hình danh sách NPC và Tỉ lệ %")]
    public List<NPCRarity> npcList;       
    public Transform[] spawnPoints;       
    
    [Header("Cài đặt Lộ Trình")]
    public Transform[] route1;            
    public Transform[] route2;            
    public Transform[] route3;            

    [Header("Cài đặt Số lượng & Thời gian")]
    [Tooltip("Số lượng NPC tối đa luôn cố định trên map.")]
    public int totalNPCsCount = 10; 

    [Tooltip("Khoảng thời gian chờ (giây) giữa 2 lần spawn NPC tiếp theo.")]
    public float spawnDelay = 2f; 

    private List<GameObject> currentNPCs = new List<GameObject>();
    private bool isSpawning = false; // Biến cờ để kiểm tra trạng thái đang sinh NPC

    void Start()
    {
        // Khởi động vòng lặp kiểm tra liên tục bằng Coroutine
        StartCoroutine(MaintainNPCPopulationRoutine());
    }

    // Coroutine tự động quản lý số lượng và tạo khoảng trễ giữa các lần sinh
    IEnumerator MaintainNPCPopulationRoutine()
    {
        while (true) // Vòng lặp vô hạn chạy suốt game
        {
            // Dọn sạch danh sách các con NPC đã bị hủy (do đứng im lỗi kẹt)
            currentNPCs.RemoveAll(item => item == null);

            // Nếu số lượng NPC hiện tại trên map nhỏ hơn số lượng cố định
            if (currentNPCs.Count < totalNPCsCount && !isSpawning)
            {
                // Gọi hàm sinh ra 1 con NPC duy nhất
                SpawnSingleNPC();

                // Kích hoạt thời gian chờ giữa 2 lần spawn theo đúng số giây bạn chỉnh
                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                // Nếu đã đủ số lượng, đợi 0.5 giây rồi mới quét kiểm tra lại (để tối ưu CPU)
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void SpawnSingleNPC()
    {
        if (npcList.Count == 0 || spawnPoints.Length == 0) return;

        // 1. Chọn ngẫu nhiên vị trí Spawn ban đầu
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[randomSpawnIndex];

        // 2. Lọc NPC theo tỉ lệ %
        GameObject selectedPrefab = GetWeightedRandomNPC();
        if (selectedPrefab == null) return;

        // 3. Sửa lỗi đứng im bằng cách tìm điểm chuẩn trên NavMesh
        Vector3 exactNavMeshPos = selectedSpawnPoint.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(selectedSpawnPoint.position, out hit, 3.0f, NavMesh.AllAreas))
        {
            exactNavMeshPos = hit.position; 
        }

        // 4. Sinh ra NPC
        GameObject newNPC = Instantiate(selectedPrefab, exactNavMeshPos, selectedSpawnPoint.rotation);
        currentNPCs.Add(newNPC); 

        // 5. Bọc lót lỗi vị trí
        NavMeshAgent agent = newNPC.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;                       
            newNPC.transform.position = exactNavMeshPos; 
            agent.enabled = true;                        
        }

        // 6. Giao lộ trình ngẫu nhiên
        Transform[] selectedRoute = GetRandomRoute();
        NPCRouteMover mover = newNPC.GetComponent<NPCRouteMover>();
        if (mover != null && selectedRoute != null)
        {
            mover.SetupRoute(selectedRoute);
        }
    }

    GameObject GetWeightedRandomNPC()
    {
        float totalWeight = 0;
        foreach (var npc in npcList) totalWeight += npc.spawnChance;
        if (totalWeight == 0) return null;

        float randomRoll = Random.Range(0f, totalWeight);
        float processedWeight = 0;

        foreach (var npc in npcList)
        {
            processedWeight += npc.spawnChance;
            if (randomRoll <= processedWeight) return npc.prefab;
        }
        return npcList[0].prefab;
    }

    public Transform[] GetRandomRoute()
    {
        int randomRouteIndex = Random.Range(1, 4); 
        if (randomRouteIndex == 1) return route1;
        if (randomRouteIndex == 2) return route2;
        return route3;
    }
}