using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Enemy
{
    private bool movingRight = true;
    private float lastFlipTime;
    [SerializeField] private float flipCooldown = 0.5f;
    private Animator anim;
    
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 12f;
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        
        if(!isRecoiling)
        {
            // Ground check
            bool groundAhead = Physics2D.Raycast(
                transform.position, 
                Vector2.down, 
                groundCheckDistance, 
                groundLayer
            );

            if (groundAhead)
            {
                // Wall check
                bool wallAhead = Physics2D.Raycast(
                    transform.position,
                    movingRight ? Vector2.right : Vector2.left,
                    wallCheckDistance,
                    groundLayer
                );

                // Check for ground ahead
                Vector2 groundCheckPosition = transform.position + (Vector3)(movingRight ? Vector2.right : Vector2.left) * 0.5f;
                bool groundInFront = Physics2D.Raycast(
                    groundCheckPosition,
                    Vector2.down,
                    groundCheckDistance,
                    groundLayer
                );

                if (wallAhead || !groundInFront)
                {
                    if (Time.time >= lastFlipTime + flipCooldown)
                    {
                        Flip();
                        lastFlipTime = Time.time;
                    }
                }

                // Move zombie
                float direction = movingRight ? 1f : -1f;
                rb.velocity = new Vector2(speed * direction, rb.velocity.y);
                anim.SetBool("Andando", true);
            }
            else
            {
                // Stop movement if no ground
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (Time.time >= lastFlipTime + flipCooldown)
                {
                    Flip();
                    lastFlipTime = Time.time;
                }
                anim.SetBool("Andando", false);
            }
        }
        else
        {
            anim.SetBool("Andando", false);
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnDrawGizmos()
    {
        // Visualizar rayos de detección
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(transform.position, 
            (movingRight ? Vector2.right : Vector2.left) * wallCheckDistance);
    }
}