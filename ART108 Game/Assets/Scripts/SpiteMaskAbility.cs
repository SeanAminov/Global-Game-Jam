using UnityEngine;

public class SpiteMaskAbility : MonoBehaviour
{
    [Header("Spite Mask Stats")]
    public int enhancedDamage = 3;
    public float enhancedRange = 2.5f;
    public float enhancedKnockbackForce = 8f;

    private PlayerAttack playerAttack;
    private Transform attackPoint;
    private Vector3 originalAttackPointScale;
    private int originalDamage;
    private float originalRange;
    private float originalKnockbackForce;
    private bool isEquipped = false;

    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            attackPoint = playerAttack.attackPoint;
            if (attackPoint != null)
            {
                originalAttackPointScale = attackPoint.localScale;
            }

            originalDamage = playerAttack.attackDamage;
            originalRange = playerAttack.attackRange;
            originalKnockbackForce = playerAttack.enemyKnockbackForce;
        }

        // Subscribe to mask changes
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskEquipped += HandleMaskEquipped;
            MaskManager.Instance.OnMaskUnequipped += HandleMaskUnequipped;
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
        if (playerAttack == null || isEquipped)
            return;

        isEquipped = true;

        // Enhance attack stats
        playerAttack.attackDamage = enhancedDamage;
        playerAttack.attackRange = enhancedRange;
        playerAttack.enemyKnockbackForce = enhancedKnockbackForce;

        // Scale attack point to match range increase
        if (attackPoint != null && originalRange > 0)
        {
            float scaleRatio = enhancedRange / originalRange;
            attackPoint.localScale = originalAttackPointScale * scaleRatio;
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
}
