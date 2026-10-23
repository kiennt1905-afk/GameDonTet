using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController characterController;
    private float verticalRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Khóa chuột vào giữa màn hình và ẩn con trỏ chuột khi chơi game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // 1. Xử lý QUAY CAMERA bằng chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Quay người theo trục ngang (Trái / Phải)
        transform.Rotate(Vector3.up * mouseX);

        // Quay camera theo trục dọc (Lên / Xuống) và giới hạn góc nhìn không bị lộn ngược
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // 2. Xử lý DI CHUYỂN bằng phím WASD / Mũi tên
        float moveForwardBack = Input.GetAxis("Vertical"); 
        float moveLeftRight = Input.GetAxis("Horizontal"); 

        Vector3 moveDirection = (transform.forward * moveForwardBack) + (transform.right * moveLeftRight);
        
        // Áp dụng trọng lực cơ bản để player chạm đất
        moveDirection.y = Physics.gravity.y;

        // Di chuyển nhân vật
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}