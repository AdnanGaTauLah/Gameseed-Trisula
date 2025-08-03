using UnityEngine;

public class TrampoulineObject : MonoBehaviour
{private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang masuk trigger adalah player
        if (other.CompareTag("Players"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // Pastikan player sedang jatuh (agar tidak terpantul saat berjalan di atasnya)
            if (playerRb != null && playerRb.velocity.y <= 0f)
            {
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    // Panggil fungsi pantulan di player
                    playerMovement.BounceOnTrampoline();
                }
            }
        }
    }
}
