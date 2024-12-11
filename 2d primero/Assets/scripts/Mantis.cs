using UnityEngine;
using System.Collections;

public class Mantis : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 7f;

    private float nextDistanceCheck = 0f;
    [SerializeField] private float distanceCheckDelay = 0.2f; // Intervalo de chequeo


    [Header("Attack Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float recoveryTime = 1f;

    private bool isPreparingAttack = false;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool facingRight = true;
    private Animator anim;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();
        if (!isRecoiling && canAttack)
        {
            CheckPlayerDistance();
            if (player != null)
            {
                FacePlayer();
            }
        }
    }

    private void CheckPlayerDistance()
    {
        if (player == null) return;

        // Solo comprobar cada X segundos
        if (Time.time < nextDistanceCheck) return;
        nextDistanceCheck = Time.time + distanceCheckDelay;

        float distanceToPlayer = (player.position - transform.position).sqrMagnitude;
        float detectionRangeSqr = detectionRange * detectionRange;
        float attackRangeSqr = attackRange * attackRange;
        
        if (distanceToPlayer <= detectionRangeSqr && !isPreparingAttack && !isAttacking)
        {
            FacePlayer();
            isPreparingAttack = true;
            anim.SetBool("preparar", true);
        }
        
        if (distanceToPlayer <= attackRangeSqr && isPreparingAttack && !isAttacking)
        {
            StartCoroutine(DashAttack());
        }
    }

    private void FacePlayer()
    {
        facingRight = player.position.x > transform.position.x;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * (facingRight ? 1 : -1);
        transform.localScale = newScale;
    }

    private IEnumerator DashAttack()
    {
        isPreparingAttack = false;
        isAttacking = true;
        canAttack = false;
        anim.SetBool("preparar", true);

        yield return new WaitForSeconds(0.7f);

        anim.SetBool("preparar", false);
        anim.SetBool("atacar", true);

        float direction = facingRight ? 1f : -1f;
        
        float dashTimer = 0;
        while (dashTimer < dashDuration)
        {
            rb.velocity = new Vector2(direction * dashSpeed, 0f);
            dashTimer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        isAttacking = false;
        anim.SetBool("atacar", false);
        anim.SetBool("preparar", false);


        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
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
        base.Attack();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}