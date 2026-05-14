using UnityEngine;
using UnityEngine.SceneManagement;

public class AcidRainParticleHazard : MonoBehaviour
{
    // This built-in Unity function fires every time a particle hits a 2D Collider
    private void OnParticleCollision(GameObject other)
    {
        // Did the particle touch the player?
        if (other.CompareTag("Player"))
        {
            Debug.Log("A rain drop hit the player!");
            KillPlayer();
        }
    }

    void KillPlayer()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
