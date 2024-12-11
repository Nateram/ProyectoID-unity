// Bat.cs
using UnityEngine;
using System.Collections;

public class Bat : Enemy
{
    [Header("Movement Settings")]
    [SerializeField] private float heightAbovePlayer = 3f; // Height to maintain above player
    [SerializeField] private float flySpeed = 5f;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private float maxDistanceFromPlayer = 8f;
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverFrequency = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float shootingCooldown = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;

   [Header("Movement Settings")]
    [SerializeField] private float maxTravelDistance = 10f; // Max distance from start
    private Transform player;
    private float nextShootTime;
    private Vector3 startPosition;
    private float hoverOffset;

    private bool isPlayerDetected = false;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        if (!isRecoiling && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            // Check for initial detection
            if (!isPlayerDetected && distanceToPlayer <= attackRange)
            {
                isPlayerDetected = true;
            }
            
            // Only move and attack if player is detected
            if (isPlayerDetected)
            {
                Movement();
                CheckShoot();
                UpdateFacing();
            }
        }
    }

    private void Movement()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        
        // Calculate target position above player
        Vector2 targetHeight = new Vector2(transform.position.x, player.position.y + heightAbovePlayer);
        
        // Hover effect
        hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;

        // Calculate movement
        Vector2 moveDirection;
        if (distanceFromStart >= maxTravelDistance)
        {
            // Return to start boundary
            moveDirection = ((Vector2)startPosition - (Vector2)transform.position).normalized;
        }
        else if (distanceToPlayer < minDistanceFromPlayer)
        {
            // Move away from player while maintaining height
            moveDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
            moveDirection.y = (targetHeight.y - transform.position.y) * 0.5f;
        }
        else if (distanceToPlayer > maxDistanceFromPlayer)
        {
            // Move towards player while maintaining height
            moveDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            moveDirection.y = (targetHeight.y - transform.position.y) * 0.5f;
        }
        else
        {
            // Maintain height and hover
            moveDirection = new Vector2(0, (targetHeight.y - transform.position.y));
        }

        // Apply movement
        rb.velocity = moveDirection.normalized * flySpeed + Vector2.up * hoverOffset;
    }

    private void CheckShoot()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange && Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootingCooldown;
        }
    }

    private void Shoot()
    {
        Debug.Log("Attempting to shoot");
        if (projectilePrefab == null)
        {
            Debug.LogError("No projectile prefab assigned!");
            return;
        }

        Vector2 spawnPosition = transform.position - Vector3.up * 0.5f; // Spawn below bat
        Vector2 direction = (player.position - transform.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }
        else
        {
            Debug.LogError("No Rigidbody2D on projectile!");
        }
    }

    private void UpdateFacing()
    {
        // Update sprite facing direction
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? 1 : -1);
        transform.localScale = scale;
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0.005f, 20, 0.1f);
        }
    }

    protected override void Attack()
    {
        base.Attack(); // Uses Enemy.Attack() which handles damage
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, maxTravelDistance);
    }
}