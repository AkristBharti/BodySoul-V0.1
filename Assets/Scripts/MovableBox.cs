using UnityEngine;
using System.Collections;

public class MovableBox : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 slideOffset = new Vector2(-5f, 0f); // Moves left by 5 units
    public float moveSpeed = 5f;

    [Header("Return Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Coroutine returnCoroutine;

    void Start()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void MoveBox()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        targetPosition = originalPosition + slideOffset;
    }

    public void StartReturnSequence()
    {
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
    }
}