using UnityEngine;

namespace ShoppingGame
{
    public class ShoppingBasket : MonoBehaviour
    {
        public float moveSpeed = 15f;
        public float limitX = 8f; // Limit horizontal movement

        void Update()
        {
            if (ShoppingGameManager.Instance != null && (!ShoppingGameManager.Instance.IsGameStarted || ShoppingGameManager.Instance.IsGameOver)) return;

            float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow

            // Also allow mouse/touch follow if holding down button
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Move towards mouse X
                float direction = mousePos.x > transform.position.x ? 1 : -1;
                if (Mathf.Abs(mousePos.x - transform.position.x) > 0.5f)
                {
                    horizontalInput = direction;
                }
            }

            Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, -limitX, limitX);
            transform.position = newPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ShoppingItem"))
            {
                ShoppingItem item = other.GetComponent<ShoppingItem>();
                if (item != null)
                {
                    if (ShoppingGameManager.Instance != null && ShoppingGameManager.Instance.IsGameStarted && !ShoppingGameManager.Instance.IsGameOver)
                    {
                        ShoppingGameManager.Instance.OnItemCaught(item.itemName);
                        Destroy(other.gameObject);
                    }
                }
            }
        }
    }
}
