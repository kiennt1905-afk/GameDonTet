using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float jumpRepeatTime = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("First Person Camera Settings")]
    [SerializeField] private Transform cameraTransform; // Kéo Main Camera vào đây trong Inspector
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float topLookLimit = -80f;  // Giới hạn ngước lên
    [SerializeField] private float bottomLookLimit = 80f; // Giới hạn cúi xuống

    private float lastJumpTime;
    private Animator animator;
    private CharacterController controller;
    private float verticalVelocity;
    
    // Lưu trữ góc xoay trục X của Camera để giới hạn (Clamp)
    private float verticalRotation = 0f; 

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Khóa chuột vào giữa màn hình và ẩn con trỏ chuột khi chơi game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Nếu quên chưa kéo Camera vào Inspector, tự động lấy Main Camera
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Đồng bộ trạng thái Grounded để hiển thị trong Inspector debug
        isGrounded = controller.isGrounded;

        // --- 1. XỬ LÝ XOAY CAMERA & NHÂN VẬT (LOOK LOGIC) ---
        HandleMouseLook();

        // --- 2. XỬ LÝ DI CHUYỂN (MOVEMENT LOGIC) ---
        // Lấy giá trị input di chuyển (WASD)
        Vector2 input = InputSystem.actions["Move"].ReadValue<Vector2>();

        // Tính toán hướng di chuyển DỰA TRÊN HƯỚNG CỦA NHÂN VẬT (Local Space sang World Space)
        // transform.forward là hướng trước mặt, transform.right là hướng bên phải của nhân vật
        Vector3 moveDir = (transform.forward * input.y) + (transform.right * input.x);

        bool isMoving = input != Vector2.zero;

        // Set animator nếu model có animation góc nhìn thứ nhất
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
        }

        // --- 3. XỬ LÝ TRỌNG LỰC & NHẢY (JUMP & GRAVITY) ---
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f; 
            }

            if (InputSystem.actions["Jump"].WasPressedThisFrame() && Time.time > lastJumpTime + jumpRepeatTime)
            {
                // Công thức tính lực nhảy: v = sqrt(-2gh)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                // if (animator != null) animator.SetTrigger("Jump");
                lastJumpTime = Time.time;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // --- 4. ÁP DỤNG DI CHUYỂN CUỐI CÙNG ---
        Vector3 finalMovement = moveDir * movementSpeed;
        finalMovement.y = verticalVelocity; // Thêm vận tốc nhảy/rơi

        controller.Move(finalMovement * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (cameraTransform == null) return;

        // Lấy giá trị di chuyển chuột từ Input System mới
        // Lưu ý: Đảm bảo trong Input Action có Action tên là "Look" dạng Vector2 (mặc định của Unity có sẵn)
        Vector2 mouseInput = InputSystem.actions["Look"].ReadValue<Vector2>();

        float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseInput.y * mouseSensitivity * Time.deltaTime;

        // Xoay nhân vật theo trục Y (Quay trái / phải)
        transform.Rotate(Vector3.up * mouseX);

        // Tính toán góc ngước lên/xuống của Camera (Trục X)
        verticalRotation -= mouseY;
        // Giới hạn góc nhìn để Camera không bị lộn ngược ra phía sau khi ngước quá đà
        verticalRotation = Mathf.Clamp(verticalRotation, topLookLimit, bottomLookLimit);

        // Áp dụng góc xoay đó vào Camera
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}