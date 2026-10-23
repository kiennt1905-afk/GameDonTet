// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PlayerController : MonoBehaviour
// {
//     [Header("Di Chuyển")]
//     public float moveSpeed = 5f;
//     public float gravity = -9.81f;
//     private CharacterController controller;
//     private Vector3 velocity;

//     [Header("Góc Nhìn Chuột")]
//     public Transform cameraTransform;
//     public float mouseSensitivity = 200f;
//     private float xRotation = 0f;
//     public InputAction moveAction; // Vector2 (x: left/right, y: forward/back)
//     public InputAction lookAction; // Vector2 (x: deltaX, y: deltaY)

//     void Start()
//     {
//         controller = GetComponent<CharacterController>();
//         if (controller == null) Debug.LogError("PlayerController: No CharacterController found on this GameObject.");
//         if (cameraTransform == null) Debug.LogError("PlayerController: cameraTransform is not assigned.");
        
//         // Khóa chuột vào giữa màn hình và ẩn con trỏ chuột đi
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }

//     void OnEnable()
//     {
//         if (moveAction != null) moveAction.Enable();
//         if (lookAction != null) lookAction.Enable();
//     }

//     void OnDisable()
//     {
//         if (moveAction != null) moveAction.Disable();
//         if (lookAction != null) lookAction.Disable();
//     }

//     void Update()
//     {
//         if (controller == null) controller = GetComponent<CharacterController>();
//         HandleLook();
//         HandleMove();
//     }

//     void HandleLook()
//     {
//         Vector2 lookDelta = Vector2.zero;

//         if (Mouse.current != null)
//         {
//             lookDelta = Mouse.current.delta.ReadValue();
//         }

//         if (lookDelta.sqrMagnitude < 0.01f)
//             return;

//         // Độ nhạy được tối ưu cho cảm giác chậm & mượt
//         float sensitivityMultiplier = mouseSensitivity * 0.015f * Time.deltaTime * 60f;

//         float mouseX = lookDelta.x * sensitivityMultiplier;
//         float mouseY = lookDelta.y * sensitivityMultiplier;

//         // Xoay ngang nhân vật (trái phải)
//         transform.Rotate(Vector3.up * mouseX, Space.World);

//         // Xoay dọc camera (nhìn lên xuống)
//         xRotation -= mouseY;
//         xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
//         if (cameraTransform != null)
//             cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//     }

//     void HandleMove()
//     {
//         // Kiểm tra xem nhân vật có chạm đất không
//         if (controller.isGrounded && velocity.y < 0)
//         {
//             velocity.y = -2f; // Giữ nhân vật bám đất ổn định
//         }

//         // Lấy input từ Input System devices trực tiếp (ưu tiên), hoặc từ moveAction, hoặc fallback legacy
//         float x = 0f;
//         float z = 0f;
//         var kb = Keyboard.current;
//         if (kb != null)
//         {
//             if (kb.wKey.isPressed) z += 1f;
//             if (kb.sKey.isPressed) z -= 1f;
//             if (kb.aKey.isPressed) x -= 1f;
//             if (kb.dKey.isPressed) x += 1f;
//             //Debug.Log("HandleMove: using Keyboard.current");
//         }
//         else if (moveAction != null)
//         {
//             Vector2 inV = moveAction.ReadValue<Vector2>();
//             x = inV.x;
//             z = inV.y;
//             //Debug.Log("HandleMove: using moveAction");
//         }
//         else
//         {
//             try { x = Input.GetAxis("Horizontal"); z = Input.GetAxis("Vertical"); }
//             catch (System.InvalidOperationException) { x = 0f; z = 0f; }
//         }

//         // Tính hướng đi dựa theo hướng nhân vật đang nhìn
//         Vector3 move = transform.right * x + transform.forward * z;

//         if (move.magnitude > 1f) move.Normalize();

//         // Di chuyển nhân vật
//         controller.Move(move * moveSpeed * Time.deltaTime);

//         // Áp dụng trọng lực
//         velocity.y += gravity * Time.deltaTime;
//         controller.Move(velocity * Time.deltaTime);
//     }
// }