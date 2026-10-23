using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class DustMinigame : MonoBehaviour
{
    [Header("Cameras & Tools")]
    public Camera mainCamera;
    public Camera minigameCamera;
    public GameObject cleaningTool;

    [Header("Interaction")]
    public Transform playerTransform;
    public Transform furnitureTransform;
    public Collider furnitureCollider;
    public float interactDistance = 2.5f;

    [Header("Player Control")]
    [SerializeField] private Behaviour playerController;

    [Header("Dust Decal")]
    public DecalProjector dustDecal;
    [Range(0f,1f)]
    public float currentOpacity = 1f;

    public float cleaningSpeed = 0.15f;
    public float minBrushMovePixels = 4f;
    public float movementToCleaningMultiplier = 0.005f;

    [Header("Raycast Layer")]
    public LayerMask furnitureLayer;
    public Vector3 brushSlideAxis = Vector3.right;
    public float brushSlideRange = 0.18f;
    public float brushSlideSpeed = 0.0025f;

    private bool isMinigameActive = false;
    private bool isCleaningCompleted = false;
    private bool waitingForExitAfterClean = false;
    private bool canInteractNow = false;

    private Vector3 lastMousePosition;
    private bool hasLastMousePosition = false;

    private Vector3 brushBasePosition;
    private Quaternion brushBaseRotation;
    private float brushSlideOffset = 0f;
    private string interactionMessage = "Press F to clean";

    [Header("Task Management")]
    public string taskIdToComplete = "Cleaning";
    private bool hasCompletedTask = false;
    
    [Tooltip("Kéo các vật thể muốn TẮT (ẩn đi) sau khi dọn dẹp xong vào đây")]
    public GameObject[] objectsToHideAfter;

    void Start()
    {
        ResolvePlayerController();

        if (minigameCamera != null)
            minigameCamera.gameObject.SetActive(false);

        if (cleaningTool != null)
            cleaningTool.SetActive(false);

        currentOpacity = Mathf.Clamp01(currentOpacity);
        ApplyOpacity();
    }

    void Update()
    {
        if (waitingForExitAfterClean)
        {
            if (Input.anyKeyDown)
            {
                ExitMinigame();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isMinigameActive)
        {
            ExitMinigame();
        }

        if (isMinigameActive)
        {
            MoveToolWithMouse();
            TryCleaning();
        }
    }

    /// <summary>
    /// Hàm này sẽ được gọi từ PlayerInteract khi người chơi đứng trong TriggerBox và bấm E
    /// </summary>
    public void Interact()
    {
        if (!isMinigameActive && !isCleaningCompleted)
        {
            EnterMinigame();
        }
    }

    void EnterMinigame()
    {
        isMinigameActive = true;

        if (cleaningTool != null)
        {
            brushBasePosition = cleaningTool.transform.position;
            brushBaseRotation = cleaningTool.transform.rotation;
            brushSlideOffset = 0f;
        }

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        if (minigameCamera != null)
            minigameCamera.gameObject.SetActive(true);

        if (cleaningTool != null)
            cleaningTool.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        hasLastMousePosition = false;
    }

    void ExitMinigame()
    {
        isMinigameActive = false;
        waitingForExitAfterClean = false;

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        if (minigameCamera != null)
            minigameCamera.gameObject.SetActive(false);

        if (cleaningTool != null)
            cleaningTool.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hoàn thành task khi chơi xong và thoát ra
        if (isCleaningCompleted && !hasCompletedTask)
        {
            hasCompletedTask = true;

            // Ẩn các vật thể được chỉ định
            if (objectsToHideAfter != null)
            {
                foreach (GameObject obj in objectsToHideAfter)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(taskIdToComplete);
            }
        }
    }

    void MoveToolWithMouse()
    {
        if (cleaningTool == null)
        {
            return;
        }

        Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        brushSlideOffset -= mouseDelta.x * brushSlideSpeed;
        brushSlideOffset = Mathf.Clamp(brushSlideOffset, -brushSlideRange, brushSlideRange);

        Vector3 slideAxis = brushSlideAxis.sqrMagnitude > 0.0001f
            ? brushSlideAxis.normalized
            : Vector3.right;

        Vector3 worldSlideAxis = brushBaseRotation * slideAxis;
        cleaningTool.transform.SetPositionAndRotation(
            brushBasePosition + worldSlideAxis * brushSlideOffset,
            brushBaseRotation);
    }

    void TryCleaning()
    {
        if (isCleaningCompleted)
            return;

        Vector3 currentMousePosition =
            new Vector3(brushSlideOffset * 1000f, 0f, 0f);

        if (!hasLastMousePosition)
        {
            lastMousePosition = currentMousePosition;
            hasLastMousePosition = true;
            return;
        }

        float movePixels =
            Vector2.Distance(
                currentMousePosition,
                lastMousePosition);

        lastMousePosition = currentMousePosition;

        if (movePixels < minBrushMovePixels)
            return;

        float cleaningAmount =
            cleaningSpeed *
            Time.deltaTime *
            Mathf.Clamp(
                movePixels / 50f,
                0.2f,
                2f);

        currentOpacity -= cleaningAmount * 0.01f;
        currentOpacity = Mathf.Clamp01(currentOpacity);

        ApplyOpacity();

        if (currentOpacity <= 0f)
        {
            isCleaningCompleted = true;
            waitingForExitAfterClean = true;

            Debug.Log("Cleaned. Press any key to exit the minigame.");
        }
    }

    void ApplyOpacity()
    {
        if (dustDecal == null)
            return;

        dustDecal.fadeFactor = currentOpacity;
    }

    // Đã xoá UpdateInteractionState vì giờ dùng PlayerInteract và TriggerBox
    void ResolvePlayerController()
    {
        if (playerController != null)
            return;

        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponentInParent<global::PlayerController>();
            if (playerController != null)
                return;
        }

        playerController = FindFirstObjectByType<global::PlayerController>();
    }

    void OnGUI()
    {
        if (waitingForExitAfterClean)
        {
            const int completionBoxWidth = 420;
            const int completionBoxHeight = 64;
            Rect completionRect = new Rect((Screen.width - completionBoxWidth) * 0.5f, Screen.height - 120f, completionBoxWidth, completionBoxHeight);

            GUI.depth = -1000;
            GUI.Box(completionRect, GUIContent.none);

            GUIStyle completionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUI.Label(completionRect, "Cleaned. Press any key to exit.", completionStyle);
            return;
        }
    }
}