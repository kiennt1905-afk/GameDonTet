using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShoppingGame
{
    [System.Serializable]
    public class ShoppingRequirement
    {
        public string itemName;
        public int requiredCount;
        [HideInInspector] public int currentCount;
        public Text uiText;
    }

    public class ShoppingGameManager : MonoBehaviour
    {
        public static ShoppingGameManager Instance { get; private set; }

        public List<ShoppingRequirement> requirements;
        
        [Header("Spawning")]
        public GameObject[] itemPrefabs;
        public float spawnInterval = 1f;
        public float spawnAreaX = 8f;
        public float spawnY = 10f;

        [Header("UI Panels")]
        public GameObject startPanel;
        public GameObject gameOverPanel;
        public GameObject victoryPanel;
        public Text loseReasonText;

        public string mainSceneName = "Cho"; // The scene to load when exiting

        public bool IsGameOver { get; private set; }
        public bool IsGameStarted { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Hiển thị và mở khóa chuột
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Fix lỗi click UI cho project dùng New Input System
            UnityEngine.EventSystems.EventSystem es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (es != null)
            {
                var standalone = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (standalone != null)
                {
                    Destroy(standalone);
                    es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
            }

            IsGameOver = false;
            IsGameStarted = false;
            
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (startPanel != null) startPanel.SetActive(true);
            
            UpdateAllUI();
        }

        private void Update()
        {
            // Liên tục ép mở khóa chuột phòng trường hợp FirstPersonController khởi chạy sau và khóa lại
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void StartGame()
        {
            IsGameStarted = true;
            IsGameOver = false;
            
            if (startPanel != null) startPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            
            // Reset counts
            foreach (var req in requirements)
            {
                req.currentCount = 0;
            }
            UpdateAllUI();
            
            // Clear existing items in scene if any
            GameObject[] items = GameObject.FindGameObjectsWithTag("ShoppingItem");
            foreach (var item in items) Destroy(item);

            StartCoroutine(SpawnItemsRoutine());
        }

        public void ExitGame()
        {
            // Thiết lập vị trí trả về cho Player
            PlayerPrefs.SetFloat("PlayerPosX", -32f);
            PlayerPrefs.SetFloat("PlayerPosY", 0.7f);
            PlayerPrefs.SetFloat("PlayerPosZ", 209f);
            PlayerPrefs.SetInt("HasSavedPos", 1);
            PlayerPrefs.Save();

            UnityEngine.SceneManagement.SceneManager.LoadScene(mainSceneName);
        }

        private IEnumerator SpawnItemsRoutine()
        {
            while (!IsGameOver && IsGameStarted)
            {
                yield return new WaitForSeconds(spawnInterval);
                if (!IsGameOver && IsGameStarted)
                {
                    SpawnRandomItem();
                }
            }
        }

        private void SpawnRandomItem()
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0) return;

            int randomIndex = Random.Range(0, itemPrefabs.Length);
            GameObject prefab = itemPrefabs[randomIndex];
            
            float randomX = Random.Range(-spawnAreaX, spawnAreaX);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        public void OnItemCaught(string itemName)
        {
            if (IsGameOver || !IsGameStarted) return;

            bool isNeeded = false;
            bool exceeded = false;

            foreach (var req in requirements)
            {
                if (req.itemName == itemName)
                {
                    isNeeded = true;
                    req.currentCount++;
                    UpdateUI(req);

                    if (req.currentCount > req.requiredCount)
                    {
                        exceeded = true;
                    }
                    break;
                }
            }

            if (!isNeeded)
            {
                GameOver("Bạn nhặt nhầm đồ rùi!");
            }
            else if (exceeded)
            {
                GameOver("Nhặt thừa quá số lượng rồi!");
            }
            else
            {
                CheckWinCondition();
            }
        }

        private void UpdateUI(ShoppingRequirement req)
        {
            if (req.uiText != null)
            {
                // UI will display the translated name, but we match internally using itemName
                string vietnameseName = GetVietnameseName(req.itemName);
                req.uiText.text = $"{vietnameseName}: {req.currentCount}/{req.requiredCount}";
            }
        }

        private string GetVietnameseName(string name)
        {
            switch (name)
            {
                case "Broccoli": return "Súp lơ";
                case "Cabbage": return "Bắp cải";
                case "Carpfish": return "Cá chép";
                case "Chicken": return "Thịt gà";
                default: return name;
            }
        }

        private void UpdateAllUI()
        {
            foreach (var req in requirements)
            {
                UpdateUI(req);
            }
        }

        private void CheckWinCondition()
        {
            foreach (var req in requirements)
            {
                if (req.currentCount < req.requiredCount)
                    return; // Still need more
            }

            Victory();
        }

        private void GameOver(string reason)
        {
            IsGameOver = true;
            if (loseReasonText != null) loseReasonText.text = reason;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            
            StartCoroutine(WaitAndShowStartMenu());
        }
        
        private IEnumerator WaitAndShowStartMenu()
        {
            yield return new WaitForSeconds(2f);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (startPanel != null) startPanel.SetActive(true);
            IsGameStarted = false; // Allow restarting
        }

        [Header("Task Settings")]
        [Tooltip("Nhập ID của Task muốn hoàn thành khi thắng (Vd: Shopping)")]
        public string taskIdToComplete = "Shopping";

        private void Victory()
        {
            IsGameOver = true;
            if (victoryPanel != null) victoryPanel.SetActive(true);
            
            // Cập nhật nhiệm vụ
            if (!string.IsNullOrEmpty(taskIdToComplete) && TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(taskIdToComplete);
            }
            
            StartCoroutine(WaitAndExit());
        }

        private IEnumerator WaitAndExit()
        {
            yield return new WaitForSeconds(2f);
            ExitGame();
        }

        // Keep this for backwards compatibility just in case
        public void ReplayGame()
        {
            StartGame();
        }
    }
}
