using UnityEngine;
using System.Collections;

public class MagneticPlatform : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float pullSpeed = 10f;
    public Vector2 hoverOffset = new Vector2(0f, -1.5f);

    [Header("Movement Bounds")]
    [Tooltip("The platform will not be allowed to leave this area.")]
    public Collider2D barrierArea;

    [Header("Return Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    private bool isMagnetized = false;
    private Transform soulTransform;

    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Coroutine returnCoroutine;

    void Start()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition;

        // AUTO-LINK: If you forgot to assign the barrier in the Inspector, 
        // the platform will search for a Barrier touching it when the game starts!
        if (barrierArea == null)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Barrier"))
                {
                    barrierArea = col;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (isMagnetized && soulTransform != null)
        {
            // 1. Where does the platform WANT to go?
            Vector2 desiredPos = (Vector2)soulTransform.position + hoverOffset;

            // 2. Is it allowed to go there? 
            if (barrierArea != null)
            {
                // This magic Unity function forces the coordinates to stay completely inside the Barrier
                desiredPos = barrierArea.ClosestPoint(desiredPos);
            }

            // 3. Move smoothly to the restricted position
            transform.position = Vector2.MoveTowards(transform.position, desiredPos, pullSpeed * Time.deltaTime);
        }
        else
        {
            // Move back to starting position or stay frozen
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, pullSpeed * Time.deltaTime);
        }
    }

    // --- The Magnet Controls ---

    public void MagnetizeTo(Transform soul)
    {
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

        targetPosition = transform.position;
        returnCoroutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(3f);

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
        targetPosition = originalPosition;
        returnCoroutine = null;
    }

    // --- The "Sticky" Player Logic ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}