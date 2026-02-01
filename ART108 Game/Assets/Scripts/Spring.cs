using UnityEngine;

public class Spring : MonoBehaviour
{
    [Header("Spring Settings")]
    public float bounceForce = 20f;
    public bool resetJumps = true;

    [Header("Audio")]
    public AudioClip springSound;
    [Range(0f, 2f)]
    public float springVolume = 1f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Apply bounce force
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            }

            // Reset jumps
            if (resetJumps)
            {
                PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.ResetJumps();
                }
            }

            // Play sound
            if (audioSource != null && springSound != null)
            {
                audioSource.PlayOneShot(springSound, springVolume);
            }
        }
    }
}
