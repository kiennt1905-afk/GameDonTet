using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Khôi phục vị trí cũ khi từ Minigame quay lại
        if (PlayerPrefs.GetInt("HasSavedPos", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            transform.position = new Vector3(x, y, z);
            
            if (PlayerPrefs.HasKey("PlayerRotY"))
            {
                float rotY = PlayerPrefs.GetFloat("PlayerRotY");
                transform.rotation = Quaternion.Euler(0, rotY, 0);
            }

            // Xóa cờ để lần sau chơi game mới không bị dính vị trí cũ
            PlayerPrefs.SetInt("HasSavedPos", 0); 
            PlayerPrefs.Save();
        }
    }

    void FixedUpdate()
    {
        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
        }

        Vector3 moveDirection = new Vector3(h, 0, v).normalized;

        rb.MovePosition(
            rb.position +
            moveDirection * moveSpeed * Time.fixedDeltaTime
        );
    }
}