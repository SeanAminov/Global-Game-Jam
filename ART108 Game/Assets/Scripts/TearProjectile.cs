using UnityEngine;

public class TearProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;
    public LayerMask enemyLayer;
    public LayerMask[] collisionLayers;
    public GameObject impactEffect;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float spawnTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int hitLayerMask = 1 << collision.gameObject.layer;
        if ((hitLayerMask & enemyLayer) != 0)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, direction, 3f);
            }

            ExplodeAndDestroy();
            return;
        }

        if ((hitLayerMask & GetCollisionLayerMask()) != 0)
        {
            ExplodeAndDestroy();
        }
    }

    private int GetCollisionLayerMask()
    {
        if (collisionLayers == null || collisionLayers.Length == 0)
        {
            return 0;
        }

        int combined = 0;
        for (int i = 0; i < collisionLayers.Length; i++)
        {
            combined |= collisionLayers[i];
        }

        return combined;
    }

    private void ExplodeAndDestroy()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
