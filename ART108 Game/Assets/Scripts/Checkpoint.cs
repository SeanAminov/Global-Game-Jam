using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public int checkpointID = 1;  // 1, 2, 3 for each mask area
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;

    [Header("Visual")]
    public SpriteRenderer checkpointSprite;
    public ParticleSystem activationEffect;

    [Header("Audio")]
    public AudioClip activationSound;
    private AudioSource audioSource;

    private bool isActivated = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (checkpointSprite != null)
        {
            checkpointSprite.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // Update visual
        if (checkpointSprite != null)
        {
            checkpointSprite.color = activeColor;
        }

        // Play effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        // Play sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Register with checkpoint manager
        CheckpointManager.Instance?.SetCheckpoint(checkpointID, transform.position);
    }

    public void Deactivate()
    {
        isActivated = false;
        if (checkpointSprite != null)
        {
            checkpointSprite.color = inactiveColor;
        }
    }
}
