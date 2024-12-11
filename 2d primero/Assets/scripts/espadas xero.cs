using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EspadasXero.cs
public class espadasxero : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackSpeed = 15f;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackExtendDistance = 3f; // Add this field

    [SerializeField] private float swordOffset = 2f; // Add this field
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second
        [SerializeField] private float idleRotationSpeed = 1000f; // Add faster rotation for idle position


    private Vector3 originalTargetPos;
    private Vector3 extendedTargetPos;  
    private Vector3 homePosition;
    private Vector3 targetPosition;
    private bool isAttacking;
    private bool isReturning;
    private Rigidbody2D rb;
    private Transform xeroTransform;
    public bool IsAttacking => isAttacking;
private float currentOffset;

        private Vector3 targetHomePosition;
    private bool isRepositioning = false;
    private float repositionSpeed = 5f;

    private bool isLeftSword; // Add this to track sword position


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        xeroTransform = transform.parent;
        SetupRigidbody();
        homePosition = transform.position;
        transform.rotation = Quaternion.Euler(0, 0, -90);

        // Determine if this is left or right sword based on initial position
        isLeftSword = transform.position.x < xeroTransform.position.x;
    }

private void SetupRigidbody()
{
    rb.gravityScale = 0f;
    rb.isKinematic = true;
    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Fixed property name
    if (TryGetComponent<Collider2D>(out Collider2D collider))
    {
        collider.isTrigger = true;
    }
}

    private void Update()
    {
        if (isAttacking)
        {
            MoveToTarget();
        }
        else if (isReturning)
        {
            ReturnHome();
        }
        else if (isRepositioning)
        {
            RepositionToNewOffset();
        }
    }


    private void RepositionToNewOffset()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetHomePosition, repositionSpeed * Time.deltaTime);
        
        // Calculate rotation to face movement direction
        Vector2 direction = (targetHomePosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        float currentAngle = transform.rotation.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);

        if (Vector3.Distance(transform.position, targetHomePosition) < 0.1f)
        {
            isRepositioning = false;
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
    }
// In EspadasXero.cs, modify UpdatePosition():
public void UpdatePosition(Vector3 newPosition)
{
    if (!isAttacking && !isReturning)
    {
        if (xeroTransform != null)
        {
            Vector3 targetPos = xeroTransform.position + (isLeftSword ? Vector3.left : Vector3.right) * currentOffset;
            newPosition = new Vector3(targetPos.x, xeroTransform.position.y, targetPos.z);
            transform.position = newPosition;
            homePosition = newPosition;
        }
        else
        {
            Debug.LogWarning("Sword parent (Xero) reference is missing!");
            xeroTransform = transform.parent;
        }
    }
}

    public void StartAttack(Vector3 targetPos)
    {
        if (!isAttacking && !isReturning)
        {
            originalTargetPos = targetPos;
            
            // Calculate extended target position
            Vector2 direction = (targetPos - transform.position).normalized;
            extendedTargetPos = targetPos + (Vector3)(direction * attackExtendDistance);
            
            targetPosition = extendedTargetPos;
            isAttacking = true;
            isReturning = false;
        }
    }

private void MoveToTarget()
{
    transform.position = Vector3.MoveTowards(transform.position, targetPosition, attackSpeed * Time.deltaTime);
    
    // Calculate desired rotation to target
    Vector2 direction = (targetPosition - transform.position).normalized;
    float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
    // Smooth rotation
    float currentAngle = transform.rotation.eulerAngles.z;
    float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
    transform.rotation = Quaternion.Euler(0, 0, newAngle);
    
    // Only switch to returning when reaching end of trajectory
    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
    {
        isAttacking = false;
        isReturning = true;
    }
}

    private void ReturnHome()
    {
        if (xeroTransform != null)
        {
            Vector3 offset = isLeftSword ? Vector3.left : Vector3.right;
            Vector3 targetPosition = xeroTransform.position + (offset * currentOffset);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, returnSpeed * Time.deltaTime);
            
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            float targetAngle;
            float currentRotationSpeed;
            
            if (distanceToTarget > 0.1f)
            {
                // Use normal rotation speed while returning
                Vector2 direction = (targetPosition - transform.position).normalized;
                targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                currentRotationSpeed = rotationSpeed;
            }
            else
            {
                // Use faster rotation when getting to idle position
                targetAngle = -90f;
                currentRotationSpeed = idleRotationSpeed;
            }
            
            float currentAngle = transform.rotation.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, currentRotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
            
            if (distanceToTarget < 0.1f && Mathf.Abs(newAngle - (-90f)) < 0.1f)
            {
                isReturning = false;
                homePosition = targetPosition;
            }
        }
    }
public void Initialize(Transform parentTransform, float offset, bool isLeft)
{
    xeroTransform = parentTransform;
    currentOffset = offset; // Store the correct offset
    isLeftSword = isLeft;
    homePosition = transform.position;
    Debug.Log($"Inicializando espada: {(isLeft ? "Izquierda" : "Derecha")} con offset {offset}");
}

    public void MoveToNewOffset(Vector3 newPosition)
    {
        if (xeroTransform != null)
        {
            targetHomePosition = newPosition;
            homePosition = targetHomePosition;
            isRepositioning = true;
            
            // Update left/right status based on new position
            isLeftSword = newPosition.x < xeroTransform.position.x;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")  && !PlayerController.Instance.pState.invincible)
        {
            PlayerController.Instance.TakeDamage(damage, transform.position);
            PlayerController.Instance.HitStopTime(0.005f, 20, 0.1f);
        }
    }
}