using UnityEngine;
using System.Collections; // Required for Coroutines

public class MovableGround : MonoBehaviour
{
    [Header("Settings")]
    public float liftHeight = 3f;
    public float moveSpeed = 5f;

    [Header("Drop Settings")]
    public float shakeDuration = 0.5f;   // How long it trembles before falling
    public float shakeMagnitude = 0.1f;  // How violent the shake is

    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Coroutine dropCoroutine;

    void Start()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition; // Start at the bottom
    }

    void Update()
    {
        // Always smoothly move towards the target position (whether that's up or down)
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void LiftGround()
    {
        // If we were currently dropping, stop!
        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
        }

        // Set the target to the high position
        targetPosition = originalPosition + new Vector2(0f, liftHeight);
    }

    public void StartDropSequence()
    {
        dropCoroutine = StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        // 1. Wait for 3 seconds
        yield return new WaitForSeconds(2f);

        // 2. The Shake!
        float elapsed = 0f;
        Vector2 currentPos = transform.position; // Remember exactly where we are hovering

        while (elapsed < shakeDuration)
        {
            // Pick a random tiny offset
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float yOffset = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = new Vector2(currentPos.x + xOffset, currentPos.y + yOffset);

            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        // Snap back to the center after shaking
        transform.position = currentPos;

        // 3. Set the target back to the ground so Update() will slide it down
        targetPosition = originalPosition;
    }
}