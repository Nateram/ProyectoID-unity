using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xero : Enemy
{
    [Header("Phase 2 Settings")]
    [SerializeField] private float phase2AttackCooldown = 1.5f;
    [SerializeField] private float extraSwordOffset = 4f;
    private bool isPhase2 = false;

    [Header("UI Settings")]
    [SerializeField] private UnityEngine.UI.Image healthBar;

    [Header("UI Settings")]
    [SerializeField] private UnityEngine.UI.Image healthBarBorder; // Reference to outer border image
    [SerializeField] private float initialHealth;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float movementRange = 5f;
    [SerializeField] private float directionChangeTime = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Sword Settings")]
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private float swordOffset = 2f;
    [SerializeField] private float attackCooldown = 3f;

    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;

[Header("Hit Settings")]
[SerializeField] private float knockbackForceX = 10f;
[SerializeField] private float knockbackForceY = 5f;
[SerializeField] private float knockbackDuration = 0.2f;
[SerializeField] private float invincibilityDuration = 0.4f;

private SpriteRenderer spriteRenderer;

private bool isInvincible = false;

    private bool inCombat = false;
    private float floatTimer;
    private float startY;
    private Vector3 startPosition;
    private float directionTimer;
    private int moveDirection = 1;
    private bool playerDetected;
    private float nextAttackTime;
    private espadasxero[] swords;

    Animator anim;


    protected override void Start()
    {   
        base.Start();
        anim = GetComponent<Animator>();
        initialHealth = health;
        startPosition = transform.position;
        startY = startPosition.y;
        floatTimer = 0f;
        spriteRenderer=GetComponent<SpriteRenderer>();
        swords = new espadasxero[4]; // Increased array size
        SpawnInitialSwords();
    // Initialize health bar
        if (healthBar != null)
        {
            healthBar.fillAmount = health / initialHealth;
            UpdateHealthBarVisibility(false); // Start invisible
        }
    }
private void UpdateHealthBarVisibility(bool visible)
{
    if (healthBarBorder != null && healthBar != null)
    {
        Color borderColor = healthBarBorder.color;
        Color fillColor = healthBar.color;
        
        borderColor.a = visible ? 1f : 0f;
        fillColor.a = visible ? 1f : 0f;
        
        healthBarBorder.color = borderColor;
        healthBar.color = fillColor;
    }
}
protected override void Update()
{
    base.Update();
    CheckPlayerDistance();
    
    // Update health bar visibility
    UpdateHealthBarVisibility(inCombat && health > 0);
    
    if (inCombat)
    {
        Movement();
        UpdateSwordPositions();
        TryAttack();
    }
}

private void SpawnInitialSwords()
{
    // Use swordOffset for initial swords
    Vector3 leftPos = transform.position + Vector3.left * swordOffset;
    Vector3 rightPos = transform.position + Vector3.right * swordOffset;

    GameObject leftSword = Instantiate(swordPrefab, leftPos, Quaternion.Euler(0, 0, -90));
    GameObject rightSword = Instantiate(swordPrefab, rightPos, Quaternion.Euler(0, 0, -90));
    
    leftSword.transform.SetParent(transform);
    rightSword.transform.SetParent(transform);
    
    swords[0] = leftSword.GetComponent<espadasxero>();
    swords[1] = rightSword.GetComponent<espadasxero>();

    // Initialize with correct offset
    swords[0].Initialize(transform, swordOffset, true);
    swords[1].Initialize(transform, swordOffset, false);
}

// En Xero.cs, modifica SpawnPhase2SwordsSequence:
private IEnumerator SpawnPhase2SwordsSequence()
{
    // Log distancias actuales de las espadas originales
    Debug.Log($"Espada izquierda original distancia: {Vector3.Distance(swords[0].transform.position, transform.position)}");
    Debug.Log($"Espada derecha original distancia: {Vector3.Distance(swords[1].transform.position, transform.position)}");
    
    // Calcular posiciones finales con extraSwordOffset
    Vector3 farLeftPos = transform.position + Vector3.left * extraSwordOffset;
    Vector3 farRightPos = transform.position + Vector3.right * extraSwordOffset;
    
    Debug.Log($"Posición objetivo izquierda: {farLeftPos}, Offset: {extraSwordOffset}");
    Debug.Log($"Posición objetivo derecha: {farRightPos}, Offset: {extraSwordOffset}");

    // Spawn directo en posiciones finales
    GameObject farLeftSword = Instantiate(swordPrefab, farLeftPos, Quaternion.Euler(0, 0, -90));
    GameObject farRightSword = Instantiate(swordPrefab, farRightPos, Quaternion.Euler(0, 0, -90));
    
    farLeftSword.transform.SetParent(transform);
    farRightSword.transform.SetParent(transform);
    
    swords[2] = farLeftSword.GetComponent<espadasxero>();
    swords[3] = farRightSword.GetComponent<espadasxero>();

    // Inicializar con posiciones correctas
    swords[2].Initialize(transform, extraSwordOffset, true);  // true para izquierda
    swords[3].Initialize(transform, extraSwordOffset, false); // false para derecha

    Debug.Log($"Espada fase 2 izquierda distancia: {Vector3.Distance(swords[2].transform.position, transform.position)}");
    Debug.Log($"Espada fase 2 derecha distancia: {Vector3.Distance(swords[3].transform.position, transform.position)}");

    yield return null;
}

    private void Movement()
    {
        directionTimer += Time.deltaTime;
        floatTimer += Time.deltaTime;

        if (directionTimer >= directionChangeTime)
        {
            moveDirection *= -1;
            directionTimer = 0;
        }

        float newX = transform.position.x + (moveDirection * moveSpeed * Time.deltaTime);
        float distanceFromStart = Mathf.Abs(newX - startPosition.x);

        if (distanceFromStart > movementRange)
        {
            moveDirection *= -1;
            newX = startPosition.x + (movementRange * Mathf.Sign(moveDirection));
        }

        float newY = startY + (Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude);

        Vector2 targetVelocity = new Vector2(moveDirection * moveSpeed, 0);
        rb.velocity = new Vector2(targetVelocity.x, 0);

        float clampedX = Mathf.Clamp(transform.position.x, 
            startPosition.x - movementRange, 
            startPosition.x + movementRange);
        transform.position = new Vector3(clampedX, newY, transform.position.z);
    }

    private void CheckPlayerDistance()
    {
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        bool wasDetected = playerDetected;
        playerDetected = distanceToPlayer <= detectionRange;

        // Enter combat
        if (playerDetected && !inCombat)
        {
            inCombat = true;
        }
        // Don't exit combat just because player left detection range
        else if (!playerDetected && inCombat && distanceToPlayer > detectionRange * 2)
        {
            inCombat = false;
        }
    }

private void UpdateSwordPositions()
{
    // Update initial swords with swordOffset
    if (swords[0] != null)
        swords[0].UpdatePosition(transform.position + Vector3.left * swordOffset);
    if (swords[1] != null)
        swords[1].UpdatePosition(transform.position + Vector3.right * swordOffset);
    
    // Update phase 2 swords with extraSwordOffset
    if (isPhase2)
    {
        if (swords[2] != null)
            swords[2].UpdatePosition(transform.position + Vector3.left * extraSwordOffset);
        if (swords[3] != null)
            swords[3].UpdatePosition(transform.position + Vector3.right * extraSwordOffset);
    }
}

private void TryAttack()
{
    if (Time.time >= nextAttackTime)
    {
        int maxSwords = isPhase2 ? 4 : 2;
        int randomSword = Random.Range(0, maxSwords);
        if (swords[randomSword] != null && !swords[randomSword].IsAttacking)
        {
            StartCoroutine(PlayAttackAnimation());
            swords[randomSword].StartAttack(PlayerController.Instance.transform.position);
            nextAttackTime = Time.time + (isPhase2 ? phase2AttackCooldown : attackCooldown);
        }
    }
}
    private IEnumerator PlayAttackAnimation()
    {
        anim.SetBool("ataque", true);
        yield return new WaitForSeconds(1f);
        anim.SetBool("ataque", false);
    }
public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
{
    if (!isInvincible)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        StartCoroutine(ApplyKnockback(_hitDirection));
        StartCoroutine(InvincibilityFrames());
        
        if (healthBar != null)
        {
            healthBar.fillAmount = health / initialHealth;
        }
        
        if (!isPhase2 && health <= initialHealth / 2)
        {
            isPhase2 = true;
            StartCoroutine(SpawnPhase2SwordsSequence());
        }
    }
}
private IEnumerator ApplyKnockback(Vector2 direction)
{
    rb.velocity = Vector2.zero;
    float directionX = Mathf.Sign(direction.x);
    rb.AddForce(new Vector2(directionX * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);
    
    yield return new WaitForSeconds(knockbackDuration);
    
    rb.velocity = Vector2.zero;
}

private IEnumerator InvincibilityFrames()
{
    isInvincible = true;
    float flashInterval = 0.1f;
    
    for (float elapsed = 0; elapsed < invincibilityDuration; elapsed += flashInterval)
    {
        spriteRenderer.enabled = !spriteRenderer.enabled;
        yield return new WaitForSeconds(flashInterval);
    }
    
    spriteRenderer.enabled = true;
    isInvincible = false;
}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(startPosition, new Vector3(movementRange * 2, 1, 0));
        }
    }
}