using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In Estatua.cs
public class estatua : Enemy
{
    [Header("Statue Settings")]
    [SerializeField] private float manaRestoreAmount = 0.1f;
    [SerializeField] private bool isDestroyed = false;

    protected override void Start()
    {
        base.Start();
        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.isTrigger = true;
        }
        rb.gravityScale = 0f;
        rb.isKinematic = true;
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        if (!isDestroyed)
        {
            base.EnemyHit(_damageDone, _hitDirection, _hitForce);
            PlayerController.Instance.RestoreMana(manaRestoreAmount);
        }
    }

    protected override void OnTriggerStay2D(Collider2D _other)
    {
        // Override to do nothing - statues don't interact on trigger
    }

    protected override void Update()
    {
        if (health <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    protected override void Attack()
    {
        // Statues don't attack
    }
}