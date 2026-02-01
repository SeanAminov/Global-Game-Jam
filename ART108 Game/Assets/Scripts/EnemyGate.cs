using UnityEngine;
using System.Collections.Generic;

public class EnemyGate : MonoBehaviour
{
    [Header("Enemies to Kill")]
    public List<GameObject> requiredEnemies = new List<GameObject>();
    
    [Header("Visual (Optional)")]
    public SpriteRenderer gateVisual;
    public bool hideWhenUnlocked = true;
    public bool destroyWhenUnlocked = false;

    private Collider2D gateCollider;
    private bool isUnlocked = false;

    void Start()
    {
        gateCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isUnlocked)
            return;

        // Check if all required enemies are dead
        bool allDead = true;
        foreach (GameObject enemy in requiredEnemies)
        {
            if (enemy != null)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            Unlock();
        }
    }

    private void Unlock()
    {
        isUnlocked = true;

        // Disable collider so player can pass
        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }

        // Hide or destroy visual
        if (destroyWhenUnlocked)
        {
            Destroy(gameObject);
        }
        else if (hideWhenUnlocked)
        {
            if (gateVisual != null)
            {
                gateVisual.enabled = false;
            }
            else
            {
                // Try to hide self if no separate visual assigned
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
        }
    }
}
