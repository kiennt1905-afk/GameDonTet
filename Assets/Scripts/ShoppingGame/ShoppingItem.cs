using UnityEngine;

namespace ShoppingGame
{
    public class ShoppingItem : MonoBehaviour
    {
        public string itemName;
        public float fallSpeed = 5f;
        public float destroyY = -10f; // Y position at which the item is destroyed

        // Give some random rotation for better visuals
        private Vector3 rotationSpeed;

        void Start()
        {
            rotationSpeed = new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f)
            );
        }

        void Update()
        {
            if (ShoppingGameManager.Instance != null && (!ShoppingGameManager.Instance.IsGameStarted || ShoppingGameManager.Instance.IsGameOver)) return;

            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            transform.Rotate(rotationSpeed * Time.deltaTime);

            if (transform.position.y < destroyY)
            {
                Destroy(gameObject);
            }
        }
    }
}
