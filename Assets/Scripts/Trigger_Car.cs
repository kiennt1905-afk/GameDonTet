using UnityEngine;
using UnityEngine.SceneManagement;

public class Trigger_Car : MonoBehaviour
{
    [Tooltip("Tên Scene cần load khi tương tác")]
    public string sceneToLoad = "Nha";

    public void Interact()
    {
        Debug.Log("Trigger_Car: Đang load scene " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
