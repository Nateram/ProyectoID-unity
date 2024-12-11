using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In bench.cs
public class bench : MonoBehaviour
{
    public bool interacted;
    private bool playerInRange;
    private bool playerIsSitting;
    private PlayerController player;
    private Animator playerAnimator;

    void Start()
    {
        interacted = false;
        playerInRange = false;
        playerIsSitting = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetButtonDown("Interactuar") && !playerIsSitting)
        {
            interacted = true;
            playerIsSitting = true;
            Debug.Log($"Bench {gameObject.name} interacted");
            GameManager.Instance.SetCurrentBench(this);
            playerAnimator.SetBool("sentarse", true);
        }

        // Check for any input to exit sitting state
        if (playerIsSitting && (
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0 ||
            Input.GetButtonDown("Jump") ||
            Input.GetButtonDown("Attack") ||
            Input.GetButtonDown("Dash") ||
            Input.GetButton("Cast/Heal")))
        {
            playerIsSitting = false;
            playerAnimator.SetBool("sentarse", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            player = _collision.gameObject.GetComponent<PlayerController>();
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            playerIsSitting = false;
            if (playerAnimator != null)
                playerAnimator.SetBool("sentarse", false);
        }
    }
}