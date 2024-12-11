// BatProjectile.cs
using UnityEngine;

public class BatProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask collisionLayers; // Set to Ground and Player layers

    private Rigidbody2D rb;

    private void Start()
    {
        Debug.Log("Projectile spawned");
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Projectile collided with: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        if (collision.gameObject.CompareTag("Suelo") || collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
            {
                PlayerController.Instance.TakeDamage(damage, transform.position);
                PlayerController.Instance.HitStopTime(0.005f, 20, 0.1f);
                Debug.Log("Player hit by projectile");
            }
            
            Debug.Log("Destroying projectile");
            Destroy(gameObject);
        }
    }
}