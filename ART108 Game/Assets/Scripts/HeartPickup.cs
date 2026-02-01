using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Header("Heart Settings")]
    public int heartValue = 1;
    public bool expandMaxHealth = true;  // If true, gains a heart slot; if false, just heals

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if player walked over it
        if (collision.CompareTag("Player"))
        {
            Collect(collision.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        if (isCollected)
            return;

        isCollected = true;

        // Apply heart to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (expandMaxHealth)
            {
                // Gain a new heart slot
                playerHealth.GainMaxHealth(heartValue);
                Debug.Log($"Heart pickup collected! Gained {heartValue} max heart(s).");
            }
            else
            {
                // Just heal
                playerHealth.Heal(heartValue);
                Debug.Log($"Heart pickup collected! Restored {heartValue} hearts.");
            }
        }

        // Destroy heart
        Destroy(gameObject);
    }
}
