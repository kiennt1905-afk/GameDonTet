using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string houseSceneName = "House";

    private void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadHouseScene);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitGame);
        }
    }

    public void LoadHouseScene()
    {
        SceneManager.LoadScene(houseSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
