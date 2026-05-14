using UnityEngine;
using UnityEngine.SceneManagement;

public class killBox : MonoBehaviour
{
    // 1. This runs if your BoxCollider2D has "Is Trigger" CHECKED (Player falls through it)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KillPlayer();
        }
    }

    // 2. This runs if your BoxCollider2D has "Is Trigger" UNCHECKED (Player bumps into it like a solid wall)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer();
        }
    }

    void KillPlayer()
    {
        Debug.Log("Player touched the Danger Box! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
