using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardObject : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cek apakah yang menabrak adalah player
        if (collision.gameObject.CompareTag("Players"))
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // 1. Hitung arah pentalan
                // Menentukan apakah player berada di kanan (1) atau kiri (-1) dari rintangan.
                float directionX = Mathf.Sign(collision.transform.position.x - transform.position.x);

                // Buat vektor arah horizontal
                Vector2 knockbackDirection = new Vector2(directionX, 0);

                // 2. Panggil fungsi untuk memberikan knockback & stun ke player
                playerMovement.ApplyKnockbackAndStun(knockbackDirection);
            }
        }
    }
}
