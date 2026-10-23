using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RacingGameManager : MonoBehaviour
{
    public static RacingGameManager Instance;

    public bool isGameOver = false;
    public bool isGameStarted = false;
    public float timeLeft = 15f;

    [Header("Scene Settings")]
    [Tooltip("Tên scene sẽ load khi thắng game. Cần add vào Build Settings.")]
    public string sceneToLoad = "Cho";

    [Header("UI Elements")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    
    [Header("Game Over Effects")]
    public GameObject wreckInstance;
    public AudioSource audioSource;
    public AudioClip newsClip;
    public AudioClip shutdownClip;
    public AudioClip tuyenTruyenClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Hiện và mở khóa cursor (scene trước có thể đã lock cursor cho FPS)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 0f; // Pause the game until Start is pressed
    }


    private void Update()
    {
        if (isGameStarted && !isGameOver)
        {
            timeLeft -= Time.deltaTime;
            
            if (timeLeft <= 0)
            {
                timeLeft = 0;
                WinGame();
            }

            if (scoreText != null)
                scoreText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }

    public void WinGame()
    {
        isGameOver = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;
        if (startPanel != null) startPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void GameOver(string reason)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f; // Stop game movement
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = reason;
            }
        }

        if (wreckInstance != null)
        {
            wreckInstance.SetActive(true);
        }

        if (audioSource != null && newsClip != null && shutdownClip != null)
        {
            StartCoroutine(PlayGameOverAudio());
        }
    }

    private IEnumerator PlayGameOverAudio()
    {
        // Sử dụng ignoreListenerPause để âm thanh vẫn chạy khi timeScale = 0 (nếu bị pause bởi AudioListener)
        audioSource.ignoreListenerPause = true; 
        
        audioSource.clip = newsClip;
        audioSource.Play();
        
        // Dùng WaitForSecondsRealtime vì timeScale = 0
        yield return new WaitForSecondsRealtime(newsClip.length);
        
        audioSource.clip = shutdownClip;
        audioSource.Play();

        if (tuyenTruyenClip != null)
        {
            yield return new WaitForSecondsRealtime(3f);
            audioSource.clip = tuyenTruyenClip;
            audioSource.Play();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
