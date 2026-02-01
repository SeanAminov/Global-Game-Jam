using UnityEngine;

public class SpiteMaskAbility : MonoBehaviour
{
    [Header("Spite Mask Stats")]
    public int enhancedDamage = 3;
    public float enhancedRange = 2.5f;
    public float enhancedKnockbackForce = 8f;

    [Header("Visual Scale")]
    public float attackPointScaleMultiplier = 1.67f;

    [Header("Aura Blast")]
    public int slashCount = 12;
    public float pulseStartSize = 0.5f;
    public float pulseEndSize = 3f;
    public float pulseDuration = 0.4f;

    [Header("References (Auto-assigned)")]
    public PlayerAttack playerAttack;
    public Transform attackPoint;

    private Vector3 originalAttackPointScale;
    private int originalDamage;
    private float originalRange;
    private float originalKnockbackForce;
    private bool isEquipped = false;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        // Retry initialization if MaskManager wasn't ready
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

        if (playerAttack != null)
        {
            if (attackPoint == null)
            {
                attackPoint = playerAttack.attackPoint;
            }

            if (attackPoint != null)
            {
                originalAttackPointScale = attackPoint.localScale;
            }

            originalDamage = playerAttack.attackDamage;
            originalRange = playerAttack.attackRange;
            originalKnockbackForce = playerAttack.enemyKnockbackForce;
            
            Debug.Log($"SpiteMaskAbility initialized: originalRange={originalRange}, originalDamage={originalDamage}");
        }

        // Subscribe to mask changes
        if (MaskManager.Instance != null && !isInitialized)
        {
            MaskManager.Instance.OnMaskEquipped += HandleMaskEquipped;
            MaskManager.Instance.OnMaskUnequipped += HandleMaskUnequipped;
            isInitialized = true;
            Debug.Log("SpiteMaskAbility subscribed to MaskManager events");
        }
    }

    void OnDestroy()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskEquipped -= HandleMaskEquipped;
            MaskManager.Instance.OnMaskUnequipped -= HandleMaskUnequipped;
        }
    }

    private void HandleMaskEquipped(MaskType maskType)
    {
        if (maskType == MaskType.Spite)
        {
            EquipSpiteMask();
        }
    }

    private void HandleMaskUnequipped(MaskType maskType)
    {
        if (maskType == MaskType.Spite)
        {
            UnequipSpiteMask();
        }
    }

    private void EquipSpiteMask()
    {
        if (playerAttack == null)
        {
            Debug.LogWarning("SpiteMaskAbility: PlayerAttack is null!");
            return;
        }
        
        if (isEquipped)
            return;

        isEquipped = true;

        Debug.Log($"Equipping Spite Mask: damage {originalDamage} -> {enhancedDamage}, range {originalRange} -> {enhancedRange}");

        // Enhance attack stats
        playerAttack.attackDamage = enhancedDamage;
        playerAttack.attackRange = enhancedRange;
        playerAttack.enemyKnockbackForce = enhancedKnockbackForce;

        // Scale attack point visually
        if (attackPoint != null)
        {
            attackPoint.localScale = originalAttackPointScale * attackPointScaleMultiplier;
            Debug.Log($"Scaling attack point by {attackPointScaleMultiplier}x");
        }
    }

    private void UnequipSpiteMask()
    {
        if (playerAttack == null || !isEquipped)
            return;

        isEquipped = false;

        // Restore original stats
        playerAttack.attackDamage = originalDamage;
        playerAttack.attackRange = originalRange;
        playerAttack.enemyKnockbackForce = originalKnockbackForce;

        // Restore original attack point scale
        if (attackPoint != null)
        {
            attackPoint.localScale = originalAttackPointScale;
        }
    }

    /// <summary>
    /// Call this from PlayerAttack when attacking in Spite mode to fire aura blast
    /// </summary>
    public void FireAuraBlast()
    {
        if (!isEquipped || attackPoint == null)
            return;

        // Get the attack point's sprite
        SpriteRenderer attackSprite = attackPoint.GetComponent<SpriteRenderer>();
        if (attackSprite == null || attackSprite.sprite == null)
        {
            Debug.LogWarning("SpiteMaskAbility: No sprite on attack point!");
            return;
        }

        StartCoroutine(PulseAuraBlast(attackSprite));
    }

    private System.Collections.IEnumerator PulseAuraBlast(SpriteRenderer attackSprite)
    {
        float angleStep = 360f / slashCount;
        GameObject[] slashes = new GameObject[slashCount];
        
        // Create a single centered hitbox that will expand
        GameObject hitboxObj = new GameObject("SpitePulseHitbox");
        hitboxObj.transform.position = transform.position;
        CircleCollider2D mainHitbox = hitboxObj.AddComponent<CircleCollider2D>();
        mainHitbox.isTrigger = true;
        mainHitbox.radius = pulseStartSize;
        
        // Add damage script to the main hitbox
        SpiteSlash hitboxDamage = hitboxObj.AddComponent<SpiteSlash>();
        hitboxDamage.damage = enhancedDamage;
        hitboxDamage.knockbackForce = enhancedKnockbackForce;
        
        // Track which enemies have been hit to prevent multiple hits
        hitboxDamage.trackHitEnemies = true;

        // Create visual slashes (no colliders - just for looks)
        for (int i = 0; i < slashCount; i++)
        {
            float angle = i * angleStep;

            GameObject slash = new GameObject("SpiteSlashVisual");
            slash.transform.position = transform.position;
            slash.transform.localScale = Vector3.one * pulseStartSize;
            
            // Copy sprite
            SpriteRenderer sr = slash.AddComponent<SpriteRenderer>();
            sr.sprite = attackSprite.sprite;
            sr.color = attackSprite.color;
            sr.sortingLayerID = attackSprite.sortingLayerID;
            sr.sortingOrder = attackSprite.sortingOrder;
            
            // Rotate to face outward direction
            slash.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            slashes[i] = slash;
        }

        // Animate pulse - scale up from center
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;
            float currentSize = Mathf.Lerp(pulseStartSize, pulseEndSize, t);
            
            // Also fade out as it expands
            float alpha = Mathf.Lerp(1f, 0f, t);

            // Update main hitbox size and keep centered
            hitboxObj.transform.position = transform.position;
            mainHitbox.radius = currentSize;

            // Update visual slashes
            for (int i = 0; i < slashCount; i++)
            {
                if (slashes[i] != null)
                {
                    slashes[i].transform.localScale = Vector3.one * currentSize;
                    slashes[i].transform.position = transform.position; // Keep centered on player

                    // Fade out
                    SpriteRenderer sr = slashes[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = alpha;
                        sr.color = c;
                    }
                }
            }

            yield return null;
        }

        // Destroy hitbox
        Destroy(hitboxObj);

        // Destroy all visual slashes
        for (int i = 0; i < slashCount; i++)
        {
            if (slashes[i] != null)
            {
                Destroy(slashes[i]);
            }
        }

        Debug.Log($"Spite aura pulse complete!");
    }
}
