using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Checkpoint Data")]
    public Vector3 startPosition;  // Set this to player's starting position
    private int currentCheckpointID = 0;  // 0 = no checkpoint, 1-3 = checkpoint IDs
    private Vector3 currentCheckpointPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Find player start position if not set
        if (startPosition == Vector3.zero)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                startPosition = player.transform.position;
            }
        }

        currentCheckpointPosition = startPosition;
    }

    public void SetCheckpoint(int checkpointID, Vector3 position)
    {
        currentCheckpointID = checkpointID;
        currentCheckpointPosition = position;
    }

    public Vector3 GetRespawnPosition()
    {
        return currentCheckpointID > 0 ? currentCheckpointPosition : startPosition;
    }

    public bool HasCheckpoint()
    {
        return currentCheckpointID > 0;
    }

    public void ResetToStart()
    {
        currentCheckpointID = 0;
        currentCheckpointPosition = startPosition;
    }
}
