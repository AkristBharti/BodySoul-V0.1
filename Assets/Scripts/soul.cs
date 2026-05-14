using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class SoulController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject player;
    public float floatSpeed = 8f;
    public float returnSpeed = 15f;

    [Header("Pop Out Settings")]
    public float popOutSpeed = 12f;
    public Vector2 popOutOffset = new Vector2(0f, 1.5f);

    [Header("Ability Settings")]
    public float abilityRadius = 3.5f;
    public float magnetRadius = 5.0f; // NEW: How close you need to be to grab the platform

    private Rigidbody2D soulRb;
    private player playerMovementScript;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;

    private bool isDetached = false;
    private bool isReturning = false;
    private bool isPoppingOut = false;
    private Vector2 popOutTarget;

    // Memory Lists
    private List<MovableGround> liftedBlocks = new List<MovableGround>();
    private List<MovableBox> movedBoxes = new List<MovableBox>();

    // NEW: Remembers the specific platform we are currently carrying
    private MagneticPlatform currentlyHeldPlatform;

    void Start()
    {
        soulRb = GetComponent<Rigidbody2D>();
        playerMovementScript = player.GetComponent<player>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && isReturning == false && isPoppingOut == false)
        {
            ToggleSoul();
        }

        // --- M KEY: Lift Ground ---
        if (isDetached && Input.GetKeyDown(KeyCode.M))
        {
            UseLiftAbility();
        }

        // --- N KEY: Magnetize Platform ---
        if (isDetached && Input.GetKeyDown(KeyCode.N))
        {
            ToggleMagnet();
        }

        if (isPoppingOut)
        {
            soulRb.velocity = Vector2.zero;
            transform.position = Vector2.MoveTowards(transform.position, popOutTarget, popOutSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, popOutTarget) < 0.05f)
            {
                isPoppingOut = false;
                isDetached = true;
            }
        }
        else if (isDetached)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            soulRb.velocity = new Vector2(moveX, moveY).normalized * floatSpeed;
        }
        else if (isReturning)
        {
            soulRb.velocity = Vector2.zero;
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, returnSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.transform.position) < 0.05f)
            {
                isReturning = false;
                transform.position = player.transform.position;
                playerMovementScript.enabled = true;

                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("soulBack");
                }

                foreach (MovableGround block in liftedBlocks) { block.StartDropSequence(); }
                liftedBlocks.Clear();

                foreach (MovableBox box in movedBoxes) { box.StartReturnSequence(); }
                movedBoxes.Clear();
            }
        }
        else
        {
            soulRb.velocity = Vector2.zero;
            transform.position = player.transform.position;
        }
    }

    void ToggleSoul()
    {
        if (isDetached == false)
        {
            isPoppingOut = true;
            popOutTarget = new Vector2(player.transform.position.x, player.transform.position.y) + popOutOffset;
            playerMovementScript.enabled = false;
            playerRb.velocity = Vector2.zero;

            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("soulOut");
            }
        }
        else
        {
            // ---> NEW: Drop the platform exactly where it is before flying back! <---
            if (currentlyHeldPlatform != null)
            {
                currentlyHeldPlatform.Drop();
                currentlyHeldPlatform = null;
            }

            isDetached = false;
            isReturning = true;
        }
    }

    void UseLiftAbility()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, abilityRadius);
        foreach (Collider2D obj in nearbyObjects)
        {
            if (obj.CompareTag("Bridge"))
            {
                MovableGround groundScript = obj.GetComponent<MovableGround>();
                if (groundScript != null && !liftedBlocks.Contains(groundScript))
                {
                    groundScript.LiftGround();
                    liftedBlocks.Add(groundScript);
                }
            }

            if (obj.CompareTag("UmbrellaBox"))
            {
                MovableBox boxScript = obj.GetComponent<MovableBox>();
                if (boxScript != null && !movedBoxes.Contains(boxScript))
                {
                    boxScript.MoveBox();
                    movedBoxes.Add(boxScript);
                }
            }
        }
    }

    // --- NEW: The Magnet Logic ---
    void ToggleMagnet()
    {
        // 1. If we are already holding something, drop it!
        if (currentlyHeldPlatform != null)
        {
            currentlyHeldPlatform.Drop();
            currentlyHeldPlatform = null;
            return; // Stop the function here so we don't accidentally pick it right back up
        }

        // 2. If we are NOT holding something, look for one
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, magnetRadius);

        foreach (Collider2D obj in nearbyObjects)
        {
            if (obj.CompareTag("FloatableObj"))
            {
                MagneticPlatform platformScript = obj.GetComponent<MagneticPlatform>();
                if (platformScript != null)
                {
                    // Grab the first one we find and break out of the loop
                    currentlyHeldPlatform = platformScript;
                    platformScript.MagnetizeTo(this.transform);
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Cyan circle for M ability
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, abilityRadius);

        // Yellow circle for N ability (Magnet)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
