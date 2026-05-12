using UnityEngine;
using System.Collections; // Required for Coroutines

public class MagneticPlatform : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float pullSpeed = 10f;
    public Vector2 hoverOffset = new Vector2(0f, -1.5f);

    [Header("Return Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    private bool isMagnetized = false;
    private Transform soulTransform;

    // Memory variables
    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Coroutine returnCoroutine;

    void Start()
    {
        // Remember exactly where we started so we can return here later
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    void Update()
    {
        if (isMagnetized && soulTransform != null)
        {
            // 1. If the soul is holding us, smoothly fly to the soul
            Vector2 targetPos = (Vector2)soulTransform.position + hoverOffset;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, pullSpeed * Time.deltaTime);
        }
        else
        {
            // 2. If the soul dropped us, smoothly slide to our target position (either holding still, or going home)
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, pullSpeed * Time.deltaTime);
        }
    }

    // --- The Magnet Controls ---

    public void MagnetizeTo(Transform soul)
    {
        // If we were in the middle of dropping/shaking, stop!
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        soulTransform = soul;
        isMagnetized = true;
    }

    public void Drop()
    {
        isMagnetized = false;
        soulTransform = null;

        // Lock our current position as the target so we freeze in mid-air temporarily
        targetPosition = transform.position;

        // Start the 3-second timer and shake sequence
        returnCoroutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        // 1. Wait for 3 seconds in mid-air so the player has time to cross
        yield return new WaitForSeconds(3f);

        // 2. The Shake!
        float elapsed = 0f;
        Vector2 currentPos = transform.position;

        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float yOffset = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = new Vector2(currentPos.x + xOffset, currentPos.y + yOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = currentPos;

        // 3. Set the target back to home so the Update() function slides us back
        targetPosition = originalPosition;
        returnCoroutine = null;
    }

    // --- The "Sticky" Player Logic ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the player lands on us, make the player a "Child" of this platform
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the player walks or jumps off, un-parent them so they are free again
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}