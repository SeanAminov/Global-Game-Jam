using UnityEngine;
using UnityEngine.InputSystem;

public class SadMaskAbility : MonoBehaviour
{
    [Header("Tear Projectile")]
    public GameObject tearPrefab;
    public Transform shootPoint;
    public LayerMask enemyLayer;
    public float shootCooldown = 0.3f;

    private float lastShootTime = -999f;

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed && MaskManager.Instance != null && MaskManager.Instance.IsMaskEquipped(MaskType.Sad))
        {
            if (Time.time >= lastShootTime + shootCooldown)
            {
                PerformShoot();
                lastShootTime = Time.time;
            }
        }
    }

    private void PerformShoot()
    {
        if (tearPrefab == null || shootPoint == null)
        {
            Debug.LogWarning("Tear prefab or shoot point not assigned!");
            return;
        }

        Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        GameObject tear = Instantiate(tearPrefab, shootPoint.position, Quaternion.identity);
        TearProjectile projectile = tear.GetComponent<TearProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(shootDirection);
            projectile.enemyLayer = enemyLayer;
        }
    }
}
